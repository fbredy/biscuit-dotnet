using Biscuit.Errors;
using Ristretto;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Biscuit.Crypto
{
    public class Token
    {
        public List<byte[]> blocks { get; }
        public List<RistrettoElement> keys { get; }
        public TokenSignature signature { get; }

        public Token(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            this.signature = new TokenSignature(rng, keypair, message);
            this.blocks = new List<byte[]>();
            this.blocks.Add(message);
            this.keys = new List<RistrettoElement>();
            this.keys.Add(keypair.Public_key);
        }

        public Token(List<byte[]> blocks, List<RistrettoElement> keys, TokenSignature signature)
        {
            this.signature = signature;
            this.blocks = blocks;
            this.keys = keys;
        }

        public Token append(RNGCryptoServiceProvider rng, KeyPair keypair, byte[] message)
        {
            TokenSignature signature = this.signature.Sign(rng, keypair, message);


            Token token = new Token(this.blocks, this.keys, signature);
            token.blocks.Add(message);
            token.keys.Add(keypair.Public_key);

            return token;
        }

        // FIXME: rust version returns a Result<(), error::Signature>
        public Either<Error, Void> Verify()
        {
            return this.signature.Verify(this.keys, this.blocks);
        }
    }
}
