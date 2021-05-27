using Google.Protobuf;
using Ristretto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Biscuit.Crypto
{
    /// <summary>
    /// Signature aggregation
    /// </summary>
    public class TokenSignature
    {
        List<RistrettoElement> parameters { get; }
        Scalar z { get; }

        /// <summary>
        /// Generates a new valid signature for a message and a private key
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="keypair"></param>
        /// <param name="message"></param>
        public TokenSignature(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            byte[] b = new byte[64];
            rng.GetBytes(b);
            Scalar r = Scalar.FromBytesModOrderWide(b);

            RistrettoElement A = Constants.RISTRETTO_GENERATOR.Multiply(r);
            List<RistrettoElement> l = new List<RistrettoElement>();
            l.Add(A);
            Scalar d = Hash_points(l);
            Scalar e = hash_message(keypair.Public_key, message);

            Scalar z = r.Multiply(d).Subtract(e.Multiply(keypair.Private_key));

            this.parameters = l;
            this.z = z;
        }

        TokenSignature(List<RistrettoElement> parameters, Scalar z)
        {
            this.parameters = parameters;
            this.z = z;
        }

        /// <summary>
        /// Generates a new valid signature from an existing one, a private key and a message
        /// </summary>
        /// <param name="rng"></param>
        /// <param name="keypair"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public TokenSignature Sign(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            byte[] b = new byte[64];
            rng.GetBytes(b);
            Scalar r = Scalar.FromBytesModOrderWide(b);

            RistrettoElement A = Constants.RISTRETTO_GENERATOR.Multiply(r);
            List<RistrettoElement> l = new List<RistrettoElement>();
            l.Add(A);
            Scalar d = Hash_points(l);
            Scalar e = hash_message(keypair.Public_key, message);

            Scalar z = r.Multiply(d).Subtract(e.Multiply(keypair.Private_key));

            TokenSignature sig = new TokenSignature(this.parameters, this.z.Add(z));
            sig.parameters.Add(A);

            return sig;
        }

        /// <summary>
        /// checks that a signature is valid for a set of public keys and messages
        /// </summary>
        /// <param name="public_keys"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Either<Errors.Error, Void> Verify(List<RistrettoElement> public_keys, List<byte[]> messages)
        {
            if (!(public_keys.Count() == messages.Count() && public_keys.Count() == this.parameters.Count()))
            {
                Console.WriteLine(("lists are not the same size"));
                return new Either<Errors.Error, Void>(new Errors.InvalidFormat());
            }

            //System.out.println("z, zp");
            RistrettoElement zP = Constants.RISTRETTO_GENERATOR.Multiply(this.z);
            //System.out.println(hex(z.toByteArray()));
            //System.out.println(hex(zP.compress().toByteArray()));


            //System.out.println("eiXi");
            RistrettoElement eiXi = RistrettoElement.IDENTITY;
            for (int i = 0; i < public_keys.Count(); i++)
            {
                Scalar e = hash_message(public_keys[i], messages[i]);
                //System.out.println(hex(e.toByteArray()));
                //System.out.println(hex((public_keys.get(i).multiply(e)).compress().toByteArray()));


                eiXi = eiXi.Add(public_keys[i].Multiply(e));
                //System.out.println(hex(eiXi.compress().toByteArray()));

            }

            //System.out.println("diAi");
            RistrettoElement diAi = RistrettoElement.IDENTITY;
            foreach (RistrettoElement A in parameters)
            {
                List<RistrettoElement> l = new List<RistrettoElement>();
                l.Add(A);
                Scalar d = Hash_points(l);

                diAi = diAi.Add(A.Multiply(d));
            }

            //System.out.println(hex(eiXi.compress().toByteArray()));
            //System.out.println(hex(diAi.compress().toByteArray()));



            RistrettoElement res = zP.Add(eiXi).Subtract(diAi);

            //System.out.println(hex(RistrettoElement.IDENTITY.compress().toByteArray()));
            //System.out.println(hex(res.compress().toByteArray()));

            if (res.Equals(RistrettoElement.IDENTITY))
            {
                return new Either<Errors.Error, Void>((Void)null);
            }
            else
            {
                return new Either<Errors.Error, Void>(new Errors.InvalidSignature());
            }
        }

        /**
         * Serializes a signature to its Protobuf representation
         * @return
         */
        public Format.Schema.Signature serialize()
        {
            Format.Schema.Signature sig = new Format.Schema.Signature()
            {
                Z = ByteString.CopyFrom(this.z.ToByteArray())
            };

            //System.out.println(this.parameters.size());
            for (int i = 0; i < this.parameters.Count; i++)
            {
                //System.out.println(i);
                sig.Parameters.Add(ByteString.CopyFrom(this.parameters[i].Compress().ToByteArray()));
            }

            return sig;
        }
        /**
         * Deserializes a signature from its Protobuf representation
         * @param sig
         * @return
         */
        static public Either<Errors.Error, TokenSignature> deserialize(Format.Schema.Signature sig)
        {
            try
            {
                List<RistrettoElement> parameters = new List<RistrettoElement>();
                foreach (ByteString parameter in sig.Parameters)
                {
                    parameters.Add((new CompressedRistretto(parameter.ToByteArray())).Decompress());
                }

                //System.out.println(hex(sig.getZ().toByteArray()));
                //System.out.println(sig.getZ().toByteArray().length);

                Scalar z = Scalar.FromBytesModOrder(sig.Z.ToByteArray());

                return new Right(new TokenSignature(parameters, z));
            }
            catch (InvalidEncodingException)
            {
                return new Left(new Errors.InvalidFormat());
            }
            catch (ArgumentException e)
            {
                return new Left(new Errors.DeserializationError(e.ToString()));
            }
        }

        static Scalar Hash_points(List<RistrettoElement> points)
        {
            try
            {
                using (var sha = SHA512.Create())
                {
                    sha.Initialize();
                    
                    var compressed = points.SelectMany(point => point.Compress().ToByteArray()).ToArray();
                    byte[] hashed = sha.ComputeHash(compressed);
                    return Scalar.FromBytesModOrderWide(hashed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }

        static Scalar hash_message(RistrettoElement point, byte[] data)
        {
            try
            {
                using (var sha = SHA512.Create())
                {
                    sha.Initialize();

                    var combined = point.Compress().ToByteArray().Concat(data).ToArray();
                    byte[] hashed = sha.ComputeHash(combined);
                    return Scalar.FromBytesModOrderWide(hashed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }

        //public static string Hex(byte[] byteArray)
        //{
        //    StringBuilder result = new StringBuilder();
        //    foreach (byte bb in byteArray)
        //    {
        //        result.Append(string.format("%02X", bb));
        //    }
        //    return result.ToString();
        //}

        //public static byte[] FromHex(string s)
        //{
        //    int len = s.Length;
        //    byte[] data = new byte[len / 2];
        //    for (int i = 0; i < len; i += 2)
        //    {
        //        data[i / 2] = (byte)((Character.digit(s.charAt(i), 16) << 4)
        //                + Character.digit(s.charAt(i + 1), 16));
        //    }
        //    return data;
        //}
    }
}
