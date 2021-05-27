using Biscuit.Token.Builder;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public abstract class ID
    {
        public abstract bool match(ID other);
        public abstract Format.Schema.IDV1 serialize();

        public abstract Term toTerm(SymbolTable symbols);

        static public Either<Errors.FormatError, ID> deserialize_enumV0(Format.Schema.IDV0 id)
        {
            if (id.Kind == Format.Schema.IDV0.Types.Kind.Date)
            {
                return Date.deserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Integer)
            {
                return Integer.deserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Str)
            {
                return Str.deserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Bytes)
            {
                return Bytes.deserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Symbol)
            {
                return Symbol.deserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Variable)
            {
                return Variable.deserializeV0(id);
            }
            else
            {
                return new Left(new Errors.DeserializationError("invalid ID kind: " + id.Kind));
            }
        }

        static public Either<Errors.FormatError, ID> deserialize_enumV1(Format.Schema.IDV1 id)
        {
            if (id.HasDate)
            {
                return Date.deserializeV1(id);
            }
            else if (id.HasInteger)
            {
                return Integer.deserializeV1(id);
            }
            else if (id.HasString)
            {
                return Str.deserializeV1(id);
            }
            else if (id.HasBytes)
            {
                return Bytes.deserializeV1(id);
            }
            else if (id.HasSymbol)
            {
                return Symbol.deserializeV1(id);
            }
            else if (id.HasVariable)
            {
                return Variable.deserializeV1(id);
            }
            else if (id.HasBool)
            {
                return Bool.deserializeV1(id);
            }
            else if (id.Set != null)
            {
                return Set.deserializeV1(id);
            }
            else
            {
                return new Left(new Errors.DeserializationError("invalid ID kind: id.Kind"));
            }
        }

        [Serializable]
        public class Date : ID
        {
            public ulong value { get; }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                else
                {
                    return this.Equals(other);
                }
            }

            public Date(ulong value)
            {
                this.value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Date date = (Date)o;

                return value == date.value;
            }


            public override int GetHashCode()
            {
                return (int)(value ^ (value >> 32));
            }


            public override string ToString()
            {
                return "@" + this.value.ToString();
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { Date = this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Date)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected date"));
                }
                else
                {
                    return new Right(new Date(id.Date));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasDate)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected date"));
                }
                else
                {
                    return new Right(new Date(id.Date));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Date(this.value);
            }
        }
        [Serializable]
        public class Integer : ID
        {
            public long value { get; }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Integer)
                {
                    return this.value == ((Integer)other).value;
                }
                return false;
            }

            public Integer(long value)
            {
                this.value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Integer integer = (Integer)o;

                return value == integer.value;
            }


            public override int GetHashCode()
            {
                return (int)(value ^ (value >> 32));
            }


            public override string ToString()
            {
                return string.Empty + this.value.ToString();
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { Integer = this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Integer)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected integer"));
                }
                else
                {
                    return new Right(new Integer(id.Integer));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasInteger)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected integer"));
                }
                else
                {
                    return new Right(new Integer(id.Integer));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Integer(this.value);
            }
        }
        [Serializable]
        public class Str : ID
        {
            public string value { get; }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Str)
                {
                    return this.value.Equals(((Str)other).value);
                }
                return false;
            }

            public Str(string value)
            {
                this.value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Str str = (Str)o;

                return value != null ? value.Equals(str.value) : str.value == null;
            }


            public override int GetHashCode()
            {
                return value != null ? value.GetHashCode() : 0;
            }


            public override string ToString()
            {
                return this.value.ToString();
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { String = this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Str)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected string"));
                }
                else
                {
                    return new Right(new Str(id.Str));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasString)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected string"));
                }
                else
                {
                    return new Right(new Str(id.String));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Str(this.value);
            }
        }

        [Serializable]
        public class Bytes : ID
        {
            public byte[] value { get; }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Bytes)
                {
                    return this.value.Equals(((Bytes)other).value);
                }
                return false;
            }

            public Bytes(byte[] value)
            {
                this.value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Bytes bytes = (Bytes)o;

                return value.SequenceEqual(bytes.value);
            }


            public override int GetHashCode()
            {
                return value.GetHashCode();
            }


            public override string ToString()
            {
                return this.value.ToString();
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1()
                {
                    Bytes = ByteString.CopyFrom(this.value)
                };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Str)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected byte array"));
                }
                else
                {
                    return new Right(new Str(id.Str));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasBytes)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected byte array"));
                }
                else
                {
                    return new Right(new Bytes(id.Bytes.ToByteArray()));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Bytes(this.value);
            }
        }
        [Serializable]
        public class Symbol : ID
        {
            public ulong value
            {
                get;
            }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Symbol)
                {
                    return this.value == ((Symbol)other).value;
                }
                return false;
            }

            public Symbol(ulong value)
            {
                this.value = value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Symbol symbol = (Symbol)o;

                if (value != symbol.value) return false;

                return true;
            }

            public override int GetHashCode()
            {
                return (int)(value ^ (value >> 32));
            }

            public override string ToString()
            {
                return "#" + this.value;
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { Symbol = this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Symbol)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected symbol"));
                }
                else
                {
                    return new Right(new Symbol(id.Symbol));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasSymbol)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected symbol"));
                }
                else
                {
                    return new Right(new Symbol(id.Symbol));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Symbol(symbols.print_symbol((int)this.value));
            }
        }
        [Serializable]
        public class Variable : ID
        {
            public ulong value
            {
                get;
            }

            public override bool match(ID other)
            {
                return true;
            }

            public Variable(ulong value)
            {
                this.value = value;
            }

            public Variable(long value)
            {
                this.value = (ulong)value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Variable variable = (Variable)o;

                return value == variable.value;
            }

            public override int GetHashCode()
            {
                return (int)(value ^ (value >> 32));
            }

            public override string ToString()
            {
                return this.value + "?";
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { Variable = (uint)this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Variable)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected variable"));
                }
                else
                {
                    return new Right(new Variable(id.Variable));
                }
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasVariable)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected variable"));
                }
                else
                {
                    return new Right(new Variable(id.Variable));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Variable(symbols.print_symbol((int)this.value));
            }
        }
        [Serializable]
        public class Bool : ID
        {
            public bool value
            {
                get;
            }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Bool)
                {
                    return this.value == ((Bool)other).value;
                }
                return false;
            }

            public Bool(bool value)
            {
                this.value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Bool boolean = (Bool)o;

                return value == boolean.value;
            }

            public override int GetHashCode()
            {
                return (value ? 1 : 0);
            }


            public override string ToString()
            {
                return "" + this.value;
            }

            public override Format.Schema.IDV1 serialize()
            {
                return new Format.Schema.IDV1() { Bool = this.value };
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasBool)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected bool"));
                }
                else
                {
                    return new Right(new Bool(id.Bool));
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                return new Term.Bool(this.value);
            }
        }

        [Serializable]
        public class Set : ID
        {
            public HashSet<ID> value
            {
                get;
            }

            public override bool match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Set)
                {
                    return this.value.Equals(((Set)other).value);
                }
                return false;
            }

            public Set(HashSet<ID> value)
            {
                this.value = value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Set set = (Set)o;

                return value.Equals(set.value);
            }


            public override int GetHashCode()
            {
                return value.GetHashCode();
            }


            public override string ToString()
            {
                return "" +
                        value;
            }

            public override Format.Schema.IDV1 serialize()
            {
                Format.Schema.IDSet s = new Format.Schema.IDSet();
                s.Set.AddRange(this.value.Select(l => l.serialize()));

                return new Format.Schema.IDV1() { Set = s };
            }

            static public Either<Errors.FormatError, ID> deserializeV1(Format.Schema.IDV1 id)
            {
                if (id.Set == null)
                {
                    return new Left(new Errors.DeserializationError("invalid ID kind, expected set"));
                }
                else
                {
                    HashSet<ID> values = new HashSet<ID>();
                    Format.Schema.IDSet s = id.Set;

                    foreach (Format.Schema.IDV1 l in s.Set)
                    {
                        Either<Errors.FormatError, ID> res = ID.deserialize_enumV1(l);
                        if (res.IsLeft)
                        {
                            Errors.FormatError e = res.Left;
                            return new Left(e);
                        }
                        else
                        {
                            ID value = res.Right;

                            if (value is Variable)
                            {
                                return new Left(new Errors.DeserializationError("sets cannot contain variables"));
                            }

                            values.Add(value);
                        }
                    }

                    if (values.Count == 0)
                    {
                        return new Left(new Errors.DeserializationError("invalid Set value"));
                    }
                    else
                    {
                        return new Right(new Set(values));
                    }
                }
            }

            public override Term toTerm(SymbolTable symbols)
            {
                HashSet<Term> s = new HashSet<Term>();

                foreach (ID i in this.value)
                {
                    s.Add(i.toTerm(symbols));
                }

                return new Term.Set(s);
            }
        }
    }
}