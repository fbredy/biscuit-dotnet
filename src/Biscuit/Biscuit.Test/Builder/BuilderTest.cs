using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Token.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Biscuit.Test.Builder
{
    [TestClass]
    public class BuilderTest
    {
        [TestMethod]
        public void TestBuild()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);
            SymbolTable symbols = Biscuit.Token.Biscuit.DefaultSymbolTable();

            BlockBuilder authority_builder = new BlockBuilder(0, symbols);
            authority_builder.AddFact(Utils.Fact("revocation_id", Arrays.AsList(Utils.Date(DateTime.Now))));
            authority_builder.AddFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("admin"))));
            authority_builder.AddRule(Utils.ConstrainedRule("right",
                    Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("namespace"), Utils.Var("tenant"), Utils.Var("namespace"), Utils.Var("operation")),
                    Arrays.AsList(Utils.Pred("ns_operation", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("namespace"), Utils.Var("tenant"), Utils.Var("namespace"), Utils.Var("operation")))),
                    Arrays.AsList<ExpressionBuilder>(
                            new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.Contains,
                                    new ExpressionBuilder.Value(Utils.Var("operation")),
                                    new ExpressionBuilder.Value(new Term.Set(new HashSet<Term>(Arrays.AsList<Term>(
                                            Utils.Symbol("create_topic"),
                                            Utils.Symbol("get_topic"),
                                            Utils.Symbol("get_topics")
                                    )))))
                    )
            ));
            authority_builder.AddRule(Utils.ConstrainedRule("right",
                    Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("topic"), Utils.Var("tenant"), Utils.Var("namespace"), Utils.Var("topic"), Utils.Var("operation")),
                    Arrays.AsList(Utils.Pred("topic_operation", Arrays.AsList(Utils.Symbol("authority"), Utils.Symbol("topic"), Utils.Var("tenant"), Utils.Var("namespace"), Utils.Var("topic"), Utils.Var("operation")))),
                    Arrays.AsList<ExpressionBuilder>(
                            new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.Contains,
                                    new ExpressionBuilder.Value(Utils.Var("operation")),
                                    new ExpressionBuilder.Value(new Term.Set(new HashSet<Term>(Arrays.AsList(
                                            Utils.Symbol("lookup")
                                    )))))
                    )
            ));
            Biscuit.Token.Biscuit rootBiscuit = Biscuit.Token.Biscuit.Make(rng, root, symbols, authority_builder.Build()).Right;

            Console.WriteLine(rootBiscuit.Print());

            Assert.IsNotNull(rootBiscuit);
        }
    }
}
