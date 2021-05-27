using Biscuit.Errors;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Biscuit.Token.Formatter
{
    public class SealedBiscuit
    {
        public byte[] authority;
        public List<byte[]> blocks;
        public byte[] signature;

        /**
         * Deserializes a SealedBiscuit from a byte array
         * @param slice
         * @return
         */
        static public Either<Error, SealedBiscuit> from_bytes(byte[] slice, byte[] secret)
        {
            try
            {
                Format.Schema.SealedBiscuit data = Format.Schema.SealedBiscuit.Parser.ParseFrom(slice);
                using (HMACSHA256 hmac = new HMACSHA256(secret))
                {
                    byte[] authority = data.Authority.ToByteArray();

                    List<byte> toHash = new List<byte>(authority);

                    //Mac sha256_HMAC = Mac.getInstance("HmacSHA256");
                    //SecretKeySpec secret_key = new SecretKeySpec(secret, "HmacSHA256");
                    //sha256_HMAC.init(secret_key);
                    //sha256_HMAC.update(authority);

                    List<byte[]> blocks = new List<byte[]>();
                    foreach (ByteString block in data.Blocks)
                    {
                        byte[] byteBlock = block.ToByteArray();
                        blocks.Add(byteBlock);
                        toHash.AddRange(byteBlock);
                        //sha256_HMAC.update(byteBlock);
                    }

                    byte[] calculated = hmac.ComputeHash(toHash.ToArray());
                    byte[] signature = data.Signature.ToByteArray();

                    if (calculated.Length != signature.Length)
                    {
                        return new Left(new InvalidFormat());
                    }

                    int result = 0;
                    for (int i = 0; i < calculated.Length; i++)
                    {
                        result |= calculated[i] ^ signature[i];
                    }

                    if (result != 0)
                    {
                        return new Left(new SealedSignature());
                    }

                    SealedBiscuit sealedBiscuit = new SealedBiscuit(authority, blocks, signature);
                    return new Right(sealedBiscuit);
                }
            }
            catch (Exception e)
            {
                return new Left(new DeserializationError(e.ToString()));
            }
        }

        /**
         * Serializes a SealedBiscuit to a byte array
         * @return
         */
        public Either<FormatError, byte[]> serialize()
        {
            Format.Schema.SealedBiscuit biscuit = new Format.Schema.SealedBiscuit()
            {
                Signature = ByteString.CopyFrom(signature),
                Authority = ByteString.CopyFrom(authority)
            };

            foreach (var block in blocks)
            {
                biscuit.Blocks.Add(ByteString.CopyFrom(block));
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

        SealedBiscuit(byte[] authority, List<byte[]> blocks, byte[] signature)
        {
            this.authority = authority;
            this.blocks = blocks;
            this.signature = signature;
        }

        public static Either<FormatError, SealedBiscuit> make(Block authority, List<Block> blocks, byte[] secret)
        {

            try
            {
                using (HMACSHA256 hmac = new HMACSHA256(secret))
                {
                    Format.Schema.Block b = authority.serialize();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        b.WriteTo(stream);
                        byte[] authorityData = stream.ToArray();

                        List<byte> toHash = new List<byte>(authorityData);

                        List<byte[]> blocksData = new List<byte[]>();
                        foreach (Block bl in blocks)
                        {
                            Format.Schema.Block b2 = bl.serialize();
                            using (MemoryStream stream2 = new MemoryStream())
                            {
                                b2.WriteTo(stream2);
                                toHash.AddRange(stream2.ToArray());
                                blocksData.Add(stream2.ToArray());
                            }
                        }

                        byte[] signature = hmac.ComputeHash(toHash.ToArray());// sha256_HMAC.doFinal();
                        return new Right(new SealedBiscuit(authorityData, blocksData, signature));
                    }
                }
            }
            catch (Exception e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }

        public List<byte[]> revocation_identifiers()
        {
            List<byte[]> l = new List<byte[]>();

            try
            {
                var dataToCompute = new List<byte>(this.authority);

                using (var sha = SHA256.Create())
                {
                    var computedHash = sha.ComputeHash(dataToCompute.ToArray());
                    l.Add(computedHash);
                }

                for (int i = 0; i < this.blocks.Count; i++)
                {
                    byte[] block = this.blocks[i];
                    dataToCompute.AddRange(block);

                    using (var sha = SHA256.Create())
                    {
                        var computedHash = sha.ComputeHash(dataToCompute.ToArray());
                        l.Add(computedHash);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
                //e.printStackTrace();
            }

            return l;
        }
    }
}
