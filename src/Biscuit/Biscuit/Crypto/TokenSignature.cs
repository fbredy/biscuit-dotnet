using Biscuit.Errors;
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
        private List<RistrettoElement> Parameters { get; }
        private Scalar z { get; }

        /// <summary>
        /// Generates a new valid signature for a message and a private key
        /// </summary>
        /// <param name="rng">random number generator</param>
        /// <param name="keypair"></param>
        /// <param name="message"></param>
        public TokenSignature(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            byte[] randomData = new byte[64];
            rng.GetBytes(randomData);
            Scalar r = Scalar.FromBytesModOrderWide(randomData);

            var ristrettoElements = new List<RistrettoElement>
            {
                Constants.RISTRETTO_GENERATOR.Multiply(r)
            };
            Scalar d = HashPoints(ristrettoElements);
            Scalar e = HashMessage(keypair.PublicKey, message);
            Scalar z = r.Multiply(d).Subtract(e.Multiply(keypair.PrivateKey));

            this.Parameters = ristrettoElements;
            this.z = z;
        }

        TokenSignature(List<RistrettoElement> parameters, Scalar z)
        {
            this.Parameters = parameters;
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
            byte[] randomData = new byte[64];
            rng.GetBytes(randomData);
            Scalar r = Scalar.FromBytesModOrderWide(randomData);

            var ristretto = Constants.RISTRETTO_GENERATOR.Multiply(r);
            var ristrettoElements = new List<RistrettoElement> { ristretto };
            Scalar d = HashPoints(ristrettoElements);
            Scalar e = HashMessage(keypair.PublicKey, message);
            Scalar z = r.Multiply(d).Subtract(e.Multiply(keypair.PrivateKey));

            TokenSignature sig = new TokenSignature(this.Parameters, this.z.Add(z));
            sig.Parameters.Add(ristretto);

            return sig;
        }

        /// <summary>
        /// checks that a signature is valid for a set of public keys and messages
        /// </summary>
        /// <param name="publicKeys"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Either<Error, Void> Verify(IList<RistrettoElement> publicKeys, IList<byte[]> messages)
        {
            if (!(publicKeys.Count() == messages.Count() && publicKeys.Count() == this.Parameters.Count()))
            {
                return new Either<Error, Void>(new InvalidFormat());
            }

            RistrettoElement zP = Constants.RISTRETTO_GENERATOR.Multiply(this.z);

            RistrettoElement eiXi = RistrettoElement.IDENTITY;
            for (int i = 0; i < publicKeys.Count(); i++)
            {
                Scalar e = HashMessage(publicKeys[i], messages[i]);
                eiXi = eiXi.Add(publicKeys[i].Multiply(e));
            }

            RistrettoElement diAi = RistrettoElement.IDENTITY;
            foreach (RistrettoElement item in Parameters)
            {
                List<RistrettoElement> ristrettoElements = new List<RistrettoElement> { item };
                
                Scalar d = HashPoints(ristrettoElements);

                diAi = diAi.Add(item.Multiply(d));
            }

            RistrettoElement res = zP.Add(eiXi).Subtract(diAi);

            if (res.Equals(RistrettoElement.IDENTITY))
            {
                return new Right((Void)null);
            }
            else
            {
                return new InvalidSignature();
            }
        }

        /// <summary>
        /// Serializes a signature to its Protobuf representation
        /// </summary>
        /// <returns></returns>
        public Format.Schema.Signature Serialize()
        {
            Format.Schema.Signature sig = new Format.Schema.Signature()
            {
                Z = ByteString.CopyFrom(this.z.ToByteArray())
            };

            foreach (RistrettoElement element in this.Parameters)
            {
                sig.Parameters.Add(ByteString.CopyFrom(element.Compress().ToByteArray()));
            }

            return sig;
        }

        /// <summary>
        /// Deserializes a signature from its Protobuf representation
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        static public Either<Error, TokenSignature> Deserialize(Format.Schema.Signature sig)
        {
            try
            {
                List<RistrettoElement> parameters = new List<RistrettoElement>();
                foreach (ByteString parameter in sig.Parameters)
                {
                    parameters.Add((new CompressedRistretto(parameter.ToByteArray())).Decompress());
                }

                Scalar z = Scalar.FromBytesModOrder(sig.Z.ToByteArray());

                return new Right(new TokenSignature(parameters, z));
            }
            catch (InvalidEncodingException)
            {
                return new InvalidFormat();
            }
            catch (ArgumentException e)
            {
                return new DeserializationError(e.ToString());
            }
        }

        static Scalar HashPoints(List<RistrettoElement> points)
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

        static Scalar HashMessage(RistrettoElement point, byte[] data)
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
    }
}
