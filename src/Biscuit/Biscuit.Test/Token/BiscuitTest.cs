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
using Block = Biscuit.Token.Builder.Block;

namespace Biscuit.Test.Token
{
    [TestClass]
    public class BiscuitTest
    {

        [TestMethod]

        public void testBasic()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.default_symbol_table();
            Block authority_builder = new Block(0, symbols);

            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file1"), Utils.s("read"))));
            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file2"), Utils.s("read"))));
            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file1"), Utils.s("write"))));

            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.make(rng, root, Biscuit.Token.Biscuit.default_symbol_table(), authority_builder.build()).Right;

            Console.WriteLine(b.print());

            Console.WriteLine("serializing the first token");

            byte[] data = b.serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data.Length);
            //Console.WriteLine(hex(data));

            Console.WriteLine("deserializing the first token");
            Biscuit.Token.Biscuit deser = Biscuit.Token.Biscuit.from_bytes(data).Right;

            Console.WriteLine(deser.print());

            // SECOND BLOCK
            Console.WriteLine("preparing the second block");

            KeyPair keypair2 = new KeyPair(rng);

            Block builder = deser.create_block();
            builder.add_check(Utils.check(Utils.rule(
                    "caveat1",
                    Arrays.asList(Utils.var("resource")),
                    Arrays.asList(
                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource"))),
                            Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.s("read"))),
                            Utils.pred("right", Arrays.asList(Utils.s("authority"), Utils.var("resource"), Utils.s("read")))
                    )
            )));

            Biscuit.Token.Biscuit b2 = deser.attenuate(rng, keypair2, builder.build()).Right;

            Console.WriteLine(b2.print());

            Console.WriteLine("serializing the second token");

            byte[] data2 = b2.serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data2.Length);
            //Console.WriteLine(hex(data2));

            Console.WriteLine("deserializing the second token");
            Biscuit.Token.Biscuit deser2 = Biscuit.Token.Biscuit.from_bytes(data2).Right;

            Console.WriteLine(deser2.print());

            // THIRD BLOCK
            Console.WriteLine("preparing the third block");

            KeyPair keypair3 = new KeyPair(rng);

            Block builder3 = deser2.create_block();
            builder3.add_check(Utils.check(Utils.rule(
                    "caveat2",
                    Arrays.asList(Utils.s("file1")),
                    Arrays.asList(
                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.s("file1")))
                    )
            )));

            Biscuit.Token.Biscuit b3 = deser2.attenuate(rng, keypair3, builder3.build()).Right;

            Console.WriteLine(b3.print());

            Console.WriteLine("serializing the third token");

            byte[] data3 = b3.serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data3.Length);
            //Console.WriteLine(hex(data3));

            Console.WriteLine("deserializing the third token");
            Biscuit.Token.Biscuit final_token = Biscuit.Token.Biscuit.from_bytes(data3).Right;

            Console.WriteLine(final_token.print());

            // check
            Console.WriteLine("will check the token for resource=file1 and operation=read");

            SymbolTable check_symbols = new SymbolTable(final_token.symbols);
            List<Biscuit.Datalog.Fact> ambient_facts = Arrays.asList(
                    Utils.fact("resource", Arrays.asList(Utils.s("ambient"), Utils.s("file1"))).convert(check_symbols),
                    Utils.fact("operation", Arrays.asList(Utils.s("ambient"), Utils.s("read"))).convert(check_symbols)
            );

            Either<Error, Dictionary<string, HashSet<Biscuit.Datalog.Fact>>> res = final_token.check(check_symbols, ambient_facts,
                    new List<Biscuit.Datalog.Rule>(), new List<Biscuit.Datalog.Check>(), new Dictionary<string, Biscuit.Datalog.Rule>());

            Assert.IsTrue(res.IsRight);

            Console.WriteLine("will check the token for resource=file2 and operation=write");

            SymbolTable check_symbols2 = new SymbolTable(final_token.symbols);
            List<Biscuit.Datalog.Fact> ambient_facts2 = Arrays.asList(
                    Utils.fact("resource", Arrays.asList(Utils.s("ambient"), Utils.s("file2"))).convert(check_symbols2),
                    Utils.fact("operation", Arrays.asList(Utils.s("ambient"), Utils.s("write"))).convert(check_symbols2)
            );

            Either<Error, Dictionary<string, HashSet<Biscuit.Datalog.Fact>>> res2 = final_token.check(check_symbols2, ambient_facts2,
                    new List<Biscuit.Datalog.Rule>(), new List<Biscuit.Datalog.Check>(), new Dictionary<string, Biscuit.Datalog.Rule>());
            Assert.IsTrue(res2.IsLeft);
            Console.WriteLine(res2.Left);

            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)"),
                            new FailedCheck.FailedBlock(2, 0, "check if resource(#ambient, #file1)")
                    ))),
                    res2.Left);
        }

        [TestMethod]
        public void testFolders()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            Biscuit.Token.Builder.BiscuitBuilder builder = Biscuit.Token.Biscuit.builder(rng, root);

            builder.add_right("/folder1/file1", "read");
            builder.add_right("/folder1/file1", "write");
            builder.add_right("/folder1/file2", "read");
            builder.add_right("/folder1/file2", "write");
            builder.add_right("/folder2/file3", "read");

            Console.WriteLine(builder.build());
            Biscuit.Token.Biscuit b = builder.build().Right;

            Console.WriteLine(b.print());

            Biscuit.Token.Builder.Block block2 = b.create_block();
            block2.resource_prefix("/folder1/");
            block2.check_right("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.attenuate(rng, keypair2, block2.build()).Right;

            Verifier v1 = b2.verify(root.public_key()).Right;
            v1.add_resource("/folder1/file1");
            v1.add_operation("read");
            v1.allow();
            Either<Error, long> res = v1.verify();
            Assert.IsTrue(res.IsRight);

            Verifier v2 = b2.verify(root.public_key()).Right;
            v2.add_resource("/folder2/file3");
            v2.add_operation("read");
            v2.allow();
            res = v2.verify();
            Assert.IsTrue(res.IsLeft);

            Verifier v3 = b2.verify(root.public_key()).Right;
            v3.add_resource("/folder2/file1");
            v3.add_operation("write");
            v3.allow();
            res = v3.verify();

            Error e = res.Left;
            Assert.IsTrue(res.IsLeft);

            Console.WriteLine(v3.print_world());
            foreach (FailedCheck f in e.FailedCheck().get())
            {
                Console.WriteLine(f.ToString());
            }
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), $resource.starts_with(\"/folder1/\")"),
                            new FailedCheck.FailedBlock(1, 1, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)")
                    ))),
                    e);
        }

        [TestMethod]
        public void testSealedTokens()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.default_symbol_table();
            Block authority_builder = new Block(0, symbols);

            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file1"), Utils.s("read"))));
            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file2"), Utils.s("read"))));
            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("file1"), Utils.s("write"))));

            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.make(rng, root, Biscuit.Token.Biscuit.default_symbol_table(), authority_builder.build()).Right;

            Console.WriteLine(b.print());

            Console.WriteLine("serializing the first token");

            byte[] data = b.serialize().Right;

            Console.Write("data len: ");
            Console.WriteLine(data.Length);
            //Console.WriteLine(hex(data));

            Console.WriteLine("deserializing the first token");
            Biscuit.Token.Biscuit deser = Biscuit.Token.Biscuit.from_bytes(data).Right;

            Console.WriteLine(deser.print());

            // SECOND BLOCK
            Console.WriteLine("preparing the second block");

            KeyPair keypair2 = new KeyPair(rng);

            Block builder = deser.create_block();
            builder.add_check(Utils.check(Utils.rule(
                    "caveat1",
                    Arrays.asList(Utils.var("resource")),
                    Arrays.asList(
                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource"))),
                            Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.s("read"))),
                            Utils.pred("right", Arrays.asList(Utils.s("authority"), Utils.var("resource"), Utils.s("read")))
                    )
            )));

            Biscuit.Token.Biscuit b2 = deser.attenuate(rng, keypair2, builder.build()).Right;

            Console.WriteLine(b2.print());

            Console.WriteLine("sealing the second token");

            byte[] testkey = Encoding.UTF8.GetBytes("testkey");

            var sealedd = b2.seal(testkey).Right;
            Console.Write("sealed data len: ");
            Console.WriteLine(sealedd.Length);

            Console.WriteLine("deserializing the sealed token with an invalid key");
            Error e = Biscuit.Token.Biscuit.from_sealed(sealedd, Encoding.UTF8.GetBytes("not this key")).Left;
            Console.WriteLine(e);
            Assert.AreEqual(new SealedSignature(), e);

            Console.WriteLine("deserializing the sealed token with a valid key");
            Biscuit.Token.Biscuit deser2 = Biscuit.Token.Biscuit.from_sealed(sealedd, Encoding.UTF8.GetBytes("testkey")).Right;
            Console.WriteLine(deser2.print());

            Console.WriteLine("trying to attenuate to a sealed token");
            Block builder2 = deser2.create_block();
            Error e2 = deser2.attenuate(rng, keypair2, builder.build()).Left;

            Verifier v = deser2.verify_sealed().Right;
            Console.WriteLine(v.print_world());
        }

        [TestMethod]
        public void testMultipleAttenuation()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.default_symbol_table();
            Block authority_builder = new Block(0, symbols);
            DateTime date = DateTime.Now;
            authority_builder.add_fact(Utils.fact("revocation_id", Arrays.asList(Utils.date(date))));

            Biscuit.Token.Biscuit biscuit = Biscuit.Token.Biscuit.make(rng, root, Biscuit.Token.Biscuit.default_symbol_table(), authority_builder.build()).Right;

            Block builder = biscuit.create_block();
            builder.add_fact(Utils.fact(
                    "right",
                    Arrays.asList(Utils.s("topic"), Utils.s("tenant"), Utils.s("namespace"), Utils.s("topic"), Utils.s("produce"))
            ));

            string attenuatedB64 = biscuit.attenuate(rng, new KeyPair(rng), builder.build()).Right.serialize_b64().Right;

            Console.WriteLine("attenuated: " + attenuatedB64);

            var attenuatedB64Biscuit = Biscuit.Token.Biscuit.from_b64(attenuatedB64);
            Assert.IsTrue(attenuatedB64Biscuit.IsRight);

            string attenuated2B64 = biscuit.attenuate(rng, new KeyPair(rng), builder.build()).Right.serialize_b64().Right;

            Console.WriteLine("attenuated2: " + attenuated2B64);
            var attenuated2B64Biscuit = Biscuit.Token.Biscuit.from_b64(attenuated2B64);
            Assert.IsTrue(attenuated2B64Biscuit.IsRight);
        }

        [TestMethod]
        public void testGetRevocationIds()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);

            SymbolTable symbols = Biscuit.Token.Biscuit.default_symbol_table();
            Block authority_builder = new Block(0, symbols);

            Guid uuid1 = Guid.Parse("0b6d033d-83da-437f-a078-1a44890018bc");
            authority_builder.add_fact(Utils.fact("revocation_id", Arrays.asList(Utils.strings(uuid1.ToString()))));

            Biscuit.Token.Biscuit biscuit = Biscuit.Token.Biscuit.make(rng, root, Biscuit.Token.Biscuit.default_symbol_table(), authority_builder.build()).Right;

            Block builder = biscuit.create_block();
            builder.add_fact(Utils.fact(
                    "right",
                    Arrays.asList(Utils.s("topic"), Utils.s("tenant"), Utils.s("namespace"), Utils.s("topic"), Utils.s("produce"))
            ));
            Guid uuid2 = Guid.Parse("46a103de-ee65-4d04-936b-9111eac7dd3b");
            builder.add_fact(Utils.fact("revocation_id", Arrays.asList(Utils.strings(uuid2.ToString()))));

            string attenuatedB64 = biscuit.attenuate(rng, new KeyPair(rng), builder.build()).Right.serialize_b64().Right;
            Biscuit.Token.Biscuit b = Biscuit.Token.Biscuit.from_b64(attenuatedB64).Right;

            Verifier v1 = b.verify(root.public_key()).Right;
            List<Guid> revokedIds = v1.get_revocation_ids().Right.Select(s=> Guid.Parse(s)).ToList();
            Assert.IsTrue(revokedIds.Contains(uuid1));
            Assert.IsTrue(revokedIds.Contains(uuid2));
        }

        [TestMethod]
        public void testReset()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            Biscuit.Token.Builder.BiscuitBuilder builder = Biscuit.Token.Biscuit.builder(rng, root);

            builder.add_right("/folder1/file1", "read");
            builder.add_right("/folder1/file1", "write");
            builder.add_right("/folder1/file2", "read");
            builder.add_right("/folder1/file2", "write");
            builder.add_right("/folder2/file3", "read");

            Console.WriteLine(builder.build());
            Biscuit.Token.Biscuit b = builder.build().Right;

            Console.WriteLine(b.print());

            var block2 = b.create_block();
            block2.resource_prefix("/folder1/");
            block2.check_right("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.attenuate(rng, keypair2, block2.build()).Right;

            Verifier v1 = b2.verify(root.public_key()).Right;
            v1.allow();

            Verifier v2 = v1.clone();

            v2.add_resource("/folder1/file1");
            v2.add_operation("read");


            Either<Error, long> res = v2.verify();
            Assert.IsTrue(res.IsRight);

            Verifier v3 = v1.clone();

            v3.add_resource("/folder2/file3");
            v3.add_operation("read");

            res = v3.verify();
            Console.WriteLine(v3.print_world());

            Assert.IsTrue(res.IsLeft);

            Verifier v4 = v1.clone();

            v4.add_resource("/folder2/file1");
            v4.add_operation("write");

            res = v4.verify();

            Error e = res.Left;
            Assert.IsTrue(res.IsLeft);

            Console.WriteLine(v4.print_world());
            foreach (FailedCheck f in e.FailedCheck().get())
            {
                Console.WriteLine(f.ToString());
            }
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if resource(#ambient, $resource), $resource.starts_with(\"/folder1/\")"),
                            new FailedCheck.FailedBlock(1, 1, "check if resource(#ambient, $resource), operation(#ambient, #read), right(#authority, $resource, #read)")
                    ))),
                    e);
        }

        [TestMethod]
        public void testEmptyVerifier()
        {
            byte[] seed = { 0, 0, 0, 0 };
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(seed);

            Console.WriteLine("preparing the authority block");

            KeyPair root = new KeyPair(rng);

            Biscuit.Token.Builder.BiscuitBuilder builder = Biscuit.Token.Biscuit.builder(rng, root);

            builder.add_right("/folder1/file1", "read");
            builder.add_right("/folder1/file1", "write");
            builder.add_right("/folder1/file2", "read");
            builder.add_right("/folder1/file2", "write");
            builder.add_right("/folder2/file3", "read");

            Console.WriteLine(builder.build());
            Biscuit.Token.Biscuit b = builder.build().Right;

            Console.WriteLine(b.print());

            Block block2 = b.create_block();
            block2.resource_prefix("/folder1/");
            block2.check_right("read");

            KeyPair keypair2 = new KeyPair(rng);
            Biscuit.Token.Biscuit b2 = b.attenuate(rng, keypair2, block2.build()).Right;

            Verifier v1 = new Verifier();
            v1.allow();

            Either<Error, long> res = v1.verify();
            Assert.IsTrue(res.IsRight);

            v1.add_token(b2, Option.some(root.public_key())).Get();

            v1.add_resource("/folder2/file1");
            v1.add_operation("write");

            res = v1.verify();

            Error e = res.Left;
            Assert.IsTrue(res.IsLeft);
        }
    }
}
