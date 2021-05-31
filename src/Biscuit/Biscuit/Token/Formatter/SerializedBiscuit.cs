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

        /// <summary>
        /// Deserializes a SerializedBiscuit from a byte array
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        static public Either<Errors.Error, SerializedBiscuit> FromBytes(byte[] slice)
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

                Either<Error, TokenSignature> signatureRes = TokenSignature.Deserialize(data.Signature);

                if (signatureRes.IsLeft)
                {
                    return signatureRes.Left;
                }

                TokenSignature signature = signatureRes.Right;

                SerializedBiscuit b = new SerializedBiscuit(authority, blocks, keys, signature);

                Either<Error, Void> res = b.Verify();
                if (res.IsLeft)
                {
                    return res.Left;
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

        /// <summary>
        /// Serializes a SerializedBiscuit to a byte array
        /// </summary>
        /// <returns></returns>
        public Either<Error, byte[]> Serialize()
        {
            Format.Schema.Biscuit biscuit = new Format.Schema.Biscuit()
            {
                Signature = signature.Serialize()
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

        static public Either<FormatError, SerializedBiscuit> Make(RNGCryptoServiceProvider rng, KeyPair root, Block authority)
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
                    keys.Add(root.PublicKey);

                    return new SerializedBiscuit(data, new List<byte[]>(), keys, signature);
                }
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }

        public Either<FormatError, SerializedBiscuit> Append(RNGCryptoServiceProvider rng, KeyPair keypair, Block block)
        {
            Format.Schema.Block b = block.serialize();
            try
            {
                MemoryStream stream = new MemoryStream();
                b.WriteTo(stream);
                byte[] data = stream.ToArray();

                TokenSignature signature = this.signature.Sign(rng, keypair, data);

                List<RistrettoElement> keys = new List<RistrettoElement>();
                keys.AddRange(this.keys);
                keys.Add(keypair.PublicKey);

                List<byte[]> blocks = new List<byte[]>();
                blocks.AddRange(this.blocks);
                blocks.Add(data);

                return new Right(new SerializedBiscuit(authority, blocks, keys, signature));
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }

        public Either<Error, Void> Verify()
        {
            if (keys.Count == 0)
            {
                return new Left(new EmptyKeys());
            }

            List<byte[]> blocks = new List<byte[]>
            {
                authority
            };
            blocks.AddRange(this.blocks);

            return signature.Verify(keys, blocks);
        }

        public Either<Error, Void> CheckRootKey(PublicKey public_key)
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

        public List<byte[]> RevocationIdentifiers()
        {
            List<byte[]> result = new List<byte[]>();

            try
            {
                var dataToCompute = authority.Concat(keys[0].Compress().ToByteArray()).ToArray();

                using (var sha = SHA256.Create())
                {
                    var computedHash = sha.ComputeHash(dataToCompute);
                    result.Add(computedHash);
                }
                
                for (int i = 0; i < blocks.Count; i++)
                {
                    dataToCompute = dataToCompute
                        .Concat(blocks[i])
                        .Concat(keys[i + 1].Compress().ToByteArray()).ToArray();

                    using (var sha = SHA256.Create())
                    {
                        result.Add(sha.ComputeHash(dataToCompute));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
            }

            return result;
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
