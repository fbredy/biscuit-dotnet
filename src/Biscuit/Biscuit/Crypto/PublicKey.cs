using Ristretto;

namespace Biscuit.Crypto
{
    public class PublicKey
    {
        public RistrettoElement Key { get; }

        public PublicKey(RistrettoElement publicKey)
        {
            this.Key = publicKey;
        }

        public PublicKey(byte[] data)
        {
            CompressedRistretto compressed = new CompressedRistretto(data);
            this.Key = compressed.Decompress();
        }

        public PublicKey(string hex)
        {
            byte[] data = StrUtils.hexToBytes(hex);
            CompressedRistretto compressed = new CompressedRistretto(data);
            this.Key = compressed.Decompress();
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
