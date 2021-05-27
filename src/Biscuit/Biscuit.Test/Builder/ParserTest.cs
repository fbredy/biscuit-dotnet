using Biscuit.Datalog;
using Biscuit.Token.Builder;
using Biscuit.Token.Builder.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Check = Biscuit.Token.Builder.Check;
using Fact = Biscuit.Token.Builder.Fact;
using Parser = Biscuit.Token.Builder.Parser.Parser;
using Rule = Biscuit.Token.Builder.Rule;

namespace Biscuit.Test.Builder
{
    [TestClass]
    public class ParserTest
    {

        [TestMethod]
        public void testName()
        {
            Either<Error, Tuple<string, string>> res = Parser.name("operation(#ambient, #read)");
            Assert.AreEqual(new Right(new Tuple<string, string>("(#ambient, #read)", "operation")), res);
        }

        [TestMethod]
        public void testSymbol()
        {
            Either<Error, Tuple<string, Term.Symbol>> res = Parser.symbol("#ambient");
            Assert.AreEqual(new Right(new Tuple<string, Term.Symbol>("", (Term.Symbol)Utils.s("ambient"))), res);
        }

        [TestMethod]
        public void testString()
        {
            Either<Error, Tuple<string, Term.Str>> res = Parser.strings("\"file1 a hello - 123_\"");
            Assert.AreEqual(new Right(new Tuple<string, Term.Str>("", (Term.Str)Utils.strings("file1 a hello - 123_"))), res);
        }

        [TestMethod]
        public void testInteger()
        {
            Either<Error, Tuple<string, Term.Integer>> res = Parser.integer("123");
            Assert.AreEqual(new Right(new Tuple<string, Term.Integer>("", (Term.Integer)Utils.integer(123))), res);

            Either<Error, Tuple<string, Term.Integer>> res2 = Parser.integer("-42");
            Assert.AreEqual(new Right(new Tuple<string, Term.Integer>("", (Term.Integer)Utils.integer(-42))), res2);
        }

        [TestMethod]
        public void testDate()
        {
            Either<Error, Tuple<string, Term.Date>> res = Parser.date("2019-12-02T13:49:53Z,");
            Assert.AreEqual(new Right(new Tuple<string, Term.Date>(",", new Term.Date(1575294593))), res);
        }

        [TestMethod]
        public void testVariable()
        {
            Either<Error, Tuple<string, Term.Variable>> res = Parser.variable("$name");
            Assert.AreEqual(new Right(new Tuple<string, Term.Variable>("", (Term.Variable)Utils.var("name"))), res);
        }

        public void testConstraint()
        {
        }

        [TestMethod]
        public void testFact()
        {
            Either<Error, Tuple<string, Fact>> res = Biscuit.Token.Builder.Parser.Parser.fact("right( #authority, \"file1\", #read )");
            Assert.AreEqual(new Right(new Tuple<string, Fact>("",
                    Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.strings("file1"), Utils.s("read"))))),
                    res);

            Either<Error, Tuple<string, Fact>> res2 = Parser.fact("right( #authority, $var, #read )");
            //Assert.AreEqual(new Left(new Error("$var, #read )", "variables are not allowed in facts")),
            //    res2);
            Assert.AreEqual(new Left(new Error("$var, #read )", "closing parens not found")),
                    res2);

            Either<Error, Tuple<string, Fact>> res3 = Parser.fact("date(#ambient,2019-12-02T13:49:53Z)");
            Assert.AreEqual(new Right(new Tuple<string, Fact>("",
                            Utils.fact("date", Arrays.asList(Utils.s("ambient"), new Term.Date(1575294593))))),
                    res3);
        }

        [TestMethod]
        public void testRule()
        {
            Either<Error, Tuple<string, Rule>> res =
                    Parser.rule("right(#authority, $resource, #read) <- resource( #ambient, $resource), operation(#ambient, #read)");
            Assert.AreEqual(new Right(new Tuple<string, Rule>("",
                            Utils.rule("right",
                                    Arrays.asList(Utils.s("authority"), Utils.var("resource"), Utils.s("read")),
                                    Arrays.asList(
                                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource"))),
                                            Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.s("read"))))
                            ))),
                    res);
        }

        [TestMethod]
        public void testRuleWithExpression()
        {
            Either<Error, Tuple<string, Rule>> res =
                Parser.rule("valid_date(\"file1\") <- time(#ambient, $0 ), resource( #ambient, \"file1\"), $0 <= 2019-12-04T09:46:41+00:00");
            Assert.AreEqual(new Right(new Tuple<string, Rule>("",
                            Utils.constrained_rule("valid_date",
                                    Arrays.asList(Utils.strings("file1")),
                                    Arrays.asList(
                                            Utils.pred("time", Arrays.asList(Utils.s("ambient"), Utils.var("0"))),
                                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.strings("file1")))
                                            ),
                                    Arrays.asList<Expression>(
                                            new Expression.Binary(
                                                    Expression.Op.LessOrEqual,
                                                    new Expression.Value(Utils.var("0")),
                                                    new Expression.Value(new Term.Date(1575452801)))
                                    )
                            ))),
                    res);
        }

        [TestMethod]
        public void testRuleWithExpressionOrdering()
        {
            Either<Error, Tuple<string, Rule>> res =
                    Parser.rule("valid_date(\"file1\") <- time(#ambient, $0 ), $0 <= 2019-12-04T09:46:41+00:00, resource( #ambient, \"file1\")");
            Assert.AreEqual(new Right(new Tuple<string, Rule>("",
                            Utils.constrained_rule("valid_date",
                                    Arrays.asList(Utils.strings("file1")),
                                    Arrays.asList(
                                            Utils.pred("time", Arrays.asList(Utils.s("ambient"), Utils.var("0"))),
                                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.strings("file1")))
                                    ),
                                    Arrays.asList<Expression>(
                                            new Expression.Binary(
                                                    Expression.Op.LessOrEqual,
                                                    new Expression.Value(Utils.var("0")),
                                                    new Expression.Value(new Term.Date(1575452801)))
                                    )
                            ))),
                    res);
        }

        [TestMethod]
        public void testCheck()
        {
            var expectedCheck = new Check(Arrays.asList(
                        Utils.rule("query",
                                new List<Term>(),
                                Arrays.asList(
                                        Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("0"))),
                                        Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.s("read")))
                                )
                        ),
                        Utils.rule("query",
                                new List<Term>(),
                                Arrays.asList(
                                        Utils.pred("admin", Arrays.asList(Utils.s("authority")))
                                )
                        )
                        ));
            //var expected = new Right(new Tuple<string, Check>("", check));
            
            Either<Error, Tuple<string, Check>> res =
                    Parser.Check("check if resource(#ambient, $0), operation(#ambient, #read) or admin(#authority)");
            
            
            Assert.IsTrue(res.IsRight);

            Assert.AreEqual(expectedCheck, res.Right.Item2);
        }

        [TestMethod]
        public void testExpression()
        {
            Either<Error, Tuple<string, Expression>> res =
                    Parser.expression(" -1 ");

            Assert.AreEqual(
                    new Expression.Value(Utils.integer(-1)),
                    res.Right.Item2);

            Either<Error, Tuple<string, Expression>> res2 =
                    Parser.expression(" $0 <= 2019-12-04T09:46:41+00:00");

            Assert.AreEqual(
                    new Expression.Binary(
                            Expression.Op.LessOrEqual,
                            new Expression.Value(Utils.var("0")),
                            new Expression.Value(new Term.Date(1575452801))),
                    res2.Right.Item2);

            Either<Error, Tuple<string, Expression>> res3 =
                    Parser.expression(" 1 < $test + 2 ");

            Assert.AreEqual(
                new Expression.Binary(
                        Expression.Op.LessThan,
                        new Expression.Value(Utils.integer(1)),
                        new Expression.Binary(
                                Expression.Op.Add,
                                new Expression.Value(Utils.var("test")),
                                new Expression.Value(Utils.integer(2))
                        )
                ),
                res3.Right.Item2);

            SymbolTable s3 = new SymbolTable();
            ulong test = s3.insert("test");
            var expected = Arrays.asList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(test)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessThan)
                    );
            Assert.IsTrue(expected.SequenceEqual(
                res3.Right.Item2.convert(s3).getOps())
            );

            Either<Error, Tuple<string, Expression>> res4 =
                    Parser.expression("  2 < $test && $var2.starts_with(\"test\") && true ");

            Assert.IsTrue(res4.IsRight);
            Assert.AreEqual(new Expression.Binary(
                                    Expression.Op.And,
                                    new Expression.Binary(
                                            Expression.Op.And,
                                            new Expression.Binary(
                                                    Expression.Op.LessThan,
                                                    new Expression.Value(Utils.integer(2)),
                                                    new Expression.Value(Utils.var("test"))
                                            ),
                                            new Expression.Binary(
                                                    Expression.Op.Prefix,
                                                    new Expression.Value(Utils.var("var2")),
                                                    new Expression.Value(Utils.strings("test"))
                                            )
                                    ),
                                    new Expression.Value(new Term.Bool(true))
                            ),
                    res4.Right.Item2);

            Either<Error, Tuple<string, Expression>> res5 =
                Parser.expression("  [ #abc, #def ].contains($operation) ");

            HashSet<Term> s = new HashSet<Term>();
            s.Add(Utils.s("abc"));
            s.Add(Utils.s("def"));
            Assert.IsTrue(res5.IsRight);
            Assert.AreEqual(new Tuple<string, Expression>("",
                            new Expression.Binary(
                                    Expression.Op.Contains,
                                    new Expression.Value(Utils.set(s)),
                                    new Expression.Value(Utils.var("operation"))
                            )
                    ),
                    res5.Right);
        }

        [TestMethod]
        public void testParens()
        {
            Either<Error, Tuple<string, Expression>> res =
                    Parser.expression("  1 + 2 * 3  ");

            Assert.AreEqual(new Right(new Tuple<string, Expression>("",
                            new Expression.Binary(
                                    Expression.Op.Add,
                                    new Expression.Value(Utils.integer(1)),
                                    new Expression.Binary(
                                            Expression.Op.Mul,
                                            new Expression.Value(Utils.integer(2)),
                                            new Expression.Value(Utils.integer(3))
                                    )
                            )
                    )),
                    res);

            Expression e = res.Right.Item2;
            SymbolTable s = new SymbolTable();

            Biscuit.Datalog.Expressions.Expression ex = e.convert(s);

            Assert.IsTrue(
                    Arrays.asList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(3)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Mul),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add)
                    ).SequenceEqual(
                    ex.getOps())
            );

            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            Option<ID> value = ex.evaluate(variables);
            Assert.AreEqual(Option.some(new ID.Integer(7)), value);
            Assert.AreEqual("1 + 2 * 3", ex.print(s).get());


            Either<Error, Tuple<string, Expression>> res2 =
                    Parser.expression("  (1 + 2) * 3  ");

            Assert.AreEqual(new Expression.Binary(
                                    Expression.Op.Mul,
                                    new Expression.Unary(
                                            Expression.Op.Parens,
                                            new Expression.Binary(
                                                    Expression.Op.Add,
                                                    new Expression.Value(Utils.integer(1)),
                                                    new Expression.Value(Utils.integer(2))
                                            ))
                                    ,
                                    new Expression.Value(Utils.integer(3))
                            )
                    ,
                    res2.Right.Item2);

            Expression e2 = res2.Right.Item2;
            SymbolTable s2 = new SymbolTable();

            Biscuit.Datalog.Expressions.Expression ex2 = e2.convert(s2);

            Assert.IsTrue(
                    Arrays.asList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(2)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Add),
                            new Biscuit.Datalog.Expressions.Op.Unary(Biscuit.Datalog.Expressions.Op.UnaryOp.Parens),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(3)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Mul)
                    ).SequenceEqual(
                    ex2.getOps())
            );

            Dictionary<ulong, ID> variables2 = new Dictionary<ulong, ID>();
            Option<ID> value2 = ex2.evaluate(variables2);
            Assert.AreEqual(Option.some(new ID.Integer(9)), value2);
            Assert.AreEqual("(1 + 2) * 3", ex2.print(s2).get());
        }
    }
}
