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
        public abstract bool Match(ID other);

        public abstract Format.Schema.IDV1 Serialize();

        public abstract Term ToTerm(SymbolTable symbols);

        static public Either<Errors.FormatError, ID> DeserializeEnumV0(Format.Schema.IDV0 id)
        {
            if (id.Kind == Format.Schema.IDV0.Types.Kind.Date)
            {
                return Date.DeserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Integer)
            {
                return Integer.DeserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Str)
            {
                return Str.DeserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Bytes)
            {
                return Bytes.DeserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Symbol)
            {
                return Symbol.DeserializeV0(id);
            }
            else if (id.Kind == Format.Schema.IDV0.Types.Kind.Variable)
            {
                return Variable.DeserializeV0(id);
            }
            else
            {
                return new Errors.DeserializationError("invalid ID kind: " + id.Kind);
            }
        }

        static public Either<Errors.FormatError, ID> DeserializeEnumV1(Format.Schema.IDV1 id)
        {
            if (id.HasDate)
            {
                return Date.DeserializeV1(id);
            }
            else if (id.HasInteger)
            {
                return Integer.DeserializeV1(id);
            }
            else if (id.HasString)
            {
                return Str.DeserializeV1(id);
            }
            else if (id.HasBytes)
            {
                return Bytes.DeserializeV1(id);
            }
            else if (id.HasSymbol)
            {
                return Symbol.DeserializeV1(id);
            }
            else if (id.HasVariable)
            {
                return Variable.DeserializeV1(id);
            }
            else if (id.HasBool)
            {
                return Bool.DeserializeV1(id);
            }
            else if (id.Set != null)
            {
                return Set.DeserializeV1(id);
            }
            else
            {
                return new Errors.DeserializationError("invalid ID kind: id.Kind");
            }
        }

        [Serializable]
        public class Date : ID
        {
            public ulong Value { get; }

            public override bool Match(ID other)
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
                this.Value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Date date = (Date)o;

                return Value == date.Value;
            }


            public override int GetHashCode()
            {
                return (int)(Value ^ (Value >> 32));
            }


            public override string ToString()
            {
                return "@" + this.Value.ToString();
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { Date = this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Date)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected date");
                }
                else
                {
                    return new Date(id.Date);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasDate)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected date");
                }
                else
                {
                    return new Date(id.Date);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Date(this.Value);
            }
        }
        [Serializable]
        public class Integer : ID
        {
            public long Value { get; }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Integer)
                {
                    return this.Value == ((Integer)other).Value;
                }
                return false;
            }

            public Integer(long value)
            {
                this.Value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Integer integer = (Integer)o;

                return Value == integer.Value;
            }


            public override int GetHashCode()
            {
                return (int)(Value ^ (Value >> 32));
            }


            public override string ToString()
            {
                return string.Empty + this.Value.ToString();
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { Integer = this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Integer)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected integer");
                }
                else
                {
                    return new Integer(id.Integer);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasInteger)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected integer");
                }
                else
                {
                    return new Integer(id.Integer);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Integer(this.Value);
            }
        }

        [Serializable]
        public class Str : ID
        {
            public string Value { get; }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Str)
                {
                    return this.Value.Equals(((Str)other).Value);
                }
                return false;
            }

            public Str(string value)
            {
                this.Value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Str str = (Str)o;

                return Value != null ? Value.Equals(str.Value) : str.Value == null;
            }


            public override int GetHashCode()
            {
                return Value != null ? Value.GetHashCode() : 0;
            }


            public override string ToString()
            {
                return this.Value.ToString();
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { String = this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Str)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected string");
                }
                else
                {
                    return new Str(id.Str);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasString)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected string");
                }
                else
                {
                    return new Str(id.String);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Str(this.Value);
            }
        }

        [Serializable]
        public class Bytes : ID
        {
            public byte[] Value { get; }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Bytes)
                {
                    return this.Value.Equals(((Bytes)other).Value);
                }
                return false;
            }

            public Bytes(byte[] value)
            {
                this.Value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Bytes bytes = (Bytes)o;

                return Value.SequenceEqual(bytes.Value);
            }


            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }


            public override string ToString()
            {
                return this.Value.ToString();
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1()
                {
                    Bytes = ByteString.CopyFrom(this.Value)
                };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Str)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected byte array");
                }
                else
                {
                    return new Str(id.Str);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasBytes)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected byte array");
                }
                else
                {
                    return new Bytes(id.Bytes.ToByteArray());
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Bytes(this.Value);
            }
        }
        [Serializable]
        public class Symbol : ID
        {
            public ulong Value
            {
                get;
            }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Symbol)
                {
                    return this.Value == ((Symbol)other).Value;
                }
                return false;
            }

            public Symbol(ulong value)
            {
                this.Value = value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Symbol symbol = (Symbol)o;

                if (Value != symbol.Value) return false;

                return true;
            }

            public override int GetHashCode()
            {
                return (int)(Value ^ (Value >> 32));
            }

            public override string ToString()
            {
                return "#" + this.Value;
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { Symbol = this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Symbol)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected symbol");
                }
                else
                {
                    return new Symbol(id.Symbol);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasSymbol)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected symbol");
                }
                else
                {
                    return new Symbol(id.Symbol);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Symbol(symbols.PrintSymbol((int)this.Value));
            }
        }

        [Serializable]
        public class Variable : ID
        {
            public ulong Value
            {
                get;
            }

            public override bool Match(ID other)
            {
                return true;
            }

            public Variable(ulong value)
            {
                this.Value = value;
            }

            public Variable(long value)
            {
                this.Value = (ulong)value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Variable variable = (Variable)o;

                return Value == variable.Value;
            }

            public override int GetHashCode()
            {
                return (int)(Value ^ (Value >> 32));
            }

            public override string ToString()
            {
                return this.Value + "?";
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { Variable = (uint)this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV0(Format.Schema.IDV0 id)
            {
                if (id.Kind != Format.Schema.IDV0.Types.Kind.Variable)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected variable");
                }
                else
                {
                    return new Variable(id.Variable);
                }
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasVariable)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected variable");
                }
                else
                {
                    return new Variable(id.Variable);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Variable(symbols.PrintSymbol((int)this.Value));
            }
        }

        [Serializable]
        public class Bool : ID
        {
            public bool Value
            {
                get;
            }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Bool)
                {
                    return this.Value == ((Bool)other).Value;
                }
                return false;
            }

            public Bool(bool value)
            {
                this.Value = value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Bool boolean = (Bool)o;

                return Value == boolean.Value;
            }

            public override int GetHashCode()
            {
                return (Value ? 1 : 0);
            }


            public override string ToString()
            {
                return "" + this.Value;
            }

            public override Format.Schema.IDV1 Serialize()
            {
                return new Format.Schema.IDV1() { Bool = this.Value };
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (!id.HasBool)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected bool");
                }
                else
                {
                    return new Bool(id.Bool);
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                return new Term.Bool(this.Value);
            }
        }

        [Serializable]
        public class Set : ID
        {
            public HashSet<ID> Value
            {
                get;
            }

            public override bool Match(ID other)
            {
                if (other is Variable)
                {
                    return true;
                }
                if (other is Set set)
                {
                    return this.Value.Equals(set.Value);
                }
                return false;
            }

            public Set(HashSet<ID> value)
            {
                this.Value = value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Set set = (Set)o;

                return Value.Equals(set.Value);
            }


            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }


            public override string ToString()
            {
                return "" + Value;
            }

            public override Format.Schema.IDV1 Serialize()
            {
                Format.Schema.IDSet s = new Format.Schema.IDSet();
                s.Set.AddRange(this.Value.Select(l => l.Serialize()));

                return new Format.Schema.IDV1() { Set = s };
            }

            static public Either<Errors.FormatError, ID> DeserializeV1(Format.Schema.IDV1 id)
            {
                if (id.Set == null)
                {
                    return new Errors.DeserializationError("invalid ID kind, expected set");
                }
                else
                {
                    HashSet<ID> values = new HashSet<ID>();
                    Format.Schema.IDSet s = id.Set;

                    foreach (Format.Schema.IDV1 l in s.Set)
                    {
                        Either<Errors.FormatError, ID> res = ID.DeserializeEnumV1(l);
                        if (res.IsLeft)
                        {
                            return res.Left;
                        }
                        else
                        {
                            ID value = res.Right;

                            if (value is Variable)
                            {
                                return new Errors.DeserializationError("sets cannot contain variables");
                            }

                            values.Add(value);
                        }
                    }

                    if (values.Count == 0)
                    {
                        return new Errors.DeserializationError("invalid Set value");
                    }
                    else
                    {
                        return new Set(values);
                    }
                }
            }

            public override Term ToTerm(SymbolTable symbols)
            {
                HashSet<Term> set = new HashSet<Term>();

                foreach (ID i in this.Value)
                {
                    set.Add(i.ToTerm(symbols));
                }

                return new Term.Set(set);
            }
        }
    }
}