using Ristretto;

namespace Biscuit.Crypto
{
    public class PublicKey
    {
        public RistrettoElement Key { get; }

        public PublicKey(RistrettoElement public_key)
        {
            this.Key = public_key;
        }

        public PublicKey(byte[] data)
        {
            CompressedRistretto c = new CompressedRistretto(data);
            this.Key = c.Decompress();
        }

        public PublicKey(string hex)
        {
            byte[] data = StrUtils.hexToBytes(hex);
            CompressedRistretto c = new CompressedRistretto(data);
            this.Key = c.Decompress();
        }

        public byte[] ToBytes()
        {
            return this.Key.Compress().ToByteArray();
        }

        public string ToHex()
        {
            return StrUtils.bytesToHex(this.ToBytes());
        }
    }
}
