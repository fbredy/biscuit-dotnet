using Biscuit.Datalog;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public abstract class Term
    {
        public abstract ID Convert(SymbolTable symbols);

        static public Term ConvertFrom(ID id, SymbolTable symbols)
        {
            return id.ToTerm(symbols);
        }

        public class Symbol : Term
        {
            private readonly string value;

            public Symbol(string value)
            {
                this.value = value;
            }


            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Symbol(symbols.Insert(this.value));
            }


            public override string ToString()
            {
                return "#" + value;
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is Symbol)) return false;
                Symbol symbol = (Symbol)o;
                return value == symbol.value;
            }


            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }

        public class Variable : Term
        {
            private readonly string value;

            public Variable(string value)
            {
                this.value = value;
            }

            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Variable(symbols.Insert(this.value));
            }


            public override string ToString()
            {
                return "$" + value;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is Variable)) return false;

                Variable variable = (Variable)o;

                return value.Equals(variable.value);
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }

        public class Integer : Term
        {
            private readonly long value;

            public Integer(long value)
            {
                this.value = value;
            }

            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Integer(this.value);
            }

            public override string ToString()
            {
                return value.ToString();
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
        }

        public class Str : Term
        {
            public string Value { get; }

            public Str(string value)
            {
                this.Value = value;
            }

            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Str(this.Value);
            }

            public override string ToString()
            {
                return "\"" + Value + "\"";
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
        }

        public class Bytes : Term
        {
            private readonly byte[] value;

            public Bytes(byte[] value)
            {
                this.value = value;
            }


            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Bytes(this.value);
            }


            public override string ToString()
            {
                return "\"" + value + "\"";
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
        }

        public class Date : Term
        {
            private readonly ulong value;

            /// <summary>
            /// constructor, takes seconds since January 1, 1970, 00:00:00 GTM
            /// </summary>
            /// <param name="value">seconds</param>
            public Date(ulong value)
            {
                this.value = value;
            }


            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Date(this.value);
            }


            public override string ToString()
            {
                return "" + value;
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
        }

        public class Bool : Term
        {
            private readonly bool value;

            public Bool(bool value)
            {
                this.value = value;
            }


            public override ID Convert(SymbolTable symbols)
            {
                return new ID.Bool(this.value);
            }


            public override string ToString()
            {
                return value.ToString();
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
        }

        public class Set : Term
        {
            private readonly HashSet<Term> value;

            public Set(HashSet<Term> value)
            {
                this.value = value;
            }


            public override ID Convert(SymbolTable symbols)
            {
                HashSet<ID> s = new HashSet<ID>();

                foreach (Term t in this.value)
                {
                    s.Add(t.Convert(symbols));
                }

                return new ID.Set(s);
            }


            public override string ToString()
            {
                return "[" + value + ']';
            }


            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Set set = (Set)o;

                return value != null ? value.SequenceEqual(set.value) : set.value == null;
            }


            public override int GetHashCode()
            {
                return value != null ? value.GetHashCode() : 0;
            }
        }
    }
}
