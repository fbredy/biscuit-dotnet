using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ristretto;
using System;
using System.Collections.Generic;

namespace Biscuit.Test.Token
{
    [TestClass]
    public class SamplesV0Test
    {
        static byte[] rootData =
            Ristretto.StrUtils.hexToBytes("529e780f28d9181c968b0eab9977ed8494a27a4544c3adc1910f41bb3dc36958");

        [TestMethod]
        public void test1_Basic()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV0.test1_basic;

            Console.WriteLine("a");

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.add_operation("read");
            v1.allow();
            var res = v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.IsTrue(res.IsRight);

            byte[] serialized = token.serialize().Right;

            Assert.AreEqual(data.Length, serialized.Length);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], serialized[i]);
            }
        }

        [TestMethod]
        public void test2_DifferentRootKey()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test2_different_root_key;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Error e = token.check_root_key(root).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new UnknownPublicKey(), e);
        }

        [TestMethod]
        public void test3_InvalidSignatureFormat()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test3_invalid_signature_format;

            Error e = Biscuit.Token.Biscuit.from_bytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.IsTrue(e is DeserializationError);
            DeserializationError errorDeserialized = (DeserializationError)e;
            Assert.IsTrue(errorDeserialized.e.Contains("Input must by 32 bytes"));
        }

        [TestMethod]
        public void test4_random_block()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test4_random_block;


            Error e = Biscuit.Token.Biscuit.from_bytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new InvalidSignature(), e);
        }

        [TestMethod]
        public void test5_InvalidSignature()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test5_invalid_signature;

            Error e = Biscuit.Token.Biscuit.from_bytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new InvalidSignature(), e);
        }

        [TestMethod]
        public void test6_reordered_blocks()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test6_reordered_blocks;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;

            var res = token.verify(root);
            Console.WriteLine(token.print());
            Console.WriteLine(res);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new InvalidBlockIndex(3, 2), res.Left);

        }

        [TestMethod]
        public void test7_invalid_block_fact_authority()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test7_invalid_block_fact_authority;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var res = token.verify(root);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new FailedLogic(new LogicError.InvalidBlockFact(0, "right(#authority, \"file1\", #write)")), res.Left);
        }

        [TestMethod]
        public void test8_invalid_block_fact_ambient()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test8_invalid_block_fact_ambient;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var res = token.verify(root);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new FailedLogic(new LogicError.InvalidBlockFact(0, "right(#ambient, \"file1\", #write)")), res.Left);
        }

        [TestMethod]
        public void test9_ExpiredToken()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test9_expired_token;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.add_operation("read");
            v1.set_time();
            Console.WriteLine(v1.print_world());

            Error e = v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).Left;
            Assert.AreEqual(
            new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                    new FailedCheck.FailedBlock(1, 1, "check if time(#ambient, $date), $date <= 2018-12-20T00:00:00Z")
            ))),
            e);
        }

        [TestMethod]
        public void test10_AuthorityRules()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test10_authority_rules;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.add_operation("read");
            v1.add_fact(Utils.fact("owner", Arrays.asList(Utils.s("ambient"), Utils.s("alice"), Utils.strings("file1"))));
            v1.allow();
            var res = v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Assert.IsTrue(res.IsRight);
        }

        [TestMethod]
        public void test11_VerifierAuthorityCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test11_verifier_authority_caveats;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file2");
            v1.add_operation("read");
            v1.add_check(Utils.check(Utils.rule(
                    "caveat1",
                    Arrays.asList(Utils.var("0")),
                    Arrays.asList(
                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("0"))),
                            Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.var("1"))),
                            Utils.pred("right", Arrays.asList(Utils.s("authority"), Utils.var("0"), Utils.var("1")))
                    )
            )));
            v1.allow();
            var res = v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
            new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                    new FailedCheck.FailedVerifier(0, "check if resource(#ambient, $0), operation(#ambient, $1), right(#authority, $0, $1)")
            ))),
            e);
        }

        [TestMethod]
        public void test12_VerifierAuthorityCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test12_authority_caveats;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.add_operation("read");
            v1.allow();
            Assert.IsTrue(v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);

            var v2 = token.verify(root).Right;
            v2.add_resource("file2");
            v2.add_operation("read");
            v2.allow();

            var res = v2.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                        new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, \"file1\")")
                ))),
                e);
        }

        [TestMethod]
        public void test13_BlockRules()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test13_block_rules;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.set_time();
            v1.allow();
            Assert.IsTrue(v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);

            var v2 = token.verify(root).Right;
            v2.add_resource("file2");
            v2.set_time();
            v2.allow();

            var res = v2.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
            new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                    new FailedCheck.FailedBlock(1, 0, "check if valid_date($0), resource(#ambient, $0)")
            ))),
            e);
        }

        [TestMethod]
        public void test14_RegexConstraint()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test14_regex_constraint;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.add_resource("file1");
            v1.set_time();
            v1.allow();

            var res = v1.verify(new RunLimits(1000, 100, TimeSpan.FromMilliseconds(30)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                        new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, $0), $0.matches(\"file[0-9]+.txt\")")
                ))),
                e);

            var v2 = token.verify(root).Right;
            v2.add_resource("file123.txt");
            v2.set_time();
            v2.allow();
            Assert.IsTrue(v2.verify(new RunLimits(1000, 100, TimeSpan.FromMilliseconds(30))).IsRight);

        }

        [TestMethod]
        public void test15_MultiQueriesCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test15_multi_queries_caveats;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            var queries = new List<Biscuit.Token.Builder.Rule>();
            queries.Add(Utils.rule(
                    "test_must_be_present_authority",
                    Arrays.asList(Utils.var("0")),
                    Arrays.asList(
                            Utils.pred("must_be_present", Arrays.asList(Utils.s("authority"), Utils.var("0")))
                    )
                    ));
            queries.Add(Utils.rule(
                    "test_must_be_present",
                    Arrays.asList(Utils.var("0")),
                    Arrays.asList(
                            Utils.pred("mst_be_present", Arrays.asList(Utils.var("0")))
                    )
            ));
            v1.add_check(new Biscuit.Token.Builder.Check(queries));
            v1.allow();

            Assert.IsTrue(v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);
        }

        [TestMethod]
        public void test16_CaveatHeadName()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data =
                    resources.ResourceTestV0.test16_caveat_head_name;

            var token = Biscuit.Token.Biscuit.from_bytes(data).Right;
            Console.WriteLine(token.print());

            var v1 = token.verify(root).Right;
            v1.allow();


            var res = v1.verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.asList<FailedCheck>(
                            new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, #hello)")
                    ))),
                    e);
        }
    }
}
