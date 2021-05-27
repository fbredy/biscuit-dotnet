using Ristretto;
using System.Security.Cryptography;

namespace Biscuit.Crypto
{
    /// <summary>
    /// Private and public key
    /// </summary>
    public sealed class KeyPair
    {
        public Scalar Private_key { get; }
    
        public RistrettoElement Public_key { get; }
    
        public KeyPair(RNGCryptoServiceProvider rng)
        {
            byte[] b = new byte[64];
            rng.GetBytes(b);
            this.Private_key = Scalar.FromBytesModOrderWide(b);
            this.Public_key = Constants.RISTRETTO_GENERATOR.Multiply(this.Private_key);
        }

        public KeyPair(string hex)
        {
            byte[] b = StrUtils.hexToBytes(hex);
            this.Private_key = Scalar.FromBytesModOrder(b);
            this.Public_key = Constants.RISTRETTO_GENERATOR.Multiply(this.Private_key);
        }

        public KeyPair(byte[] b)
        {
            this.Private_key = Scalar.FromBytesModOrderWide(b);
            this.Public_key = Constants.RISTRETTO_GENERATOR.Multiply(this.Private_key);
        }

        public byte[] ToBytes()
        {
            return this.Private_key.ToByteArray();
        }
        
        public string ToHex()
        {
            return StrUtils.bytesToHex(this.ToBytes());
        }

        public PublicKey public_key()
        {
            return new PublicKey(this.Public_key);
        }
    }
}
