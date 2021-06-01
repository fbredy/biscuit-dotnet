using Biscuit.Datalog.Expressions;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public abstract class ExpressionBuilder
    {
        public Expression Convert(Datalog.SymbolTable symbols)
        {
            List<Datalog.Expressions.Op> ops = new List<Datalog.Expressions.Op>();
            this.ToOpcodes(symbols, ops);

            return new Expression(ops);
        }

        public static ExpressionBuilder ConvertFrom(Expression e, Datalog.SymbolTable symbols)
        {
            List<Op> ops = new List<Op>();
            Stack<ExpressionBuilder> stack = new Stack<ExpressionBuilder>(16);
            foreach (Datalog.Expressions.Op op in e.GetOps())
            {
                if (op is Datalog.Expressions.Op.Value)
                {
                    Datalog.Expressions.Op.Value v = (Datalog.Expressions.Op.Value)op;
                    stack.Push(new ExpressionBuilder.Value(Term.ConvertFrom(v.GetValue(), symbols)));
                }
                else if (op is Datalog.Expressions.Op.Unary)
                {
                    Datalog.Expressions.Op.Unary v = (Datalog.Expressions.Op.Unary)op;
                    ExpressionBuilder e1 = stack.Pop();

                    switch (v.GetOp())
                    {
                        case Datalog.Expressions.Op.UnaryOp.Length:
                            stack.Push(new ExpressionBuilder.Unary(Op.Length, e1));
                            break;
                        case Datalog.Expressions.Op.UnaryOp.Negate:
                            stack.Push(new ExpressionBuilder.Unary(Op.Negate, e1));
                            break;
                        case Datalog.Expressions.Op.UnaryOp.Parens:
                            stack.Push(new ExpressionBuilder.Unary(Op.Parens, e1));
                            break;
                        default:
                            return null;
                    }
                }
                else if (op is Datalog.Expressions.Op.Binary)
                {
                    Datalog.Expressions.Op.Binary v = (Datalog.Expressions.Op.Binary)op;
                    ExpressionBuilder e1 = stack.Pop();
                    ExpressionBuilder e2 = stack.Pop();

                    switch (v.GetOp())
                    {
                        case Datalog.Expressions.Op.BinaryOp.LessThan:
                            stack.Push(new ExpressionBuilder.Binary(Op.LessThan, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.GreaterThan:
                            stack.Push(new ExpressionBuilder.Binary(Op.GreaterThan, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.LessOrEqual:
                            stack.Push(new ExpressionBuilder.Binary(Op.LessOrEqual, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.GreaterOrEqual:
                            stack.Push(new ExpressionBuilder.Binary(Op.GreaterOrEqual, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Equal:
                            stack.Push(new ExpressionBuilder.Binary(Op.Equal, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Contains:
                            stack.Push(new ExpressionBuilder.Binary(Op.Contains, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Prefix:
                            stack.Push(new ExpressionBuilder.Binary(Op.Prefix, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Suffix:
                            stack.Push(new ExpressionBuilder.Binary(Op.Suffix, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Regex:
                            stack.Push(new ExpressionBuilder.Binary(Op.Regex, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Add:
                            stack.Push(new ExpressionBuilder.Binary(Op.Add, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Sub:
                            stack.Push(new ExpressionBuilder.Binary(Op.Sub, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Mul:
                            stack.Push(new ExpressionBuilder.Binary(Op.Mul, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Div:
                            stack.Push(new ExpressionBuilder.Binary(Op.Div, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.And:
                            stack.Push(new ExpressionBuilder.Binary(Op.And, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Or:
                            stack.Push(new ExpressionBuilder.Binary(Op.Or, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Intersection:
                            stack.Push(new ExpressionBuilder.Binary(Op.Intersection, e1, e2));
                            break;
                        case Datalog.Expressions.Op.BinaryOp.Union:
                            stack.Push(new ExpressionBuilder.Binary(Op.Union, e1, e2));
                            break;
                        default:
                            return null;
                    }
                }
            }

            return stack.Pop();
        }

        public abstract void ToOpcodes(Datalog.SymbolTable symbols, List<Datalog.Expressions.Op> ops);

        public enum Op
        {
            Negate,
            Parens,
            LessThan,
            GreaterThan,
            LessOrEqual,
            GreaterOrEqual,
            Equal,
            Contains,
            Prefix,
            Suffix,
            Regex,
            Add,
            Sub,
            Mul,
            Div,
            And,
            Or,
            Length,
            Intersection,
            Union,
        }

        public class Value : ExpressionBuilder
        {
            private readonly Term value;

            public Value(Term value)
            {
                this.value = value;
            }

            public override void ToOpcodes(Datalog.SymbolTable symbols, List<Datalog.Expressions.Op> ops)
            {
                ops.Add(new Datalog.Expressions.Op.Value(this.value.Convert(symbols)));
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Value value1 = (Value)o;

                return value != null ? value.Equals(value1.value) : value1.value == null;
            }


            public override int GetHashCode()
            {
                return value != null ? value.GetHashCode() : 0;
            }


            public override string ToString()
            {
                return "Value{" + "value=" + value + '}';
            }
        }

        public class Unary : ExpressionBuilder
        {
            private readonly Op op;
            private readonly ExpressionBuilder arg1;

            public Unary(Op op, ExpressionBuilder arg1)
            {
                this.op = op;
                this.arg1 = arg1;
            }

            public override void ToOpcodes(Datalog.SymbolTable symbols, List<Datalog.Expressions.Op> ops)
            {
                this.arg1.ToOpcodes(symbols, ops);

                switch (this.op)
                {
                    case Op.Negate:
                        ops.Add(new Datalog.Expressions.Op.Unary(Datalog.Expressions.Op.UnaryOp.Negate));
                        break;
                    case Op.Parens:
                        ops.Add(new Datalog.Expressions.Op.Unary(Datalog.Expressions.Op.UnaryOp.Parens));
                        break;
                    case Op.Length:
                        ops.Add(new Datalog.Expressions.Op.Unary(Datalog.Expressions.Op.UnaryOp.Length));
                        break;
                }
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Unary unary = (Unary)o;

                if (op != unary.op) return false;
                return arg1.Equals(unary.arg1);
            }


            public override int GetHashCode()
            {
                int result = op.GetHashCode();
                result = 31 * result + arg1.GetHashCode();
                return result;
            }

            public override string ToString()
            {
                return "Unary{op=" + op + ", arg1=" + arg1 + '}';
            }
        }

        public class Binary : ExpressionBuilder
        {
            private readonly Op op;
            private readonly ExpressionBuilder arg1;
            private readonly ExpressionBuilder arg2;

            public Binary(Op op, ExpressionBuilder arg1, ExpressionBuilder arg2)
            {
                this.op = op;
                this.arg1 = arg1;
                this.arg2 = arg2;
            }

            public override void ToOpcodes(Datalog.SymbolTable symbols, List<Datalog.Expressions.Op> ops)
            {
                this.arg1.ToOpcodes(symbols, ops);
                this.arg2.ToOpcodes(symbols, ops);

                switch (this.op)
                {
                    case Op.LessThan:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.LessThan));
                        break;
                    case Op.GreaterThan:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.GreaterThan));
                        break;
                    case Op.LessOrEqual:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.LessOrEqual));
                        break;
                    case Op.GreaterOrEqual:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.GreaterOrEqual));
                        break;
                    case Op.Equal:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Equal));
                        break;
                    case Op.Contains:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Contains));
                        break;
                    case Op.Prefix:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Prefix));
                        break;
                    case Op.Suffix:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Suffix));
                        break;
                    case Op.Regex:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Regex));
                        break;
                    case Op.Add:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Add));
                        break;
                    case Op.Sub:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Sub));
                        break;
                    case Op.Mul:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Mul));
                        break;
                    case Op.Div:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Div));
                        break;
                    case Op.And:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.And));
                        break;
                    case Op.Or:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Or));
                        break;
                    case Op.Intersection:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Intersection));
                        break;
                    case Op.Union:
                        ops.Add(new Datalog.Expressions.Op.Binary(Datalog.Expressions.Op.BinaryOp.Union));
                        break;
                }
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Binary binary = (Binary)o;

                if (op != binary.op) return false;
                if (!arg1.Equals(binary.arg1)) return false;
                return arg2.Equals(binary.arg2);
            }


            public override int GetHashCode()
            {
                int result = op.GetHashCode();
                result = 31 * result + arg1.GetHashCode();
                result = 31 * result + arg2.GetHashCode();
                return result;
            }


            public override string ToString()
            {
                return $"Binary{{op={op}, arg1={arg1}, arg2={arg2}}}";
            }
        }
    }
}
