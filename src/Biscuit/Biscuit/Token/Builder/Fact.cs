using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class Fact
    {
        public Predicate predicate { get; }

        public Fact(string name, List<Term> ids)
        {
            this.predicate = new Predicate(name, ids);
        }

        public Fact(Predicate p)
        {
            this.predicate = p;
        }

        public Datalog.Fact convert(Datalog.SymbolTable symbols)
        {
            return new Datalog.Fact(this.predicate.convert(symbols));
        }

        public static Fact convert_from(Datalog.Fact f, Datalog.SymbolTable symbols)
        {
            return new Fact(Predicate.convert_from(f.predicate, symbols));
        }


        public override string ToString()
        {
            return "fact(" + predicate + ")";
        }

        public string name()
        {
            return this.predicate.name;
        }

        public List<Term> ids 
        { 
            get
            {
                return this.predicate.ids;
            } 
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Fact fact = (Fact)o;

            return predicate != null ? predicate.Equals(fact.predicate) : fact.predicate == null;
        }


        public override int GetHashCode()
        {
            return predicate != null ? predicate.GetHashCode() : 0;
        }
    }
}
