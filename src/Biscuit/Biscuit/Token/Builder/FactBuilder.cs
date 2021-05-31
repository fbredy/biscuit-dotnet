using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class FactBuilder
    {
        public PredicateBuilder Predicate { get; }

        public FactBuilder(string name, List<Term> ids)
        {
            this.Predicate = new PredicateBuilder(name, ids);
        }

        public FactBuilder(PredicateBuilder p)
        {
            this.Predicate = p;
        }

        public Datalog.Fact Convert(Datalog.SymbolTable symbols)
        {
            return new Datalog.Fact(this.Predicate.Convert(symbols));
        }

        public static FactBuilder ConvertFrom(Datalog.Fact f, Datalog.SymbolTable symbols)
        {
            return new FactBuilder(PredicateBuilder.ConvertFrom(f.predicate, symbols));
        }


        public override string ToString()
        {
            return "fact(" + Predicate + ")";
        }

        public string Name
        {
            get
            {
                return this.Predicate.Name;
            }
        }

        public List<Term> Ids 
        { 
            get
            {
                return this.Predicate.Ids;
            } 
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            FactBuilder fact = (FactBuilder)o;

            return Predicate != null ? Predicate.Equals(fact.Predicate) : fact.Predicate == null;
        }


        public override int GetHashCode()
        {
            return Predicate != null ? Predicate.GetHashCode() : 0;
        }
    }
}
