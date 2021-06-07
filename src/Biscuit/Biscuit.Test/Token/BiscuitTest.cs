using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token;
using Biscuit.Token.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Biscuit.Test.Token
{
    [TestClass]
    public class BiscuitTest
    {
        [TestMethod]

        public void TestBasic()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.DefaultSymbolTable();
            BlockBuilder authority_builder = new BlockBuilder(0, symbols);

            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file1"), Utils.Symbol("read"))));
            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file2"), Utils.Symbol("read"))));
            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file1"), Utils.Symbol("write"))));

            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.Make(rng, root, Biscuit.Token.Biscuit.DefaultSymbolTable(), authority_builder.Build()).Right;

            Console.WriteLine(b.Print());

            Console.WriteLine("serializing the first token");

            byte[] data = b.Serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data.Length);
            //Console.WriteLine(hex(data));

            Console.WriteLine("deserializing the first token");
            Biscuit.Token.Biscuit deser = Biscuit.Token.Biscuit.FromBytes(data).Right;

            Console.WriteLine(deser.Print());

            // SECOND BLOCK
            Console.WriteLine("preparing the second block");

            KeyPair keypair2 = new KeyPair(rng);

            BlockBuilder builder = deser.CreateBlock();
            builder.AddCheck(Utils.Check(Utils.Rule(
                    "caveat1",
                    Arrays.AsList(Utils.Var("resource")),
                    Arrays.AsList(
                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource"))),
                            Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("read"))),
                            Utils.Pred("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Var("resource"), Utils.Symbol("read")))
                    )
            )));

            Biscuit.Token.Biscuit b2 = deser.Attenuate(rng, keypair2, builder.Build()).Right;

            Console.WriteLine(b2.Print());

            Console.WriteLine("serializing the second token");

            byte[] data2 = b2.Serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data2.Length);
            //Console.WriteLine(hex(data2));

            Console.WriteLine("deserializing the second token");
            Biscuit.Token.Biscuit deser2 = Biscuit.Token.Biscuit.FromBytes(data2).Right;

            Console.WriteLine(deser2.Print());

            // THIRD BLOCK
            Console.WriteLine("preparing the third block");

            KeyPair keypair3 = new KeyPair(rng);

            BlockBuilder builder3 = deser2.CreateBlock();
            builder3.AddCheck(Utils.Check(Utils.Rule(
                    "caveat2",
                    Arrays.AsList(Utils.Symbol("file1")),
                    Arrays.AsList(
                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("file1")))
                    )
            )));

            Biscuit.Token.Biscuit b3 = deser2.Attenuate(rng, keypair3, builder3.Build()).Right;

            Console.WriteLine(b3.Print());

            Console.WriteLine("serializing the third token");

            byte[] data3 = b3.Serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data3.Length);
            //Console.WriteLine(hex(data3));

            Console.WriteLine("deserializing the third token");
            Biscuit.Token.Biscuit final_token = Biscuit.Token.Biscuit.FromBytes(data3).Right;

            Console.WriteLine(final_token.Print());

            // check
            Console.WriteLine("will check the token for resource=file1 and operation=read");

            SymbolTable check_symbols = new SymbolTable(final_token.Symbols);
            List<Fact> ambient_facts = Arrays.AsList(
                    Utils.Fact("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("file1"))).Convert(check_symbols),
                    Utils.Fact("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("read"))).Convert(check_symbols)
            );

            Either<Error, Dictionary<string, HashSet<Fact>>> res = final_token.Check(check_symbols, ambient_facts,
                    new List<Rule>(), new List<Check>(), new Dictionary<string, Rule>());

            Assert.IsTrue(res.IsRight);

            Console.WriteLine("will check the token for resource=file2 and operation=write");

            SymbolTable check_symbols2 = new SymbolTable(final_token.Symbols);
            List<Fact> ambient_facts2 = Arrays.AsList(
                    Utils.Fact("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("file2"))).Convert(check_symbols2),
                    Utils.Fact("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("write"))).Convert(check_symbols2)
            );

            Either<Error, Dictionary<string, HashSet<Fact>>> res2 = final_token.Check(check_symbols2, ambient_facts2,
                    new List<Rule>(), new List<Check>(), new Dictionary<string, Rule>());
            Assert.IsTrue(res2.IsLeft);
            Console.WriteLine(res2.Left);

            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)"),
                            new FailedCheck.FailedBlock(2, 0, "check if resource(#ambient, #file1)")
                    ))),
                    res2.Left);
        }

        [TestMethod]
        public void TestFolders()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            BiscuitBuilder builder = Biscuit.Token.Biscuit.Builder(rng, root);

            builder.AddRight("/folder1/file1", "read");
            builder.AddRight("/folder1/file1", "write");
            builder.AddRight("/folder1/file2", "read");
            builder.AddRight("/folder1/file2", "write");
            builder.AddRight("/folder2/file3", "read");

            Console.WriteLine(builder.Build());
            Biscuit.Token.Biscuit b = builder.Build().Right;

            Console.WriteLine(b.Print());

            BlockBuilder block2 = b.CreateBlock();
            block2.ResourcePrefix("/folder1/");
            block2.CheckRight("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.Attenuate(rng, keypair2, block2.Build()).Right;

            Verifier v1 = b2.Verify(root.ToPublicKey()).Right;
            v1.AddResource("/folder1/file1");
            v1.AddOperation("read");
            v1.Allow();
            Either<Error, long> res = v1.Verify();
            Assert.IsTrue(res.IsRight);

            Verifier v2 = b2.Verify(root.ToPublicKey()).Right;
            v2.AddResource("/folder2/file3");
            v2.AddOperation("read");
            v2.Allow();
            res = v2.Verify();
            Assert.IsTrue(res.IsLeft);

            Verifier v3 = b2.Verify(root.ToPublicKey()).Right;
            v3.AddResource("/folder2/file1");
            v3.AddOperation("write");
            v3.Allow();
            res = v3.Verify();

            Error e = res.Left;
            Assert.IsTrue(res.IsLeft);

            Console.WriteLine(v3.PrintWorld());
            foreach (FailedCheck f in e.FailedCheck().Get())
            {
                Console.WriteLine(f.ToString());
            }
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), $resource.starts_with(\"/folder1/\")"),
                            new FailedCheck.FailedBlock(1, 1, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)")
                    ))),
                    e);
        }

        [TestMethod]
        public void TestSealedTokens()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.DefaultSymbolTable();
            BlockBuilder authority_builder = new BlockBuilder(0, symbols);

            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file1"), Utils.Symbol("read"))));
            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file2"), Utils.Symbol("read"))));
            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("file1"), Utils.Symbol("write"))));

            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.Make(rng, root, Biscuit.Token.Biscuit.DefaultSymbolTable(), authority_builder.Build()).Right;

            Console.WriteLine(b.Print());

            Console.WriteLine("serializing the first token");

            byte[] data = b.Serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data.Length);
            //Console.WriteLine(hex(data));

            Console.WriteLine("deserializing the first token");
            Biscuit.Token.Biscuit deser = Biscuit.Token.Biscuit.FromBytes(data).Right;

            Console.WriteLine(deser.Print());

            // SECOND BLOCK
            Console.WriteLine("preparing the second block");

            KeyPair keypair2 = new KeyPair(rng);

            BlockBuilder builder = deser.CreateBlock();
            builder.AddCheck(Utils.Check(Utils.Rule(
                    "caveat1",
                    Arrays.AsList(Utils.Var("resource")),
                    Arrays.AsList(
                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource"))),
                            Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("read"))),
                            Utils.Pred("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Var("resource"), Utils.Symbol("read")))
                    )
            )));

            Biscuit.Token.Biscuit b2 = deser.Attenuate(rng, keypair2, builder.Build()).Right;

            Console.WriteLine(b2.Print());

            Console.WriteLine("sealing the second token");

            byte[] testkey = Encoding.UTF8.GetBytes("testkey");

            var sealedd = b2.Seal(testkey).Right;
            Console.Write("sealed data len: ");
            Console.WriteLine(sealedd.Length);

            Console.WriteLine("deserializing the sealed token with an invalid key");
            Error e = Biscuit.Token.Biscuit.FromSealed(sealedd, Encoding.UTF8.GetBytes("not this key")).Left;
            Console.WriteLine(e);
            Assert.AreEqual(new SealedSignature(), e);

            Console.WriteLine("deserializing the sealed token with a valid key");
            Biscuit.Token.Biscuit deser2 = Biscuit.Token.Biscuit.FromSealed(sealedd, Encoding.UTF8.GetBytes("testkey")).Right;
            Console.WriteLine(deser2.Print());

            Console.WriteLine("trying to attenuate to a sealed token");
            _ = deser2.CreateBlock();
            _ = deser2.Attenuate(rng, keypair2, builder.Build()).Left;

            Verifier v = deser2.VerifySealed().Right;
            Console.WriteLine(v.PrintWorld());
        }

        [TestMethod]
        public void TestMultipleAttenuation()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.DefaultSymbolTable();
            BlockBuilder authority_builder = new BlockBuilder(0, symbols);
            DateTime date = DateTime.Now;
            authority_builder.AddFact(Utils.Fact("revocation_id", Arrays.AsList(Utils.Date(date))));

            Biscuit.Token.Biscuit biscuit = Biscuit.Token.Biscuit.Make(rng, root, Biscuit.Token.Biscuit.DefaultSymbolTable(), authority_builder.Build()).Right;

            BlockBuilder builder = biscuit.CreateBlock();
            builder.AddFact(Utils.Fact(
                    "right",
                    Arrays.AsList(Utils.Symbol("topic"), Utils.Symbol("tenant"), Utils.Symbol("namespace"), Utils.Symbol("topic"), Utils.Symbol("produce"))
            ));

            string attenuatedB64 = biscuit.Attenuate(rng, new KeyPair(rng), builder.Build()).Right.SerializeBase64().Right;

            Console.WriteLine("attenuated: " + attenuatedB64);

            var attenuatedB64Biscuit = Biscuit.Token.Biscuit.FromBase64(attenuatedB64);
            Assert.IsTrue(attenuatedB64Biscuit.IsRight);

            string attenuated2B64 = biscuit.Attenuate(rng, new KeyPair(rng), builder.Build()).Right.SerializeBase64().Right;

            Console.WriteLine("attenuated2: " + attenuated2B64);
            var attenuated2B64Biscuit = Biscuit.Token.Biscuit.FromBase64(attenuated2B64);
            Assert.IsTrue(attenuated2B64Biscuit.IsRight);
        }

        [TestMethod]
        public void TestGetRevocationIds()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.DefaultSymbolTable();
            BlockBuilder authority_builder = new BlockBuilder(0, symbols);

            Guid uuid1 = Guid.Parse("0b6d033d-83da-437f-a078-1a44890018bc");
            authority_builder.AddFact(Utils.Fact("revocation_id", Arrays.AsList(Utils.Strings(uuid1.ToString()))));

            Biscuit.Token.Biscuit biscuit = Biscuit.Token.Biscuit.Make(rng, root, Biscuit.Token.Biscuit.DefaultSymbolTable(), authority_builder.Build()).Right;

            BlockBuilder builder = biscuit.CreateBlock();
            builder.AddFact(Utils.Fact(
                    "right",
                    Arrays.AsList(Utils.Symbol("topic"), Utils.Symbol("tenant"), Utils.Symbol("namespace"), Utils.Symbol("topic"), Utils.Symbol("produce"))
            ));
            Guid uuid2 = Guid.Parse("46a103de-ee65-4d04-936b-9111eac7dd3b");
            builder.AddFact(Utils.Fact("revocation_id", Arrays.AsList(Utils.Strings(uuid2.ToString()))));

            string attenuatedB64 = biscuit.Attenuate(rng, new KeyPair(rng), builder.Build()).Right.SerializeBase64().Right;
            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.FromBase64(attenuatedB64).Right;

            Verifier v1 = b.Verify(root.ToPublicKey()).Right;
            List<Guid> revokedIds = v1.GetRevocationIdentifiers().Right.Select(s => Guid.Parse(s)).ToList();
            Assert.IsTrue(revokedIds.Contains(uuid1));
            Assert.IsTrue(revokedIds.Contains(uuid2));
        }

        [TestMethod]
        public void TestReset()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            Biscuit.Token.Builder.BiscuitBuilder builder = Biscuit.Token.Biscuit.Builder(rng, root);

            builder.AddRight("/folder1/file1", "read");
            builder.AddRight("/folder1/file1", "write");
            builder.AddRight("/folder1/file2", "read");
            builder.AddRight("/folder1/file2", "write");
            builder.AddRight("/folder2/file3", "read");

            Console.WriteLine(builder.Build());
            Biscuit.Token.Biscuit b = builder.Build().Right;

            Console.WriteLine(b.Print());

            var block2 = b.CreateBlock();
            block2.ResourcePrefix("/folder1/");
            block2.CheckRight("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.Attenuate(rng, keypair2, block2.Build()).Right;

            Verifier v1 = b2.Verify(root.ToPublicKey()).Right;
            v1.Allow();

            Verifier v2 = v1.Clone();

            v2.AddResource("/folder1/file1");
            v2.AddOperation("read");


            Either<Error, long> res = v2.Verify();
            Assert.IsTrue(res.IsRight);

            Verifier v3 = v1.Clone();

            v3.AddResource("/folder2/file3");
            v3.AddOperation("read");

            res = v3.Verify();
            Console.WriteLine(v3.PrintWorld());

            Assert.IsTrue(res.IsLeft);

            Verifier v4 = v1.Clone();

            v4.AddResource("/folder2/file1");
            v4.AddOperation("write");

            res = v4.Verify();

            Error e = res.Left;
            Assert.IsTrue(res.IsLeft);

            Console.WriteLine(v4.PrintWorld());
            foreach (FailedCheck f in e.FailedCheck().Get())
            {
                Console.WriteLine(f.ToString());
            }
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), $resource.starts_with(\"/folder1/\")"),
                            new FailedCheck.FailedBlock(1, 1, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)")
                    ))),
                    e);
        }

        [TestMethod]
        public void TestEmptyVerifier()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            Biscuit.Token.Builder.BiscuitBuilder builder = Biscuit.Token.Biscuit.Builder(rng, root);

            builder.AddRight("/folder1/file1", "read");
            builder.AddRight("/folder1/file1", "write");
            builder.AddRight("/folder1/file2", "read");
            builder.AddRight("/folder1/file2", "write");
            builder.AddRight("/folder2/file3", "read");

            Console.WriteLine(builder.Build());
            Biscuit.Token.Biscuit b = builder.Build().Right;

            Console.WriteLine(b.Print());

            BlockBuilder block2 = b.CreateBlock();
            block2.ResourcePrefix("/folder1/");
            block2.CheckRight("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.Attenuate(rng, keypair2, block2.Build()).Right;

            Verifier v1 = new Verifier();
            v1.Allow();

            Either<Error, long> res = v1.Verify();
            Assert.IsTrue(res.IsRight);

            v1.AddToken(b2, Option.Some(root.ToPublicKey())).Get();

            v1.AddResource("/folder2/file1");
            v1.AddOperation("write");

            res = v1.Verify();

            Assert.IsTrue(res.IsLeft);
        }
    }
}
