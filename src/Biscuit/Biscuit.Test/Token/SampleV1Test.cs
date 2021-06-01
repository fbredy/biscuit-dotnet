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
    public class SamplesV1Test
    {
        static byte[] rootData = Ristretto.StrUtils.hexToBytes("529e780f28d9181c968b0eab9977ed8494a27a4544c3adc1910f41bb3dc36958");

        [TestMethod]
        public void Test1_Basic()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test1_basic;

            Console.WriteLine("a");

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.AddOperation("read");
            v1.Allow();
            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.IsTrue(res.IsRight);

            byte[] serialized = token.Serialize().Right;
            Assert.AreEqual(data.Length, serialized.Length);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], serialized[i]);
            }
        }

        [TestMethod]
        public void Test2_DifferentRootKey()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test2_different_root_key;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Error e = token.CheckRootKey(root).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new UnknownPublicKey(), e);
        }

        [TestMethod]
        public void Test3_InvalidSignatureFormat()
        {
            byte[] data = resources.ResourceTestV1.test3_invalid_signature_format;

            Error e = Biscuit.Token.Biscuit.FromBytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.IsTrue(e is DeserializationError);
            DeserializationError errorDeserialized = (DeserializationError)e;
            Assert.IsTrue(errorDeserialized.e.Contains("Input must by 32 bytes"));
            
        }

        [TestMethod]
        public void Test4_random_block()
        {
            byte[] data = resources.ResourceTestV1.test4_random_block;
            Error e = Biscuit.Token.Biscuit.FromBytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new InvalidSignature(), e);
        }

        [TestMethod]
        public void Test5_InvalidSignature()
        {
            byte[] data = resources.ResourceTestV1.test5_invalid_signature;
            Error e = Biscuit.Token.Biscuit.FromBytes(data).Left;
            Console.WriteLine("got error: " + e);
            Assert.AreEqual(new InvalidSignature(), e);
        }

        [TestMethod]
        public void Test6_reordered_blocks()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());
            byte[] data = resources.ResourceTestV1.test6_reordered_blocks;
            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;

            var res = token.Verify(root);
            Console.WriteLine(token.Print());
            Console.WriteLine(res);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new InvalidBlockIndex(3, 2), res.Left);

        }

        [TestMethod]
        public void Test7_invalid_block_fact_authority()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());
            byte[] data = resources.ResourceTestV1.test7_invalid_block_fact_authority;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var res = token.Verify(root);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new FailedLogic(new LogicError.InvalidBlockFact(0, "right(#authority, \"file1\", #write)")), res.Left);
        }

        [TestMethod]
        public void Test8_invalid_block_fact_ambient()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test8_invalid_block_fact_ambient;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var res = token.Verify(root);
            if (res.IsLeft)
            {
                Console.WriteLine("error: " + res.Left);
            }
            Assert.AreEqual(new FailedLogic(new LogicError.InvalidBlockFact(0, "right(#ambient, \"file1\", #write)")), res.Left);
        }

        [TestMethod]
        public void Test9_ExpiredToken()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test9_expired_token;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.AddOperation("read");
            v1.SetTime();
            Console.WriteLine(v1.print_world());

            Error e = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 1, "check if time(#ambient, $date), $date <= 2018-12-20T00:00:00Z")
                    ))),
                    e);
        }

        [TestMethod]
        public void Test10_AuthorityRules()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test10_authority_rules;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.AddOperation("read");
            v1.AddFact(Utils.Fact("owner", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("alice"), Utils.Strings("file1"))));
            v1.Allow();
            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Assert.IsTrue(res.IsRight);
        }

        [TestMethod]
        public void Test11_VerifierAuthorityCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test11_verifier_authority_caveats;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file2");
            v1.AddOperation("read");
            v1.AddCheck(Utils.Check(Utils.Rule(
                    "caveat1",
                    Arrays.AsList(Utils.Var("0")),
                    Arrays.AsList(
                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("0"))),
                            Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("1"))),
                            Utils.Pred("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Var("0"), Utils.Var("1")))
                    )
            )));
            v1.Allow();
            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedVerifier(0, "check if resource(#ambient, $0), operation(#ambient, $1), right(#authority, $0, $1)")
                    ))),
                    e);
        }

        [TestMethod]
        public void Test12_VerifierAuthorityCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test12_authority_caveats;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.AddOperation("read");
            v1.Allow();
            Assert.IsTrue(v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);

            var v2 = token.Verify(root).Right;
            v2.AddResource("file2");
            v2.AddOperation("read");
            v2.Allow();

            var res = v2.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, \"file1\")")
                    ))),
                    e);
        }

        [TestMethod]
        public void Test13_BlockRules()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test13_block_rules;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.SetTime();
            v1.Allow();
            Assert.IsTrue(v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);

            var v2 = token.Verify(root).Right;
            v2.AddResource("file2");
            v2.SetTime();
            v2.Allow();

            var res = v2.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(1, 0, "check if valid_date($0), resource(#ambient, $0)")
                    ))),
                    e);
        }

        [TestMethod]
        public void Test14_RegexConstraint()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test14_regex_constraint;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddResource("file1");
            v1.SetTime();
            v1.Allow();

            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, $0), $0.matches(\"file[0-9]+.txt\")")
                    ))),
                    e);

            var v2 = token.Verify(root).Right;
            v2.AddResource("file123.txt");
            v2.SetTime();
            v2.Allow();
            Assert.IsTrue(v2.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);

        }

        [TestMethod]
        public void Test15_MultiQueriesCaveats()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test15_multi_queries_caveats;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            var queries = new List<RuleBuilder>
            {
                Utils.Rule(
                    "test_must_be_present_authority",
                    Arrays.AsList(Utils.Var("0")),
                    Arrays.AsList(
                            Utils.Pred("must_be_present", Arrays.AsList(Utils.Symbol("authority"), Utils.Var("0")))
                    )
            ),
                Utils.Rule(
                    "test_must_be_present",
                    Arrays.AsList(Utils.Var("0")),
                    Arrays.AsList(
                            Utils.Pred("mst_be_present", Arrays.AsList(Utils.Var("0")))
                    )
            )
            };
            v1.AddCheck(new CheckBuilder(queries));
            v1.Allow();

            Assert.IsTrue(v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500))).IsRight);
        }

        [TestMethod]
        public void Test16_CaveatHeadName()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test16_caveat_head_name;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.Allow();

            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine(res);
            Error e = res.Left;
            Assert.AreEqual(
                    new FailedLogic(new LogicError.FailedChecks(Arrays.AsList<FailedCheck>(
                            new FailedCheck.FailedBlock(0, 0, "check if resource(#ambient, #hello)")
                    ))),
                    e);
        }

        [TestMethod]
        public void Test17_Expressions()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test17_expressions;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.Allow();
            var res = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Assert.AreEqual((long)0, res.Right);
        }

        [TestMethod]
        public void Test18_Unbound_Variables()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test18_unbound_variables_in_rule;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddOperation("write");
            v1.Allow();
            var result = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine("result: " + result);
            Assert.IsTrue(result.IsLeft);
        }

        [TestMethod]
        public void Test19_generating_ambient_from_variables()
        {
            PublicKey root = new PublicKey((new CompressedRistretto(rootData)).Decompress());

            byte[] data = resources.ResourceTestV1.test19_generating_ambient_from_variables;

            var token = Biscuit.Token.Biscuit.FromBytes(data).Right;
            Console.WriteLine(token.Print());

            var v1 = token.Verify(root).Right;
            v1.AddOperation("write");
            v1.Allow();
            var result = v1.Verify(new RunLimits(500, 100, TimeSpan.FromMilliseconds(500)));
            Console.WriteLine("result: " + result);
            Assert.IsTrue(result.IsLeft);
        }
    }

}
