using System;

namespace Biscuit.Token.Builder.Parser
{
    public class ExpressionParser
    {
        public static Either<Error, Tuple<string, Expression>> parse(String s)
        {
            return expr(Parser.space(s));
        }

        public static Either<Error, Tuple<string, Expression>> expr(String s)
        {
            Either<Error, Tuple<string, Expression>> res1 = expr1(s);
            if (res1.IsLeft)
            {
                return new Left(res1.Left);
            }
            Tuple<string, Expression> t1 = res1.Right;

            s = t1.Item1;
            Expression e = t1.Item2;

            while (true)
            {
                s = Parser.space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, Expression.Op>> res2 = binary_op0(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, Expression.Op> t2 = res2.Right;
                s = t2.Item1;
                Expression.Op op = t2.Item2;

                s = Parser.space(s);

                Either<Error, Tuple<string, Expression>> res3 = expr1(s);
                if (res3.IsLeft)
                {
                    return new Left(res3.Left);
                }
                Tuple<string, Expression> t3 = res3.Right;

                s = t3.Item1;
                Expression e2 = t3.Item2;

                e = new Expression.Binary(op, e, e2);
            }

            return new Right(new Tuple<string, Expression>(s, e));
        }

        public static Either<Error, Tuple<string, Expression>> expr1(String s)
        {
            Either<Error, Tuple<string, Expression>> res1 = expr2(s);
            if (res1.IsLeft)
            {
                return new Left(res1.Left);
            }
            Tuple<string, Expression> t1 = res1.Right;

            s = t1.Item1;
            Expression e = t1.Item2;

            while (true)
            {
                s = Parser.space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, Expression.Op>> res2 = binary_op1(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, Expression.Op> t2 = res2.Right;
                s = t2.Item1;
                Expression.Op op = t2.Item2;

                s = Parser.space(s);

                Either<Error, Tuple<string, Expression>> res3 = expr2(s);
                if (res3.IsLeft)
                {
                    return new Left(res3.Left);
                }
                Tuple<string, Expression> t3 = res3.Right;

                s = t3.Item1;
                Expression e2 = t3.Item2;

                e = new Expression.Binary(op, e, e2);
            }

            return new Right(new Tuple<string, Expression>(s, e));
        }

        public static Either<Error, Tuple<string, Expression>> expr2(String s)
        {
            Either<Error, Tuple<string, Expression>> res1 = expr3(s);
            if (res1.IsLeft)
            {
                return new Left(res1.Left);
            }
            Tuple<string, Expression> t1 = res1.Right;

            s = t1.Item1;
            Expression e = t1.Item2;

            while (true)
            {
                s = Parser.space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, Expression.Op>> res2 = binary_op2(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, Expression.Op> t2 = res2.Right;
                s = t2.Item1;
                Expression.Op op = t2.Item2;

                s = Parser.space(s);

                Either<Error, Tuple<string, Expression>> res3 = expr3(s);
                if (res3.IsLeft)
                {
                    return new Left(res3.Left);
                }
                Tuple<string, Expression> t3 = res3.Right;

                s = t3.Item1;
                Expression e2 = t3.Item2;

                e = new Expression.Binary(op, e, e2);
            }

            return new Right(new Tuple<string, Expression>(s, e));
        }

        public static Either<Error, Tuple<string, Expression>> expr3(String s)
        {
            Either<Error, Tuple<string, Expression>> res1 = expr4(s);
            if (res1.IsLeft)
            {
                return new Left(res1.Left);
            }
            Tuple<string, Expression> t1 = res1.Right;

            s = t1.Item1;
            Expression e = t1.Item2;

            while (true)
            {
                s = Parser.space(s);
                if (s.Length == 0)
                {
                    break;
                }

                Either<Error, Tuple<string, Expression.Op>> res2 = binary_op3(s);
                if (res2.IsLeft)
                {
                    break;
                }
                Tuple<string, Expression.Op> t2 = res2.Right;
                s = t2.Item1;
                Expression.Op op = t2.Item2;

                s = Parser.space(s);

                Either<Error, Tuple<string, Expression>> res3 = expr4(s);
                if (res3.IsLeft)
                {
                    return new Left(res3.Left);
                }
                Tuple<string, Expression> t3 = res3.Right;

                s = t3.Item1;
                Expression e2 = t3.Item2;

                e = new Expression.Binary(op, e, e2);
            }

            return new Right(new Tuple<string, Expression>(s, e));
        }

        public static Either<Error, Tuple<string, Expression>> expr4(String s)
        {
            Either<Error, Tuple<string, Expression>> res1 = expr_term(s);
            if (res1.IsLeft)
            {
                return new Left(res1.Left);
            }
            Tuple<string, Expression> t1 = res1.Right;
            s = Parser.space(t1.Item1);
            Expression e1 = t1.Item2;

            if (!s.StartsWith("."))
            {
                return new Right(new Tuple<string, Expression>(s, e1));
            }
            s = s.Substring(1);

            Either<Error, Tuple<string, Expression.Op>> res2 = binary_op4(s);
            if (res2.IsLeft)
            {
                return new Left(res2.Left);
            }
            Tuple<string, Expression.Op> t2 = res2.Right;
            s = Parser.space(t2.Item1);
            Expression.Op op = t2.Item2;

            if (!s.StartsWith("("))
            {
                return new Left(new Error(s, "missing ("));
            }

            s = Parser.space(s.Substring(1));

            Either<Error, Tuple<string, Expression>> res3 = expr(s);
            if (res3.IsLeft)
            {
                return new Left(res3.Left);
            }

            Tuple<string, Expression> t3 = res3.Right;

            s = Parser.space(t3.Item1);
            if (!s.StartsWith(")"))
            {
                return new Left(new Error(s, "missing )"));
            }
            s = Parser.space(s.Substring(1));
            Expression e2 = t3.Item2;

            return new Right(new Tuple<string, Expression>(s, new Expression.Binary(op, e1, e2)));
        }

        public static Either<Error, Tuple<string, Expression>> expr_term(string s)
        {
            Either<Error, Tuple<string, Expression>> res1 = unary(s);
            if (res1.IsRight)
            {
                return res1;
            }

            Either<Error, Tuple<string, Term>> res2 = Parser.term(s);
            if (res2.IsLeft)
            {
                return new Left(res2.Left);
            }
            Tuple<string, Term> t2 = res2.Right;
            Expression e = new Expression.Value(t2.Item2);

            return new Right(new Tuple<string, Expression>(t2.Item1, e));
        }

        public static Either<Error, Tuple<string, Expression>> unary(String s)
        {
            s = Parser.space(s);

            if (s.StartsWith("!"))
            {
                s = Parser.space(s.Substring(1));

                Either<Error, Tuple<string, Expression>> resultExpression = expr(s);
                if (resultExpression.IsLeft)
                {
                    return new Left(resultExpression.Left);
                }

                Tuple<string, Expression> t = resultExpression.Right;
                return new Right(new Tuple<string, Expression>(t.Item1, new Expression.Unary(Expression.Op.Negate, t.Item2)));
            }


            if (s.StartsWith("("))
            {
                Either<Error, Tuple<string, Expression>> unaryParens = unary_parens(s);
                if (unaryParens.IsLeft)
                {
                    return new Left(unaryParens.Left);
                }

                Tuple<string, Expression> t = unaryParens.Right;

                s = Parser.space(s.Substring(1));
                return new Right(new Tuple<string, Expression>(t.Item1, t.Item2));
            }

            Expression e;
            Either<Error, Tuple<string, Term>> res = Parser.term(s);
            if (res.IsRight)
            {
                Tuple<string, Term> t = res.Right;
                s = Parser.space(t.Item1);
                e = new Expression.Value(t.Item2);
            }
            else
            {
                Either<Error, Tuple<string, Expression>> res2 = unary_parens(s);
                if (res2.IsLeft)
                {
                    return new Left(res2.Left);
                }

                Tuple<string, Expression> t = res2.Right;
                s = Parser.space(t.Item1);
                e = t.Item2;
            }

            if (s.StartsWith(".Length"))
            {
                s = Parser.space(s.Substring(9));
                return new Right(new Tuple<string, Expression>(s, new Expression.Unary(Expression.Op.Length, e)));
            }
            else
            {
                return new Left(new Error(s, "unexpected token"));
            }
        }

        public static Either<Error, Tuple<string, Expression>> unary_parens(String s)
        {
            if (s.StartsWith("("))
            {
                s = Parser.space(s.Substring(1));

                Either<Error, Tuple<string, Expression>> res = expr(s);
                if (res.IsLeft)
                {
                    return new Left(res.Left);
                }

                Tuple<string, Expression> t = res.Right;

                s = Parser.space(t.Item1);
                if (!s.StartsWith(")"))
                {
                    return new Left(new Error(s, "missing )"));
                }

                s = Parser.space(s.Substring(1));
                return new Right(new Tuple<string, Expression>(s, new Expression.Unary(Expression.Op.Parens, t.Item2)));
            }
            else
            {
                return new Left(new Error(s, "missing ("));
            }
        }

        public static Either<Error, Tuple<string, Expression.Op>> binary_op0(String s)
        {
            if (s.StartsWith("&&"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(2), Expression.Op.And));
            }
            if (s.StartsWith("||"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(2), Expression.Op.Or));
            }

            return new Left(new Error(s, "unrecognized op"));
        }

        public static Either<Error, Tuple<string, Expression.Op>> binary_op1(String s)
        {
            if (s.StartsWith("<="))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(2), Expression.Op.LessOrEqual));
            }
            if (s.StartsWith(">="))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(2), Expression.Op.GreaterOrEqual));
            }
            if (s.StartsWith("<"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.LessThan));
            }
            if (s.StartsWith(">"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.GreaterThan));
            }
            if (s.StartsWith("=="))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(2), Expression.Op.Equal));
            }

            return new Left(new Error(s, "unrecognized op"));
        }

        public static Either<Error, Tuple<string, Expression.Op>> binary_op2(String s)
        {

            if (s.StartsWith("+"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.Add));
            }
            if (s.StartsWith("-"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.Sub));
            }

            return new Left(new Error(s, "unrecognized op"));
        }

        public static Either<Error, Tuple<string, Expression.Op>> binary_op3(String s)
        {
            if (s.StartsWith("*"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.Mul));
            }
            if (s.StartsWith("/"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(1), Expression.Op.Div));
            }

            return new Left(new Error(s, "unrecognized op"));
        }

        public static Either<Error, Tuple<string, Expression.Op>> binary_op4(String s)
        {
            if (s.StartsWith("contains"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(8), Expression.Op.Contains));
            }
            if (s.StartsWith("starts_with"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(11), Expression.Op.Prefix));
            }
            if (s.StartsWith("ends_with"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(9), Expression.Op.Suffix));
            }
            if (s.StartsWith("matches"))
            {
                return new Right(new Tuple<string, Expression.Op>(s.Substring(7), Expression.Op.Regex));
            }

            return new Left(new Error(s, "unrecognized op"));
        }
    }

}
