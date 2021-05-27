using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Token.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Biscuit.Test.Builder
{
    [TestClass]
    public class BuilderTest
    {
        [TestMethod]
        public void testBuild()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            KeyPair root = new KeyPair(rng);
            SymbolTable symbols = Biscuit.Token.Biscuit.default_symbol_table();

            Block authority_builder = new Block(0, symbols);
            authority_builder.add_fact(Utils.fact("revocation_id", Arrays.asList(Utils.date(DateTime.Now))));
            authority_builder.add_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.s("admin"))));
            authority_builder.add_rule(Utils.constrained_rule("right",
                    Arrays.asList(Utils.s("authority"), Utils.s("namespace"), Utils.var("tenant"), Utils.var("namespace"), Utils.var("operation")),
                    Arrays.asList(Utils.pred("ns_operation", Arrays.asList(Utils.s("authority"), Utils.s("namespace"), Utils.var("tenant"), Utils.var("namespace"), Utils.var("operation")))),
                    Arrays.asList<Expression>(
                            new Expression.Binary(
                                    Expression.Op.Contains,
                                    new Expression.Value(Utils.var("operation")),
                                    new Expression.Value(new Term.Set(new HashSet<Term>(Arrays.asList<Term>(
                                            Utils.s("create_topic"),
                                            Utils.s("get_topic"),
                                            Utils.s("get_topics")
                                    )))))
                    )
            ));
            authority_builder.add_rule(Utils.constrained_rule("right",
                    Arrays.asList(Utils.s("authority"), Utils.s("topic"), Utils.var("tenant"), Utils.var("namespace"), Utils.var("topic"), Utils.var("operation")),
                    Arrays.asList(Utils.pred("topic_operation", Arrays.asList(Utils.s("authority"), Utils.s("topic"), Utils.var("tenant"), Utils.var("namespace"), Utils.var("topic"), Utils.var("operation")))),
                    Arrays.asList<Expression>(
                            new Expression.Binary(
                                    Expression.Op.Contains,
                                    new Expression.Value(Utils.var("operation")),
                                    new Expression.Value(new Term.Set(new HashSet<Term>(Arrays.asList(
                                            Utils.s("lookup")
                                    )))))
                    )
            ));
            Biscuit.Token.Biscuit rootBiscuit = Biscuit.Token.Biscuit.make(rng, root, symbols, authority_builder.build()).Right;

            Console.WriteLine(rootBiscuit.print());

            Assert.IsNotNull(rootBiscuit);
        }
    }
}
