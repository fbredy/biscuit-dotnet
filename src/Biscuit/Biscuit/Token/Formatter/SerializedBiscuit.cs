using Biscuit.Crypto;
using Biscuit.Errors;
using Google.Protobuf;
using Ristretto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Biscuit.Token.Formatter
{
    public class SerializedBiscuit
    {
        public byte[] authority;
        public List<byte[]> blocks;
        public List<RistrettoElement> keys;
        public TokenSignature signature;

        public static uint MAX_SCHEMA_VERSION = 1;

        /**
         * Deserializes a SerializedBiscuit from a byte array
         * @param slice
         * @return
         */
        static public Either<Errors.Error, SerializedBiscuit> from_bytes(byte[] slice)
        {
            try
            {
                Format.Schema.Biscuit data = Format.Schema.Biscuit.Parser.ParseFrom(slice);

                List<RistrettoElement> keys = new List<RistrettoElement>();
                foreach (ByteString key in data.Keys)
                {
                    keys.Add(new CompressedRistretto(key.ToByteArray()).Decompress());
                }

                byte[] authority = data.Authority.ToByteArray();

                List<byte[]> blocks = new List<byte[]>();
                foreach (ByteString block in data.Blocks)
                {
                    blocks.Add(block.ToByteArray());
                }

                Either<Error, TokenSignature> signatureRes = TokenSignature.deserialize(data.Signature);

                if (signatureRes.IsLeft)
                {
                    Error e = signatureRes.Left;
                    return new Left(e);
                }

                TokenSignature signature = signatureRes.Right;

                SerializedBiscuit b = new SerializedBiscuit(authority, blocks, keys, signature);

                Either<Error, Void> res = b.verify();
                if (res.IsLeft)
                {
                    Error e = res.Left;
                    return new Left(e);
                }
                else
                {
                    return new Right(b);
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                return new Left(new DeserializationError(e.ToString()));
            }
            catch (InvalidEncodingException e)
            {
                return new Left(new DeserializationError(e.ToString()));
            }
        }

        /**
         * Serializes a SerializedBiscuit to a byte array
         * @return
         */
        public Either<Error, byte[]> serialize()
        {
            Format.Schema.Biscuit biscuit = new Format.Schema.Biscuit()
            {
                Signature = signature.serialize()
            };

            for (int i = 0; i < keys.Count; i++)
            {
                biscuit.Keys.Add(ByteString.CopyFrom(keys[i].Compress().ToByteArray()));
            }

            biscuit.Authority = ByteString.CopyFrom(authority);

            for (int i = 0; i < blocks.Count; i++)
            {
                biscuit.Blocks.Add(ByteString.CopyFrom(blocks[i]));
            }



            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    biscuit.WriteTo(stream);
                    byte[] data = stream.ToArray();
                    return new Right(data);
                }
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }

        }

        static public Either<FormatError, SerializedBiscuit> make(RNGCryptoServiceProvider rng, KeyPair root, Block authority)
        {
            Format.Schema.Block b = authority.serialize();
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    b.WriteTo(stream);
                    byte[] data = stream.ToArray();

                    TokenSignature signature = new TokenSignature(rng, root, data);
                    List<RistrettoElement> keys = new List<RistrettoElement>();
                    keys.Add(root.Public_key);

                    return new Right(new SerializedBiscuit(data, new List<byte[]>(), keys, signature));
                }
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }

        public Either<FormatError, SerializedBiscuit> append(RNGCryptoServiceProvider rng, KeyPair keypair, Block block)
        {
            Format.Schema.Block b = block.serialize();
            try
            {
                MemoryStream stream = new MemoryStream();
                b.WriteTo(stream);
                byte[] data = stream.ToArray();

                TokenSignature signature = this.signature.Sign(rng, keypair, data);

                List<RistrettoElement> keys = new List<RistrettoElement>();
                foreach (RistrettoElement key in this.keys)
                {
                    keys.Add(key);
                }
                keys.Add(keypair.Public_key);

                List<byte[]> blocks = new List<byte[]>();
                foreach (byte[] bl in this.blocks)
                {
                    blocks.Add(bl);
                }
                blocks.Add(data);

                return new Right(new SerializedBiscuit(authority, blocks, keys, signature));
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }

        public Either<Error, Void> verify()
        {
            if (keys.Count == 0)
            {
                return new Left(new EmptyKeys());
            }

            List<byte[]> blocks = new List<byte[]>();
            blocks.Add(authority);
            foreach (byte[] bl in this.blocks)
            {
                blocks.Add(bl);
            }

            return signature.Verify(keys, blocks);
        }

        public Either<Error, Void> check_root_key(PublicKey public_key)
        {
            if (keys.Count == 0)
            {
                return new Left(new EmptyKeys());
            }

            if (!keys[0].Equals(public_key.Key))
            {
                return new Left(new UnknownPublicKey());
            }

            return new Right(null);
        }

        public List<byte[]> revocation_identifiers()
        {
            List<byte[]> l = new List<byte[]>();

            try
            {
                var dataToCompute = authority.Concat(keys[0].Compress().ToByteArray()).ToArray();

                using (var sha = SHA256.Create())
                {
                    var computedHash = sha.ComputeHash(dataToCompute);
                    l.Add(computedHash);
                }
                
                //MessageDigest digest = MessageDigest.getInstance("SHA-256");
                //digest.update(authority);
                //digest.update(keys[0].Compress().ToByteArray());
                //MessageDigest cloned = (MessageDigest)digest.clone();
                //l.Add(digest.digest());

                //digest = cloned;

                for (int i = 0; i < blocks.Count; i++)
                {
                    dataToCompute = dataToCompute
                        .Concat(blocks[i])
                        .Concat(keys[i + 1].Compress().ToByteArray()).ToArray();

                    using (var sha = SHA256.Create())
                    {
                        l.Add(sha.ComputeHash(dataToCompute));
                    }
                    //byte[] block = blocks[i];
                    //digest.update(block);
                    //digest.update(keys[i + 1].Compress().ToByteArray());
                    //cloned = (MessageDigest)digest.clone();
                    //l.Add(digest.digest());

                    //digest = cloned;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
            }

            return l;
        }

        SerializedBiscuit(byte[] authority, List<byte[]> blocks, List<RistrettoElement> keys, TokenSignature signature)
        {
            this.authority = authority;
            this.blocks = blocks;
            this.keys = keys;
            this.signature = signature;
        }
    }

}
