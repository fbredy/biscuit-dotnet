using Biscuit.Datalog;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public abstract class Term
    {
        public abstract ID convert(SymbolTable symbols);

        static public Term convert_from(ID id, SymbolTable symbols)
        {
            return id.toTerm(symbols);
        }

        public class Symbol : Term
        {
            string value;

            public Symbol(string value)
            {
                this.value = value;
            }


            public override ID convert(SymbolTable symbols)
            {
                return new ID.Symbol(symbols.insert(this.value));
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
            string value;

            public Variable(string value)
            {
                this.value = value;
            }

            public override ID convert(SymbolTable symbols)
            {
                return new ID.Variable(symbols.insert(this.value));
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
            long value;

            public Integer(long value)
            {
                this.value = value;
            }

            public override ID convert(SymbolTable symbols)
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
            public string value { get; }

            public Str(string value)
            {
                this.value = value;
            }

            public override ID convert(SymbolTable symbols)
            {
                return new ID.Str(this.value);
            }

            public override string ToString()
            {
                return "\"" + value + "\"";
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
        }

        public class Bytes : Term
        {
            byte[] value;

            public Bytes(byte[] value)
            {
                this.value = value;
            }


            public override ID convert(SymbolTable symbols)
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
            ulong value;

            /// <summary>
            /// constructor, takes seconds since January 1, 1970, 00:00:00 GTM
            /// </summary>
            /// <param name="value">seconds</param>
            public Date(ulong value)
            {
                this.value = value;
            }


            public override ID convert(SymbolTable symbols)
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
            bool value;

            public Bool(bool value)
            {
                this.value = value;
            }


            public override ID convert(SymbolTable symbols)
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
            HashSet<Term> value;

            public Set(HashSet<Term> value)
            {
                this.value = value;
            }


            public override ID convert(SymbolTable symbols)
            {
                HashSet<ID> s = new HashSet<ID>();

                foreach (Term t in this.value)
                {
                    s.Add(t.convert(symbols));
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
