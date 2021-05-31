using Biscuit.Datalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class PredicateBuilder
    {
        public string Name { get; }
        public List<Term> Ids { get; set; }

        public PredicateBuilder(string name, List<Term> ids)
        {
            this.Name = name;
            this.Ids = ids;
        }

        public Predicate Convert(SymbolTable symbols)
        {
            ulong name = symbols.Insert(this.Name);
            List<Datalog.ID> ids = new List<Datalog.ID>();

            foreach (Term term in this.Ids)
            {
                ids.Add(term.convert(symbols));
            }

            return new Predicate(name, ids);
        }

        public static PredicateBuilder ConvertFrom(Predicate p, SymbolTable symbols)
        {
            String name = symbols.PrintSymbol((int)p.Name);
            List<Term> ids = new List<Term>();
            foreach (Datalog.ID i in p.Ids)
            {
                ids.Add(i.ToTerm(symbols));
            }

            return new PredicateBuilder(name, ids);
        }


        public override string ToString()
        {
            var idsInString = Ids.Select((id)=>id.ToString());
            return "" + Name + "(" + string.Join(", ", idsInString) + ")";
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            PredicateBuilder predicate = (PredicateBuilder)o;

            if (Name != null ? !Name.Equals(predicate.Name) : predicate.Name != null) return false;
            return Ids != null ? Ids.SequenceEqual(predicate.Ids) : predicate.Ids == null;
        }

        public override int GetHashCode()
        {
            int result = Name != null ? Name.GetHashCode() : 0;
            result = 31 * result + (Ids != null ? Ids.GetHashCode() : 0);
            return result;
        }
    }
}
