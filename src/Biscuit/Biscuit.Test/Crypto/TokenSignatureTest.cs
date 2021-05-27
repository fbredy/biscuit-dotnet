using Biscuit.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;

namespace Biscuit.Test.Crypto
{
    [TestClass]
    public class TokenSignatureTest
    {
        [TestMethod]
        public void testThreeMessages()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            string message1 = "hello";
            KeyPair keypair1 = new KeyPair(rng);
            Biscuit.Crypto.Token token1 = new Biscuit.Crypto.Token(rng, keypair1, Encoding.UTF8.GetBytes(message1));
            Assert.AreEqual(new Right(null), token1.Verify());

            string message2 = "world";
            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Crypto.Token token2 = token1.append(rng, keypair2, Encoding.UTF8.GetBytes(message2));
            Assert.AreEqual(new Right(null), token2.Verify());

            string message3 = "!!";
            KeyPair keypair3 = new KeyPair(rng);
            Biscuit.Crypto.Token token3 = token2.append(rng, keypair3, Encoding.UTF8.GetBytes(message3));
            Assert.AreEqual(new Right(null), token3.Verify());
        }

        [TestMethod]
        public void testChangeMessages()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            string message1 = "hello";
            KeyPair keypair1 = new KeyPair(rng);
            Biscuit.Crypto.Token token1 = new Biscuit.Crypto.Token(rng, keypair1, Encoding.UTF8.GetBytes(message1));
            var either = token1.Verify();
            Assert.AreEqual(new Right(null), either);

            string message2 = "world";
            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Crypto.Token token2 = token1.append(rng, keypair2, Encoding.UTF8.GetBytes(message2));
            token2.blocks[1] = Encoding.UTF8.GetBytes("you");
            Assert.AreEqual(new Left(new Errors.InvalidSignature()), token2.Verify());

            string message3 = "!!";
            KeyPair keypair3 = new KeyPair(rng);
            Biscuit.Crypto.Token token3 = token2.append(rng, keypair3, Encoding.UTF8.GetBytes(message3));
            Assert.AreEqual(new Left(new Errors.InvalidSignature()), token3.Verify());
        }
    }
}
