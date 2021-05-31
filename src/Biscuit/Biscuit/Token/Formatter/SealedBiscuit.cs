using Biscuit.Errors;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Biscuit.Token.Formatter
{
    public class SealedBiscuit
    {
        public byte[] Authority { get; }
        public List<byte[]> Blocks { get; }

        private readonly byte[] signature;

        /// <summary>
        /// Deserializes a SealedBiscuit from a byte array
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        static public Either<Error, SealedBiscuit> FromBytes(byte[] slice, byte[] secret)
        {
            try
            {
                Format.Schema.SealedBiscuit data = Format.Schema.SealedBiscuit.Parser.ParseFrom(slice);
                using (HMACSHA256 hmac = new HMACSHA256(secret))
                {
                    byte[] authority = data.Authority.ToByteArray();

                    List<byte> toHash = new List<byte>(authority);

                    List<byte[]> blocks = new List<byte[]>();
                    foreach (ByteString block in data.Blocks)
                    {
                        byte[] byteBlock = block.ToByteArray();
                        blocks.Add(byteBlock);
                        toHash.AddRange(byteBlock);
                    }

                    byte[] calculated = hmac.ComputeHash(toHash.ToArray());
                    byte[] signature = data.Signature.ToByteArray();

                    if (calculated.Length != signature.Length)
                    {
                        return new InvalidFormat();
                    }

                    int result = 0;
                    for (int i = 0; i < calculated.Length; i++)
                    {
                        result |= calculated[i] ^ signature[i];
                    }

                    if (result != 0)
                    {
                        return new SealedSignature();
                    }

                    return new SealedBiscuit(authority, blocks, signature);
                }
            }
            catch (Exception e)
            {
                return new DeserializationError(e.ToString());
            }
        }

        /// <summary>
        /// Serializes a SealedBiscuit to a byte array
        /// </summary>
        /// <returns></returns>
        public Either<FormatError, byte[]> Serialize()
        {
            Format.Schema.SealedBiscuit biscuit = new Format.Schema.SealedBiscuit()
            {
                Signature = ByteString.CopyFrom(signature),
                Authority = ByteString.CopyFrom(Authority)
            };

            foreach (var block in Blocks)
            {
                biscuit.Blocks.Add(ByteString.CopyFrom(block));
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    biscuit.WriteTo(stream);
                    byte[] data = stream.ToArray();
                    return data;
                }
            }
            catch (IOException e)
            {
                return new SerializationError(e.ToString());
            }

        }

        SealedBiscuit(byte[] authority, List<byte[]> blocks, byte[] signature)
        {
            this.Authority = authority;
            this.Blocks = blocks;
            this.signature = signature;
        }

        public static Either<FormatError, SealedBiscuit> Make(Block authority, List<Block> blocks, byte[] secret)
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

        public List<byte[]> RevocationIdentifiers()
        {
            List<byte[]> l = new List<byte[]>();

            try
            {
                var dataToCompute = new List<byte>(this.Authority);

                using (var sha = SHA256.Create())
                {
                    var computedHash = sha.ComputeHash(dataToCompute.ToArray());
                    l.Add(computedHash);
                }

                foreach (byte[] block in this.Blocks)
                {
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
            }

            return l;
        }
    }
}
