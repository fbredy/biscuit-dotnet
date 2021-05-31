using System;

namespace Biscuit.Token.Builder.Parser
{
    public class ExpressionParser
    {
        public static Either<Error, Tuple<string, ExpressionBuilder>> Parse(string s)
        {
            return Expr(Parser.Space(s));
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expr(string str)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = Expr1(str);
            if (res1.IsLeft)
            {
                return res1.Left;
            }

            var t1 = res1.Right;

            str = t1.Item1;
            ExpressionBuilder builder = t1.Item2;

            while (true)
            {
                str = Parser.Space(str);
                if (str.IsEmpty())
                {
                    break;
                }

                Either<Error, Tuple<string, ExpressionBuilder.Op>> res2 = BinaryOp(str);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, ExpressionBuilder.Op> t2 = res2.Right;
                str = t2.Item1;
                ExpressionBuilder.Op op = t2.Item2;

                str = Parser.Space(str);

                Either<Error, Tuple<string, ExpressionBuilder>> res3 = Expr1(str);
                if (res3.IsLeft)
                {
                    return res3.Left;
                }
                Tuple<string, ExpressionBuilder> t3 = res3.Right;

                str = t3.Item1;
                ExpressionBuilder e2 = t3.Item2;

                builder = new ExpressionBuilder.Binary(op, builder, e2);
            }

            return new Tuple<string, ExpressionBuilder>(str, builder);
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expr1(string s)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = Expr2(s);
            if (res1.IsLeft)
            {
                return res1.Left;
            }
            Tuple<string, ExpressionBuilder> t1 = res1.Right;

            s = t1.Item1;
            ExpressionBuilder e = t1.Item2;

            while (true)
            {
                s = Parser.Space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, ExpressionBuilder.Op>> res2 = BinaryOp1(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, ExpressionBuilder.Op> t2 = res2.Right;
                s = t2.Item1;
                ExpressionBuilder.Op op = t2.Item2;

                s = Parser.Space(s);

                Either<Error, Tuple<string, ExpressionBuilder>> res3 = Expr2(s);
                if (res3.IsLeft)
                {
                    return res3.Left;
                }
                Tuple<string, ExpressionBuilder> t3 = res3.Right;

                s = t3.Item1;
                ExpressionBuilder e2 = t3.Item2;

                e = new ExpressionBuilder.Binary(op, e, e2);
            }

            return new Tuple<string, ExpressionBuilder>(s, e);
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expr2(string s)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = Expr3(s);
            if (res1.IsLeft)
            {
                return res1.Left;
            }
            Tuple<string, ExpressionBuilder> t1 = res1.Right;

            s = t1.Item1;
            ExpressionBuilder e = t1.Item2;

            while (true)
            {
                s = Parser.Space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, ExpressionBuilder.Op>> res2 = BinaryOp2(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, ExpressionBuilder.Op> t2 = res2.Right;
                s = t2.Item1;
                ExpressionBuilder.Op op = t2.Item2;

                s = Parser.Space(s);

                Either<Error, Tuple<string, ExpressionBuilder>> res3 = Expr3(s);
                if (res3.IsLeft)
                {
                    return res3.Left;
                }
                Tuple<string, ExpressionBuilder> t3 = res3.Right;

                s = t3.Item1;
                ExpressionBuilder e2 = t3.Item2;

                e = new ExpressionBuilder.Binary(op, e, e2);
            }

            return new Tuple<string, ExpressionBuilder>(s, e);
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expr3(string s)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = Expr4(s);
            if (res1.IsLeft)
            {
                return res1.Left;
            }
            Tuple<string, ExpressionBuilder> t1 = res1.Right;

            s = t1.Item1;
            ExpressionBuilder e = t1.Item2;

            while (true)
            {
                s = Parser.Space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, ExpressionBuilder.Op>> res2 = BinaryOp3(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, ExpressionBuilder.Op> t2 = res2.Right;
                s = t2.Item1;
                ExpressionBuilder.Op op = t2.Item2;

                s = Parser.Space(s);

                Either<Error, Tuple<string, ExpressionBuilder>> res3 = Expr4(s);
                if (res3.IsLeft)
                {
                    return res3.Left;
                }
                Tuple<string, ExpressionBuilder> t3 = res3.Right;

                s = t3.Item1;
                ExpressionBuilder e2 = t3.Item2;

                e = new ExpressionBuilder.Binary(op, e, e2);
            }

            return new Tuple<string, ExpressionBuilder>(s, e);
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expr4(string s)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = ExprTerm(s);
            if (res1.IsLeft)
            {
                return res1.Left;
            }
            Tuple<string, ExpressionBuilder> t1 = res1.Right;
            s = Parser.Space(t1.Item1);
            ExpressionBuilder e1 = t1.Item2;

            if (!s.StartsWith("."))
            {
                return new Tuple<string, ExpressionBuilder>(s, e1);
            }
            s = s.Substring(1);

            Either<Error, Tuple<string, ExpressionBuilder.Op>> res2 = BinaryOp4(s);
            if (res2.IsLeft)
            {
                return res2.Left;
            }
            Tuple<string, ExpressionBuilder.Op> t2 = res2.Right;
            s = Parser.Space(t2.Item1);
            ExpressionBuilder.Op op = t2.Item2;

            if (!s.StartsWith("("))
            {
                return new Error(s, "missing (");
            }

            s = Parser.Space(s.Substring(1));

            Either<Error, Tuple<string, ExpressionBuilder>> res3 = Expr(s);
            if (res3.IsLeft)
            {
                return res3.Left;
            }

            Tuple<string, ExpressionBuilder> t3 = res3.Right;

            s = Parser.Space(t3.Item1);
            if (!s.StartsWith(")"))
            {
                return new Error(s, "missing )");
            }
            s = Parser.Space(s.Substring(1));
            ExpressionBuilder e2 = t3.Item2;

            return new Tuple<string, ExpressionBuilder>(s, new ExpressionBuilder.Binary(op, e1, e2));
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> ExprTerm(string s)
        {
            Either<Error, Tuple<string, ExpressionBuilder>> res1 = Unary(s);
            if (res1.IsRight)
            {
                return res1;
            }

            Either<Error, Tuple<string, Term>> res2 = Parser.Term(s);
            if (res2.IsLeft)
            {
                return res2.Left;
            }
            Tuple<string, Term> t2 = res2.Right;
            ExpressionBuilder e = new ExpressionBuilder.Value(t2.Item2);

            return new Tuple<string, ExpressionBuilder>(t2.Item1, e);
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Unary(string s)
        {
            s = Parser.Space(s);

            if (s.StartsWith("!"))
            {
                s = Parser.Space(s.Substring(1));

                Either<Error, Tuple<string, ExpressionBuilder>> resultExpression = Expr(s);
                if (resultExpression.IsLeft)
                {
                    return resultExpression.Left;
                }

                Tuple<string, ExpressionBuilder> t = resultExpression.Right;
                return new Tuple<string, ExpressionBuilder>(t.Item1, new ExpressionBuilder.Unary(ExpressionBuilder.Op.Negate, t.Item2));
            }


            if (s.StartsWith("("))
            {
                Either<Error, Tuple<string, ExpressionBuilder>> unaryParens = UnaryParens(s);
                if (unaryParens.IsLeft)
                {
                    return unaryParens.Left;
                }

                Tuple<string, ExpressionBuilder> t = unaryParens.Right;

                s = Parser.Space(s.Substring(1));
                return new Tuple<string, ExpressionBuilder>(t.Item1, t.Item2);
            }

            ExpressionBuilder e;
            Either<Error, Tuple<string, Term>> res = Parser.Term(s);
            if (res.IsRight)
            {
                Tuple<string, Term> t = res.Right;
                s = Parser.Space(t.Item1);
                e = new ExpressionBuilder.Value(t.Item2);
            }
            else
            {
                Either<Error, Tuple<string, ExpressionBuilder>> res2 = UnaryParens(s);
                if (res2.IsLeft)
                {
                    return res2.Left;
                }

                Tuple<string, ExpressionBuilder> t = res2.Right;
                s = Parser.Space(t.Item1);
                e = t.Item2;
            }

            if (s.StartsWith(".Length"))
            {
                s = Parser.Space(s.Substring(9));
                return new Tuple<string, ExpressionBuilder>(s, new ExpressionBuilder.Unary(ExpressionBuilder.Op.Length, e));
            }
            else
            {
                return new Error(s, "unexpected token");
            }
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> UnaryParens(string s)
        {
            if (s.StartsWith("("))
            {
                s = Parser.Space(s.Substring(1));

                Either<Error, Tuple<string, ExpressionBuilder>> res = Expr(s);
                if (res.IsLeft)
                {
                    return res.Left;
                }

                Tuple<string, ExpressionBuilder> t = res.Right;

                s = Parser.Space(t.Item1);
                if (!s.StartsWith(")"))
                {
                    return new Error(s, "missing )");
                }

                s = Parser.Space(s.Substring(1));
                return new Tuple<string, ExpressionBuilder>(s, new ExpressionBuilder.Unary(ExpressionBuilder.Op.Parens, t.Item2));
            }
            else
            {
                return new Error(s, "missing (");
            }
        }

        public static Either<Error, Tuple<string, ExpressionBuilder.Op>> BinaryOp(string s)
        {
            if (s.StartsWith("&&"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(2), ExpressionBuilder.Op.And);
            }
            if (s.StartsWith("||"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(2), ExpressionBuilder.Op.Or);
            }

            return new Error(s, "unrecognized op");
        }

        public static Either<Error, Tuple<string, ExpressionBuilder.Op>> BinaryOp1(string s)
        {
            if (s.StartsWith("<="))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(2), ExpressionBuilder.Op.LessOrEqual);
            }
            if (s.StartsWith(">="))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(2), ExpressionBuilder.Op.GreaterOrEqual);
            }
            if (s.StartsWith("<"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.LessThan);
            }
            if (s.StartsWith(">"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.GreaterThan);
            }
            if (s.StartsWith("=="))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(2), ExpressionBuilder.Op.Equal);
            }

            return new Error(s, "unrecognized op");
        }

        public static Either<Error, Tuple<string, ExpressionBuilder.Op>> BinaryOp2(string s)
        {

            if (s.StartsWith("+"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.Add);
            }
            if (s.StartsWith("-"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.Sub);
            }

            return new Error(s, "unrecognized op");
        }

        public static Either<Error, Tuple<string, ExpressionBuilder.Op>> BinaryOp3(string s)
        {
            if (s.StartsWith("*"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.Mul);
            }
            if (s.StartsWith("/"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(1), ExpressionBuilder.Op.Div);
            }

            return new Error(s, "unrecognized op");
        }

        public static Either<Error, Tuple<string, ExpressionBuilder.Op>> BinaryOp4(string s)
        {
            if (s.StartsWith("contains"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(8), ExpressionBuilder.Op.Contains);
            }
            if (s.StartsWith("starts_with"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(11), ExpressionBuilder.Op.Prefix);
            }
            if (s.StartsWith("ends_with"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(9), ExpressionBuilder.Op.Suffix);
            }
            if (s.StartsWith("matches"))
            {
                return new Tuple<string, ExpressionBuilder.Op>(s.Substring(7), ExpressionBuilder.Op.Regex);
            }

            return new Error(s, "unrecognized op");
        }
    }

}
