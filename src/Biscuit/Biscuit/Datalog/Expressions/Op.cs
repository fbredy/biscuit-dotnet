using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Biscuit.Datalog.Expressions
{
    public abstract class Op
    {
        public abstract bool evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables);

        public abstract string print(Stack<string> stack, SymbolTable symbols);

        public abstract Format.Schema.Op serialize();

        static public Either<Errors.FormatError, Op> deserializeV1(Format.Schema.Op op)
        {
            if (op.Value != null)
            {
                return ID.deserialize_enumV1(op.Value).Select<Op>(v => new Op.Value(v));
            }
            else if (op.Unary != null)
            {
                return Op.Unary.deserializeV1(op.Unary);
            }
            else if (op.Binary != null)
            {
                return Op.Binary.deserializeV1(op.Binary);
            }
            else
            {
                return new Left(new Errors.DeserializationError("invalid unary operation"));
            }
        }

        public sealed class Value : Op
        {
            private ID value;

            public Value(ID value)
            {
                this.value = value;
            }

            public ID getValue()
            {
                return value;
            }

            public override bool evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                if (value is ID.Variable)
                {
                    ID.Variable var = (ID.Variable)value;
                    ID valueVar = variables[var.value];
                    if (valueVar != null)
                    {
                        stack.Push(valueVar);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    stack.Push(value);
                    return true;
                }
            }

            public override string print(Stack<string> stack, SymbolTable symbols)
            {
                string s = symbols.print_id(value);
                stack.Push(s);
                return s;
            }


            public override Format.Schema.Op serialize()
            {
                Format.Schema.Op b = new Format.Schema.Op();

                b.Value = this.value.serialize();

                return b;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is Value)) return false;

                Value value1 = (Value)obj;

                return value.Equals(value1.value);
            }


            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return "Value(" + value + ')';
            }
        }

        public enum UnaryOp
        {
            Negate,
            Parens,
            Length,
        }

        public sealed class Unary : Op
        {
            private UnaryOp op;

            public Unary(UnaryOp op)
            {
                this.op = op;
            }

            public UnaryOp getOp()
            {
                return op;
            }


            public override bool evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                ID value = stack.Pop();
                switch (this.op)
                {
                    case UnaryOp.Negate:
                        if (value is ID.Bool)
                        {
                            ID.Bool b = (ID.Bool)value;
                            stack.Push(new ID.Bool(!b.value));
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case UnaryOp.Parens:
                        stack.Push(value);
                        break;
                    case UnaryOp.Length:
                        if (value is ID.Str)
                        {
                            stack.Push(new ID.Integer(((ID.Str)value).value.Length));
                        }
                        else if (value is ID.Bytes)
                        {
                            stack.Push(new ID.Integer(((ID.Bytes)value).value.Length));
                        }
                        else if (value is ID.Set)
                        {
                            stack.Push(new ID.Integer(((ID.Set)value).value.Count));
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }


            public override string print(Stack<string> stack, SymbolTable symbols)
            {
                string prec = stack.Pop();
                string _s = "";
                switch (this.op)
                {
                    case UnaryOp.Negate:
                        _s = "! " + prec;
                        stack.Push(_s);
                        break;
                    case UnaryOp.Parens:
                        _s = "(" + prec + ")";
                        stack.Push(_s);
                        break;
                }
                return _s;
            }


            public override Format.Schema.Op serialize()
            {
                Format.Schema.Op b = new Format.Schema.Op();

                Format.Schema.OpUnary b1 = new Format.Schema.OpUnary();

                switch (this.op)
                {
                    case UnaryOp.Negate:
                        b1.Kind = Format.Schema.OpUnary.Types.Kind.Negate;
                        break;
                    case UnaryOp.Parens:
                        b1.Kind = Format.Schema.OpUnary.Types.Kind.Parens;
                        break;
                    case UnaryOp.Length:
                        b1.Kind = Format.Schema.OpUnary.Types.Kind.Length;
                        break;
                }

                b.Unary = b1;

                return b;
            }

            static public Either<Errors.FormatError, Op> deserializeV1(Format.Schema.OpUnary op)
            {
                switch (op.Kind)
                {
                    case Format.Schema.OpUnary.Types.Kind.Negate:
                        return new Right(new Op.Unary(UnaryOp.Negate));
                    case Format.Schema.OpUnary.Types.Kind.Parens:
                        return new Right(new Op.Unary(UnaryOp.Parens));
                    case Format.Schema.OpUnary.Types.Kind.Length:
                        return new Right(new Op.Unary(UnaryOp.Length));
                }

                return new Left(new Errors.DeserializationError("invalid unary operation"));
            }

            public override string ToString()
            {
                return "Unary." + op;
            }

            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || !(o is Unary)) return false;

                Unary unary = (Unary)o;

                return op == unary.op;
            }

            public override int GetHashCode()
            {
                return op.GetHashCode();
            }
        }

        public enum BinaryOp
        {
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
            Intersection,
            Union,
        }

        public sealed class Binary : Op
        {
            private BinaryOp op;

            public Binary(BinaryOp value)
            {
                this.op = value;
            }

            public BinaryOp getOp()
            {
                return op;
            }

            public override bool evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                ID right = stack.Pop();
                ID left = stack.Pop();

                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).value < ((ID.Integer)right).value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).value < ((ID.Date)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.GreaterThan:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).value > ((ID.Integer)right).value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).value > ((ID.Date)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.LessOrEqual:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).value <= ((ID.Integer)right).value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).value <= ((ID.Date)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.GreaterOrEqual:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).value >= ((ID.Integer)right).value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).value >= ((ID.Date)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Equal:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).value == ((ID.Integer)right).value));
                            return true;
                        }
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).value.Equals(((ID.Str)right).value)));
                            return true;
                        }
                        if (right is ID.Bytes && left is ID.Bytes)
                        {
                            stack.Push(new ID.Bool(Arrays.equals(((ID.Bytes)left).value, (((ID.Bytes)right).value))));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).value == ((ID.Date)right).value));
                            return true;
                        }
                        if (right is ID.Symbol && left is ID.Symbol)
                        {
                            stack.Push(new ID.Bool(((ID.Symbol)left).value == ((ID.Symbol)right).value));
                            return true;
                        }
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> leftSet = ((ID.Set)left).value;
                            HashSet<ID> rightSet = ((ID.Set)right).value;
                            stack.Push(new ID.Bool(leftSet.Count == rightSet.Count && leftSet.ContainsAll(rightSet)));
                            return true;
                        }
                        break;
                    case BinaryOp.Contains:
                        if (left is ID.Set &&
                                (right is ID.Integer || right is ID.Str || right is ID.Bytes || right is ID.Date
                                || right is ID.Bool || right is ID.Symbol))
                        {

                            stack.Push(new ID.Bool(((ID.Set)left).value.Contains(right)));
                            return true;
                        }
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> leftSet = ((ID.Set)left).value;
                            HashSet<ID> rightSet = ((ID.Set)right).value;
                            stack.Push(new ID.Bool(leftSet.ContainsAll(rightSet)));
                            return true;
                        }
                        break;
                    case BinaryOp.Prefix:
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).value.StartsWith(((ID.Str)right).value)));
                            return true;
                        }
                        break;
                    case BinaryOp.Suffix:
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).value.EndsWith(((ID.Str)right).value)));
                            return true;
                        }
                        break;
                    case BinaryOp.Regex:
                        if (right is ID.Str && left is ID.Str)
                        {
                            Regex regex = new Regex(((ID.Str)right).value);
                            var found = regex.IsMatch(((ID.Str)left).value);
                            stack.Push(new ID.Bool(found));
                            return true;
                        }
                        break;
                    case BinaryOp.Add:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).value + ((ID.Integer)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Sub:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).value - ((ID.Integer)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Mul:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).value * ((ID.Integer)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Div:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            long rl = ((ID.Integer)right).value;
                            if (rl != 0)
                            {
                                stack.Push(new ID.Integer(((ID.Integer)left).value / rl));
                                return true;
                            }
                        }
                        break;
                    case BinaryOp.And:
                        if (right is ID.Bool && left is ID.Bool)
                        {
                            stack.Push(new ID.Bool(((ID.Bool)left).value && ((ID.Bool)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Or:
                        if (right is ID.Bool && left is ID.Bool)
                        {
                            stack.Push(new ID.Bool(((ID.Bool)left).value || ((ID.Bool)right).value));
                            return true;
                        }
                        break;
                    case BinaryOp.Intersection:
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> intersec = new HashSet<ID>();
                            HashSet<ID> _right = ((ID.Set)right).value;
                            HashSet<ID> _left = ((ID.Set)left).value;
                            foreach (ID _id in _right)
                            {
                                if (_left.Contains(_id))
                                {
                                    intersec.Add(_id);
                                }
                            }
                            stack.Push(new ID.Set(intersec));
                            return true;
                        }
                        break;
                    case BinaryOp.Union:
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> union = new HashSet<ID>();
                            HashSet<ID> _right = ((ID.Set)right).value;
                            HashSet<ID> _left = ((ID.Set)left).value;
                            union.addAll(_right);
                            union.addAll(_left);
                            stack.Push(new ID.Set(union));
                            return true;
                        }
                        break;
                    default:
                        return false;
                }
                return false;
            }


            public override string print(Stack<string> stack, SymbolTable symbols)
            {
                string right = stack.Pop();
                string left = stack.Pop();
                string _s = "";
                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        _s = left + " < " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.GreaterThan:
                        _s = left + " > " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.LessOrEqual:
                        _s = left + " <= " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.GreaterOrEqual:
                        _s = left + " >= " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Equal:
                        _s = left + " == " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Contains:
                        _s = left + ".contains(" + right + ")";
                        stack.Push(_s);
                        break;
                    case BinaryOp.Prefix:
                        _s = left + ".starts_with(" + right + ")";
                        stack.Push(_s);
                        break;
                    case BinaryOp.Suffix:
                        _s = left + ".ends_with(" + right + ")";
                        stack.Push(_s);
                        break;
                    case BinaryOp.Regex:
                        _s = left + ".matches(" + right + ")";
                        stack.Push(_s);
                        break;
                    case BinaryOp.Add:
                        _s = left + " + " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Sub:
                        _s = left + " - " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Mul:
                        _s = left + " * " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Div:
                        _s = left + " / " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.And:
                        _s = left + " && " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Or:
                        _s = left + " || " + right;
                        stack.Push(_s);
                        break;
                    case BinaryOp.Intersection:
                        _s = left + ".intersection(" + right + ")";
                        stack.Push(_s);
                        break;
                    case BinaryOp.Union:
                        _s = left + ".union(" + right + ")";
                        stack.Push(_s);
                        break;
                }

                return _s;
            }

            public override Format.Schema.Op serialize()
            {
                Format.Schema.Op b = new Format.Schema.Op();

                Format.Schema.OpBinary b1 = new Format.Schema.OpBinary();

                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.LessThan;
                        break;
                    case BinaryOp.GreaterThan:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.GreaterThan;
                        break;
                    case BinaryOp.LessOrEqual:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.LessOrEqual;
                        break;
                    case BinaryOp.GreaterOrEqual:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.GreaterOrEqual;
                        break;
                    case BinaryOp.Equal:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Equal;
                        break;
                    case BinaryOp.Contains:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Contains;
                        break;
                    case BinaryOp.Prefix:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Prefix;
                        break;
                    case BinaryOp.Suffix:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Suffix;
                        break;
                    case BinaryOp.Regex:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Regex;
                        break;
                    case BinaryOp.Add:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Add;
                        break;
                    case BinaryOp.Sub:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Sub;
                        break;
                    case BinaryOp.Mul:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Mul;
                        break;
                    case BinaryOp.Div:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Div;
                        break;
                    case BinaryOp.And:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.And;
                        break;
                    case BinaryOp.Or:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Or;
                        break;
                    case BinaryOp.Intersection:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Intersection;
                        break;
                    case BinaryOp.Union:
                        b1.Kind=Format.Schema.OpBinary.Types.Kind.Union;
                        break;
                }

                b.Binary = b1;

                return b;
            }

            static public Either<Errors.FormatError, Op> deserializeV1(Format.Schema.OpBinary op)
            {
                switch (op.Kind)
                {
                    case Format.Schema.OpBinary.Types.Kind.LessThan:
                        return new Right(new Op.Binary(BinaryOp.LessThan));
                    case Format.Schema.OpBinary.Types.Kind.GreaterThan:
                        return new Right(new Op.Binary(BinaryOp.GreaterThan));
                    case Format.Schema.OpBinary.Types.Kind.LessOrEqual:
                        return new Right(new Op.Binary(BinaryOp.LessOrEqual));
                    case Format.Schema.OpBinary.Types.Kind.GreaterOrEqual:
                        return new Right(new Op.Binary(BinaryOp.GreaterOrEqual));
                    case Format.Schema.OpBinary.Types.Kind.Equal:
                        return new Right(new Op.Binary(BinaryOp.Equal));
                    case Format.Schema.OpBinary.Types.Kind.Contains:
                        return new Right(new Op.Binary(BinaryOp.Contains));
                    case Format.Schema.OpBinary.Types.Kind.Prefix:
                        return new Right(new Op.Binary(BinaryOp.Prefix));
                    case Format.Schema.OpBinary.Types.Kind.Suffix:
                        return new Right(new Op.Binary(BinaryOp.Suffix));
                    case Format.Schema.OpBinary.Types.Kind.Regex:
                        return new Right(new Op.Binary(BinaryOp.Regex));
                    case Format.Schema.OpBinary.Types.Kind.Add:
                        return new Right(new Op.Binary(BinaryOp.Add));
                    case Format.Schema.OpBinary.Types.Kind.Sub:
                        return new Right(new Op.Binary(BinaryOp.Sub));
                    case Format.Schema.OpBinary.Types.Kind.Mul:
                        return new Right(new Op.Binary(BinaryOp.Mul));
                    case Format.Schema.OpBinary.Types.Kind.Div:
                        return new Right(new Op.Binary(BinaryOp.Div));
                    case Format.Schema.OpBinary.Types.Kind.And:
                        return new Right(new Op.Binary(BinaryOp.And));
                    case Format.Schema.OpBinary.Types.Kind.Or:
                        return new Right(new Op.Binary(BinaryOp.Or));
                    case Format.Schema.OpBinary.Types.Kind.Intersection:
                        return new Right(new Op.Binary(BinaryOp.Intersection));
                    case Format.Schema.OpBinary.Types.Kind.Union:
                        return new Right(new Op.Binary(BinaryOp.Union));
                }

                return new Left(new Errors.DeserializationError("invalid binary operation"));
            }

            public override string ToString()
            {
                return "Binary." + op;
            }

            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || !(o is Binary)) return false;

                Binary binary = (Binary)o;

                return op == binary.op;
            }

            public override int GetHashCode()
            {
                return op.GetHashCode();
            }
        }
    }
}
