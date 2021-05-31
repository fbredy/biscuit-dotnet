using Biscuit.Datalog;
using Biscuit.Token.Builder;
using Biscuit.Token.Builder.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CheckBuilder = Biscuit.Token.Builder.CheckBuilder;
using FactBuilder = Biscuit.Token.Builder.FactBuilder;
using Parser = Biscuit.Token.Builder.Parser.Parser;
using RuleBuilder = Biscuit.Token.Builder.RuleBuilder;

namespace Biscuit.Test.Builder
{
    [TestClass]
    public class ParserTest
    {

        [TestMethod]
        public void testName()
        {
            Either<Error, Tuple<string, string>> res = Parser.Name("operation(#ambient, #read)");
            Assert.AreEqual(new Right(new Tuple<string, string>("(#ambient, #read)", "operation")), res);
        }

        [TestMethod]
        public void testSymbol()
        {
            Either<Error, Tuple<string, Term.Symbol>> res = Parser.Symbol("#ambient");
            Assert.AreEqual(new Right(new Tuple<string, Term.Symbol>("", (Term.Symbol)Utils.Symbol("ambient"))), res);
        }

        [TestMethod]
        public void testString()
        {
            Either<Error, Tuple<string, Term.Str>> res = Parser.Strings("\"file1 a hello - 123_\"");
            Assert.AreEqual(new Right(new Tuple<string, Term.Str>("", (Term.Str)Utils.Strings("file1 a hello - 123_"))), res);
        }

        [TestMethod]
        public void testInteger()
        {
            Either<Error, Tuple<string, Term.Integer>> res = Parser.Integer("123");
            Assert.AreEqual(new Right(new Tuple<string, Term.Integer>("", (Term.Integer)Utils.Integer(123))), res);

            Either<Error, Tuple<string, Term.Integer>> res2 = Parser.Integer("-42");
            Assert.AreEqual(new Right(new Tuple<string, Term.Integer>("", (Term.Integer)Utils.Integer(-42))), res2);
        }

        [TestMethod]
        public void testDate()
        {
            Either<Error, Tuple<string, Term.Date>> res = Parser.Date("2019-12-02T13:49:53Z,");
            Assert.AreEqual(new Right(new Tuple<string, Term.Date>(",", new Term.Date(1575294593))), res);
        }

        [TestMethod]
        public void testVariable()
        {
            Either<Error, Tuple<string, Term.Variable>> res = Parser.Variable("$name");
            Assert.AreEqual(new Right(new Tuple<string, Term.Variable>("", (Term.Variable)Utils.Var("name"))), res);
        }

        public void testConstraint()
        {
        }

        [TestMethod]
        public void testFact()
        {
            Either<Error, Tuple<string, FactBuilder>> res = Biscuit.Token.Builder.Parser.Parser.Fact("right( #authority, \"file1\", #read )");
            Assert.AreEqual(new Right(new Tuple<string, FactBuilder>("",
                    Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Strings("file1"), Utils.Symbol("read"))))),
                    res);

            Either<Error, Tuple<string, FactBuilder>> res2 = Parser.Fact("right( #authority, $var, #read )");
            //Assert.AreEqual(new Left(new Error("$var, #read )", "variables are not allowed in facts")),
            //    res2);
            Assert.AreEqual(new Left(new Error("$var, #read )", "closing parens not found")),
                    res2);

            Either<Error, Tuple<string, FactBuilder>> res3 = Parser.Fact("date(#ambient,2019-12-02T13:49:53Z)");
            Assert.AreEqual(new Right(new Tuple<string, FactBuilder>("",
                            Utils.Fact("date", Arrays.AsList(Utils.Symbol("ambient"), new Term.Date(1575294593))))),
                    res3);
        }

        [TestMethod]
        public void testRule()
        {
            Either<Error, Tuple<string, RuleBuilder>> res =
                    Parser.Rule("right(#authority, $resource, #read) <- resource( #ambient, $resource), operation(#ambient, #read)");
            Assert.AreEqual(new Right(new Tuple<string, RuleBuilder>("",
                            Utils.Rule("right",
                                    Arrays.AsList(Utils.Symbol("authority"), Utils.Var("resource"), Utils.Symbol("read")),
                                    Arrays.AsList(
                                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource"))),
                                            Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("read"))))
                            ))),
                    res);
        }

        [TestMethod]
        public void testRuleWithExpression()
        {
            Either<Error, Tuple<string, RuleBuilder>> res =
                Parser.Rule("valid_date(\"file1\") <- time(#ambient, $0 ), resource( #ambient, \"file1\"), $0 <= 2019-12-04T09:46:41+00:00");
            Assert.AreEqual(new Right(new Tuple<string, RuleBuilder>("",
                            Utils.ConstrainedRule("valid_date",
                                    Arrays.AsList(Utils.Strings("file1")),
                                    Arrays.AsList(
                                            Utils.Pred("time", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("0"))),
                                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Strings("file1")))
                                            ),
                                    Arrays.AsList<ExpressionBuilder>(
                                            new ExpressionBuilder.Binary(
                                                    ExpressionBuilder.Op.LessOrEqual,
                                                    new ExpressionBuilder.Value(Utils.Var("0")),
                                                    new ExpressionBuilder.Value(new Term.Date(1575452801)))
                                    )
                            ))),
                    res);
        }

        [TestMethod]
        public void testRuleWithExpressionOrdering()
        {
            Either<Error, Tuple<string, RuleBuilder>> res =
                    Parser.Rule("valid_date(\"file1\") <- time(#ambient, $0 ), $0 <= 2019-12-04T09:46:41+00:00, resource( #ambient, \"file1\")");
            Assert.AreEqual(new Right(new Tuple<string, RuleBuilder>("",
                            Utils.ConstrainedRule("valid_date",
                                    Arrays.AsList(Utils.Strings("file1")),
                                    Arrays.AsList(
                                            Utils.Pred("time", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("0"))),
                                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Strings("file1")))
                                    ),
                                    Arrays.AsList<ExpressionBuilder>(
                                            new ExpressionBuilder.Binary(
                                                    ExpressionBuilder.Op.LessOrEqual,
                                                    new ExpressionBuilder.Value(Utils.Var("0")),
                                                    new ExpressionBuilder.Value(new Term.Date(1575452801)))
                                    )
                            ))),
                    res);
        }

        [TestMethod]
        public void testCheck()
        {
            var expectedCheck = new CheckBuilder(Arrays.AsList(
                        Utils.Rule("query",
                                new List<Term>(),
                                Arrays.AsList(
                                        Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("0"))),
                                        Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol("read")))
                                )
                        ),
                        Utils.Rule("query",
                                new List<Term>(),
                                Arrays.AsList(
                                        Utils.Pred("admin", Arrays.AsList(Utils.Symbol("authority")))
                                )
                        )
                        ));
            //var expected = new Right(new Tuple<string, Check>("", check));
            
            Either<Error, Tuple<string, CheckBuilder>> res =
                    Parser.Check("check if resource(#ambient, $0), operation(#ambient, #read) or admin(#authority)");
            
            
            Assert.IsTrue(res.IsRight);

            Assert.AreEqual(expectedCheck, res.Right.Item2);
        }

        [TestMethod]
        public void testExpression()
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res =
                    Parser.Expression(" -1 ");

            Assert.AreEqual(
                    new ExpressionBuilder.Value(Utils.Integer(-1)),
                    res.Right.Item2);

            Either<Error, Tuple<string, ExpressionBuilder>> res2 =
                    Parser.Expression(" $0 <= 2019-12-04T09:46:41+00:00");

            Assert.AreEqual(
                    new ExpressionBuilder.Binary(
                            ExpressionBuilder.Op.LessOrEqual,
                            new ExpressionBuilder.Value(Utils.Var("0")),
                            new ExpressionBuilder.Value(new Term.Date(1575452801))),
                    res2.Right.Item2);

            Either<Error, Tuple<string, ExpressionBuilder>> res3 =
                    Parser.Expression(" 1 < $test + 2 ");

            Assert.AreEqual(
                new ExpressionBuilder.Binary(
                        ExpressionBuilder.Op.LessThan,
                        new ExpressionBuilder.Value(Utils.Integer(1)),
                        new ExpressionBuilder.Binary(
                                ExpressionBuilder.Op.Add,
                                new ExpressionBuilder.Value(Utils.Var("test")),
                                new ExpressionBuilder.Value(Utils.Integer(2))
                        )
                ),
                res3.Right.Item2);

            SymbolTable s3 = new SymbolTable();
            ulong test = s3.Insert("test");
            var expected = Arrays.AsList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(test)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessThan)
                    );
            Assert.IsTrue(expected.SequenceEqual(
                res3.Right.Item2.Convert(s3).GetOps())
            );

            Either<Error, Tuple<string, ExpressionBuilder>> res4 =
                    Parser.Expression("  2 < $test && $var2.starts_with(\"test\") && true ");

            Assert.IsTrue(res4.IsRight);
            Assert.AreEqual(new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.And,
                                    new ExpressionBuilder.Binary(
                                            ExpressionBuilder.Op.And,
                                            new ExpressionBuilder.Binary(
                                                    ExpressionBuilder.Op.LessThan,
                                                    new ExpressionBuilder.Value(Utils.Integer(2)),
                                                    new ExpressionBuilder.Value(Utils.Var("test"))
                                            ),
                                            new ExpressionBuilder.Binary(
                                                    ExpressionBuilder.Op.Prefix,
                                                    new ExpressionBuilder.Value(Utils.Var("var2")),
                                                    new ExpressionBuilder.Value(Utils.Strings("test"))
                                            )
                                    ),
                                    new ExpressionBuilder.Value(new Term.Bool(true))
                            ),
                    res4.Right.Item2);

            Either<Error, Tuple<string, ExpressionBuilder>> res5 =
                Parser.Expression("  [ #abc, #def ].contains($operation) ");

            HashSet<Term> s = new HashSet<Term>();
            s.Add(Utils.Symbol("abc"));
            s.Add(Utils.Symbol("def"));
            Assert.IsTrue(res5.IsRight);
            Assert.AreEqual(new Tuple<string, ExpressionBuilder>("",
                            new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.Contains,
                                    new ExpressionBuilder.Value(Utils.Set(s)),
                                    new ExpressionBuilder.Value(Utils.Var("operation"))
                            )
                    ),
                    res5.Right);
        }

        [TestMethod]
        public void testParens()
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res =
                    Parser.Expression("  1 + 2 * 3  ");

            Assert.AreEqual(new Right(new Tuple<string, ExpressionBuilder>("",
                            new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.Add,
                                    new ExpressionBuilder.Value(Utils.Integer(1)),
                                    new ExpressionBuilder.Binary(
                                            ExpressionBuilder.Op.Mul,
                                            new ExpressionBuilder.Value(Utils.Integer(2)),
                                            new ExpressionBuilder.Value(Utils.Integer(3))
                                    )
                            )
                    )),
                    res);

            ExpressionBuilder e = res.Right.Item2;
            SymbolTable s = new SymbolTable();

            Biscuit.Datalog.Expressions.Expression ex = e.Convert(s);

            Assert.IsTrue(
                    Arrays.AsList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(3)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Mul),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add)
                    ).SequenceEqual(
                    ex.GetOps())
            );

            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            Option<ID> value = ex.Evaluate(variables);
            Assert.AreEqual(Option.Some(new ID.Integer(7)), value);
            Assert.AreEqual("1 + 2 * 3", ex.Print(s).Get());


            Either<Error, Tuple<string, ExpressionBuilder>> res2 =
                    Parser.Expression("  (1 + 2) * 3  ");

            Assert.AreEqual(new ExpressionBuilder.Binary(
                                    ExpressionBuilder.Op.Mul,
                                    new ExpressionBuilder.Unary(
                                            ExpressionBuilder.Op.Parens,
                                            new ExpressionBuilder.Binary(
                                                    ExpressionBuilder.Op.Add,
                                                    new ExpressionBuilder.Value(Utils.Integer(1)),
                                                    new ExpressionBuilder.Value(Utils.Integer(2))
                                            ))
                                    ,
                                    new ExpressionBuilder.Value(Utils.Integer(3))
                            )
                    ,
                    res2.Right.Item2);

            ExpressionBuilder e2 = res2.Right.Item2;
            SymbolTable s2 = new SymbolTable();

            Biscuit.Datalog.Expressions.Expression ex2 = e2.Convert(s2);

            Assert.IsTrue(
                    Arrays.AsList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add),
                            new Biscuit.Datalog.Expressions.Op.Unary(Biscuit.Datalog.Expressions.Op.UnaryOp.Parens),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(3)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Mul)
                    ).SequenceEqual(
                    ex2.GetOps())
            );

            Dictionary<ulong, ID> variables2 = new Dictionary<ulong, ID>();
            Option<ID> value2 = ex2.Evaluate(variables2);
            Assert.AreEqual(Option.Some(new ID.Integer(9)), value2);
            Assert.AreEqual("(1 + 2) * 3", ex2.Print(s2).Get());
        }
    }
}
