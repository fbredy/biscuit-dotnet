using Biscuit.Errors;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Biscuit.Datalog.Expressions
{
    public abstract class Op
    {
        public abstract bool Evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables);

        public abstract string Print(Stack<string> stack, SymbolTable symbols);

        public abstract Format.Schema.Op Serialize();

        static public Either<FormatError, Op> DeserializeV1(Format.Schema.Op op)
        {
            if (op.Value != null)
            {
                return ID.DeserializeEnumV1(op.Value).Select<Op>(v => new Op.Value(v));
            }
            else if (op.Unary != null)
            {
                return Op.Unary.DeserializeV1(op.Unary);
            }
            else if (op.Binary != null)
            {
                return Op.Binary.DeserializeV1(op.Binary);
            }
            else
            {
                return new DeserializationError("invalid unary operation");
            }
        }

        public sealed class Value : Op
        {
            private readonly ID value;

            public Value(ID value)
            {
                this.value = value;
            }

            public ID GetValue()
            {
                return value;
            }

            public override bool Evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                if (value is ID.Variable idVar)
                {
                    ID valueVar = variables[idVar.Value];
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

            public override string Print(Stack<string> stack, SymbolTable symbols)
            {
                string idPrinted = symbols.PrintId(value);
                stack.Push(idPrinted);
                return idPrinted;
            }


            public override Format.Schema.Op Serialize()
            {
                Format.Schema.Op op = new Format.Schema.Op
                {
                    Value = this.value.Serialize()
                };

                return op;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is Value)) return false;

                Value other = (Value)obj;

                return value.Equals(other.value);
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
            private readonly UnaryOp op;

            public Unary(UnaryOp op)
            {
                this.op = op;
            }

            public UnaryOp GetOp()
            {
                return op;
            }


            public override bool Evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                ID value = stack.Pop();
                switch (this.op)
                {
                    case UnaryOp.Negate:
                        if (value is ID.Bool idBool)
                        {
                            stack.Push(new ID.Bool(!idBool.Value));
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
                        if (value is ID.Str str)
                        {
                            stack.Push(new ID.Integer(str.Value.Length));
                        }
                        else if (value is ID.Bytes bytes)
                        {
                            stack.Push(new ID.Integer(bytes.Value.Length));
                        }
                        else if (value is ID.Set set)
                        {
                            stack.Push(new ID.Integer(set.Value.Count));
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
                return true;
            }


            public override string Print(Stack<string> stack, SymbolTable symbols)
            {
                string prec = stack.Pop();
                string result = string.Empty;
                switch (this.op)
                {
                    case UnaryOp.Negate:
                        result = "! " + prec;
                        stack.Push(result);
                        break;
                    case UnaryOp.Parens:
                        result = "(" + prec + ")";
                        stack.Push(result);
                        break;
                }
                return result;
            }


            public override Format.Schema.Op Serialize()
            {
                Format.Schema.Op result = new Format.Schema.Op();

                Format.Schema.OpUnary opUnary = new Format.Schema.OpUnary();

                switch (this.op)
                {
                    case UnaryOp.Negate:
                        opUnary.Kind = Format.Schema.OpUnary.Types.Kind.Negate;
                        break;
                    case UnaryOp.Parens:
                        opUnary.Kind = Format.Schema.OpUnary.Types.Kind.Parens;
                        break;
                    case UnaryOp.Length:
                        opUnary.Kind = Format.Schema.OpUnary.Types.Kind.Length;
                        break;
                }

                result.Unary = opUnary;

                return result;
            }

            static public Either<FormatError, Op> DeserializeV1(Format.Schema.OpUnary op)
            {
                return op.Kind switch
                {
                    Format.Schema.OpUnary.Types.Kind.Negate => new Op.Unary(UnaryOp.Negate),
                    Format.Schema.OpUnary.Types.Kind.Parens => new Op.Unary(UnaryOp.Parens),
                    Format.Schema.OpUnary.Types.Kind.Length => new Op.Unary(UnaryOp.Length),
                    _ => new DeserializationError("invalid unary operation"),
                };
            }

            public override string ToString()
            {
                return "Unary." + op;
            }

            public override bool Equals(object o)
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
            private readonly BinaryOp op;

            public Binary(BinaryOp value)
            {
                this.op = value;
            }

            public BinaryOp GetOp()
            {
                return op;
            }

            public override bool Evaluate(Stack<ID> stack, Dictionary<ulong, ID> variables)
            {
                ID right = stack.Pop();
                ID left = stack.Pop();

                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        if (right is ID.Integer rightInteger && left is ID.Integer leftInteger)
                        {
                            stack.Push(new ID.Bool(leftInteger.Value < rightInteger.Value));
                            return true;
                        }
                        if (right is ID.Date rightDate && left is ID.Date leftDate)
                        {
                            stack.Push(new ID.Bool(leftDate.Value < rightDate.Value));
                            return true;
                        }
                        break;
                    case BinaryOp.GreaterThan:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).Value > ((ID.Integer)right).Value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).Value > ((ID.Date)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.LessOrEqual:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).Value <= ((ID.Integer)right).Value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).Value <= ((ID.Date)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.GreaterOrEqual:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).Value >= ((ID.Integer)right).Value));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).Value >= ((ID.Date)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Equal:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Bool(((ID.Integer)left).Value == ((ID.Integer)right).Value));
                            return true;
                        }
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).Value.Equals(((ID.Str)right).Value)));
                            return true;
                        }
                        if (right is ID.Bytes && left is ID.Bytes)
                        {
                            stack.Push(new ID.Bool(Arrays.Equals(((ID.Bytes)left).Value, (((ID.Bytes)right).Value))));
                            return true;
                        }
                        if (right is ID.Date && left is ID.Date)
                        {
                            stack.Push(new ID.Bool(((ID.Date)left).Value == ((ID.Date)right).Value));
                            return true;
                        }
                        if (right is ID.Symbol && left is ID.Symbol)
                        {
                            stack.Push(new ID.Bool(((ID.Symbol)left).Value == ((ID.Symbol)right).Value));
                            return true;
                        }
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> leftSet = ((ID.Set)left).Value;
                            HashSet<ID> rightSet = ((ID.Set)right).Value;
                            stack.Push(new ID.Bool(leftSet.Count == rightSet.Count && leftSet.ContainsAll(rightSet)));
                            return true;
                        }
                        break;
                    case BinaryOp.Contains:
                        if (left is ID.Set &&
                                (right is ID.Integer || right is ID.Str || right is ID.Bytes || right is ID.Date
                                || right is ID.Bool || right is ID.Symbol))
                        {

                            stack.Push(new ID.Bool(((ID.Set)left).Value.Contains(right)));
                            return true;
                        }
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> leftSet = ((ID.Set)left).Value;
                            HashSet<ID> rightSet = ((ID.Set)right).Value;
                            stack.Push(new ID.Bool(leftSet.ContainsAll(rightSet)));
                            return true;
                        }
                        break;
                    case BinaryOp.Prefix:
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).Value.StartsWith(((ID.Str)right).Value)));
                            return true;
                        }
                        break;
                    case BinaryOp.Suffix:
                        if (right is ID.Str && left is ID.Str)
                        {
                            stack.Push(new ID.Bool(((ID.Str)left).Value.EndsWith(((ID.Str)right).Value)));
                            return true;
                        }
                        break;
                    case BinaryOp.Regex:
                        if (right is ID.Str && left is ID.Str)
                        {
                            Regex regex = new Regex(((ID.Str)right).Value);
                            var found = regex.IsMatch(((ID.Str)left).Value);
                            stack.Push(new ID.Bool(found));
                            return true;
                        }
                        break;
                    case BinaryOp.Add:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).Value + ((ID.Integer)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Sub:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).Value - ((ID.Integer)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Mul:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            stack.Push(new ID.Integer(((ID.Integer)left).Value * ((ID.Integer)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Div:
                        if (right is ID.Integer && left is ID.Integer)
                        {
                            long rl = ((ID.Integer)right).Value;
                            if (rl != 0)
                            {
                                stack.Push(new ID.Integer(((ID.Integer)left).Value / rl));
                                return true;
                            }
                        }
                        break;
                    case BinaryOp.And:
                        if (right is ID.Bool && left is ID.Bool)
                        {
                            stack.Push(new ID.Bool(((ID.Bool)left).Value && ((ID.Bool)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Or:
                        if (right is ID.Bool && left is ID.Bool)
                        {
                            stack.Push(new ID.Bool(((ID.Bool)left).Value || ((ID.Bool)right).Value));
                            return true;
                        }
                        break;
                    case BinaryOp.Intersection:
                        if (right is ID.Set && left is ID.Set)
                        {
                            HashSet<ID> intersec = new HashSet<ID>();
                            HashSet<ID> _right = ((ID.Set)right).Value;
                            HashSet<ID> _left = ((ID.Set)left).Value;
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
                            HashSet<ID> _right = ((ID.Set)right).Value;
                            HashSet<ID> _left = ((ID.Set)left).Value;
                            union.AddAll(_right);
                            union.AddAll(_left);
                            stack.Push(new ID.Set(union));
                            return true;
                        }
                        break;
                    default:
                        return false;
                }
                return false;
            }


            public override string Print(Stack<string> stack, SymbolTable symbols)
            {
                string right = stack.Pop();
                string left = stack.Pop();
                string result = string.Empty;
                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        result = left + " < " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.GreaterThan:
                        result = left + " > " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.LessOrEqual:
                        result = left + " <= " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.GreaterOrEqual:
                        result = left + " >= " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Equal:
                        result = left + " == " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Contains:
                        result = left + ".contains(" + right + ")";
                        stack.Push(result);
                        break;
                    case BinaryOp.Prefix:
                        result = left + ".starts_with(" + right + ")";
                        stack.Push(result);
                        break;
                    case BinaryOp.Suffix:
                        result = left + ".ends_with(" + right + ")";
                        stack.Push(result);
                        break;
                    case BinaryOp.Regex:
                        result = left + ".matches(" + right + ")";
                        stack.Push(result);
                        break;
                    case BinaryOp.Add:
                        result = left + " + " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Sub:
                        result = left + " - " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Mul:
                        result = left + " * " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Div:
                        result = left + " / " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.And:
                        result = left + " && " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Or:
                        result = left + " || " + right;
                        stack.Push(result);
                        break;
                    case BinaryOp.Intersection:
                        result = left + ".intersection(" + right + ")";
                        stack.Push(result);
                        break;
                    case BinaryOp.Union:
                        result = left + ".union(" + right + ")";
                        stack.Push(result);
                        break;
                }

                return result;
            }

            public override Format.Schema.Op Serialize()
            {
                Format.Schema.Op b = new Format.Schema.Op();

                Format.Schema.OpBinary b1 = new Format.Schema.OpBinary();

                switch (this.op)
                {
                    case BinaryOp.LessThan:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.LessThan;
                        break;
                    case BinaryOp.GreaterThan:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.GreaterThan;
                        break;
                    case BinaryOp.LessOrEqual:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.LessOrEqual;
                        break;
                    case BinaryOp.GreaterOrEqual:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.GreaterOrEqual;
                        break;
                    case BinaryOp.Equal:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Equal;
                        break;
                    case BinaryOp.Contains:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Contains;
                        break;
                    case BinaryOp.Prefix:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Prefix;
                        break;
                    case BinaryOp.Suffix:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Suffix;
                        break;
                    case BinaryOp.Regex:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Regex;
                        break;
                    case BinaryOp.Add:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Add;
                        break;
                    case BinaryOp.Sub:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Sub;
                        break;
                    case BinaryOp.Mul:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Mul;
                        break;
                    case BinaryOp.Div:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Div;
                        break;
                    case BinaryOp.And:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.And;
                        break;
                    case BinaryOp.Or:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Or;
                        break;
                    case BinaryOp.Intersection:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Intersection;
                        break;
                    case BinaryOp.Union:
                        b1.Kind = Format.Schema.OpBinary.Types.Kind.Union;
                        break;
                }

                b.Binary = b1;

                return b;
            }

            static public Either<Errors.FormatError, Op> DeserializeV1(Format.Schema.OpBinary op)
            {
                return op.Kind switch
                {
                    Format.Schema.OpBinary.Types.Kind.LessThan => new Op.Binary(BinaryOp.LessThan),
                    Format.Schema.OpBinary.Types.Kind.GreaterThan => new Op.Binary(BinaryOp.GreaterThan),
                    Format.Schema.OpBinary.Types.Kind.LessOrEqual => new Op.Binary(BinaryOp.LessOrEqual),
                    Format.Schema.OpBinary.Types.Kind.GreaterOrEqual => new Op.Binary(BinaryOp.GreaterOrEqual),
                    Format.Schema.OpBinary.Types.Kind.Equal => new Op.Binary(BinaryOp.Equal),
                    Format.Schema.OpBinary.Types.Kind.Contains => new Op.Binary(BinaryOp.Contains),
                    Format.Schema.OpBinary.Types.Kind.Prefix => new Op.Binary(BinaryOp.Prefix),
                    Format.Schema.OpBinary.Types.Kind.Suffix => new Op.Binary(BinaryOp.Suffix),
                    Format.Schema.OpBinary.Types.Kind.Regex => new Op.Binary(BinaryOp.Regex),
                    Format.Schema.OpBinary.Types.Kind.Add => new Op.Binary(BinaryOp.Add),
                    Format.Schema.OpBinary.Types.Kind.Sub => new Op.Binary(BinaryOp.Sub),
                    Format.Schema.OpBinary.Types.Kind.Mul => new Op.Binary(BinaryOp.Mul),
                    Format.Schema.OpBinary.Types.Kind.Div => new Op.Binary(BinaryOp.Div),
                    Format.Schema.OpBinary.Types.Kind.And => new Op.Binary(BinaryOp.And),
                    Format.Schema.OpBinary.Types.Kind.Or => new Op.Binary(BinaryOp.Or),
                    Format.Schema.OpBinary.Types.Kind.Intersection => new Op.Binary(BinaryOp.Intersection),
                    Format.Schema.OpBinary.Types.Kind.Union => new Op.Binary(BinaryOp.Union),
                    _ => new DeserializationError("invalid binary operation"),
                };
            }

            public override string ToString()
            {
                return "Binary." + op;
            }

            public override bool Equals(object o)
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
