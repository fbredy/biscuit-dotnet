using Ristretto;
using System.Security.Cryptography;

namespace Biscuit.Crypto
{
    /// <summary>
    /// Private and public key
    /// </summary>
    public sealed class KeyPair
    {
        public Scalar PrivateKey { get; }

        public RistrettoElement PublicKey { get; }

        public KeyPair(RNGCryptoServiceProvider rng)
        {
            byte[] b = new byte[64];
            rng.GetBytes(b);
            this.PrivateKey = Scalar.FromBytesModOrderWide(b);
            this.PublicKey = Constants.RISTRETTO_GENERATOR.Multiply(this.PrivateKey);
        }

        public KeyPair(string hex)
        {
            byte[] b = StrUtils.hexToBytes(hex);
            this.PrivateKey = Scalar.FromBytesModOrder(b);
            this.PublicKey = Constants.RISTRETTO_GENERATOR.Multiply(this.PrivateKey);
        }

        public KeyPair(byte[] b)
        {
            this.PrivateKey = Scalar.FromBytesModOrderWide(b);
            this.PublicKey = Constants.RISTRETTO_GENERATOR.Multiply(this.PrivateKey);
        }

        public byte[] ToBytes()
        {
            return this.PrivateKey.ToByteArray();
        }

        public string ToHex()
        {
            return StrUtils.bytesToHex(this.ToBytes());
        }

        public PublicKey ToPublicKey()
        {
            return new PublicKey(this.PublicKey);
        }
    }
}
