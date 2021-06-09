using System;

namespace Biscuit.Token
{
    public class RevocationIdentifier
    {
        public byte[] Bytes { get; }

        public RevocationIdentifier(byte[] bytes)
        {
            this.Bytes = bytes;
        }

        public static RevocationIdentifier From64(string base64string)
        {
            return new RevocationIdentifier(Convert.FromBase64String(base64string));
        }

        public string Serialize64()
        {
            return Convert.ToBase64String(this.Bytes);
        }

        public static RevocationIdentifier FromBytes(byte[] bytes)
        {
            return new RevocationIdentifier(bytes);
        }
    }
}
