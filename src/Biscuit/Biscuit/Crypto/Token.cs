using Biscuit.Errors;
using Ristretto;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Biscuit.Crypto
{
    public class Token
    {
        public IList<byte[]> Blocks { get; }
        public IList<RistrettoElement> Keys { get; }
        public TokenSignature Signature { get; }

        public Token(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            this.Signature = new TokenSignature(rng, keypair, message);
            this.Blocks = new List<byte[]> { message };
            this.Keys = new List<RistrettoElement> { keypair.PublicKey };
        }

        public Token(IList<byte[]> blocks, IList<RistrettoElement> keys, TokenSignature signature)
        {
            this.Signature = signature;
            this.Blocks = blocks;
            this.Keys = keys;
        }

        public Token Append(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            TokenSignature signature = this.Signature.Sign(rng, keypair, message);
            Token token = new Token(this.Blocks, this.Keys, signature);
            token.Blocks.Add(message);
            token.Keys.Add(keypair.PublicKey);

            return token;
        }

        public Either<Error, Void> Verify()
        {
            return this.Signature.Verify(this.Keys, this.Blocks);
        }
    }
}
