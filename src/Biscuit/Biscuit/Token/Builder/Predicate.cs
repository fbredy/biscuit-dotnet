using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class Predicate
    {
        public string name { get; }
        public List<Term> ids { get; set; }

        public Predicate(string name, List<Term> ids)
        {
            this.name = name;
            this.ids = ids;
        }

        public Datalog.Predicate convert(Datalog.SymbolTable symbols)
        {
            ulong name = symbols.insert(this.name);
            List<Datalog.ID> ids = new List<Datalog.ID>();

            foreach (Term a in this.ids)
            {
                ids.Add(a.convert(symbols));
            }

            return new Datalog.Predicate(name, ids);
        }

        public static Predicate convert_from(Datalog.Predicate p, Datalog.SymbolTable symbols)
        {
            String name = symbols.print_symbol((int)p.name);
            List<Term> ids = new List<Term>();
            foreach (Datalog.ID i in p.ids)
            {
                ids.Add(i.toTerm(symbols));
            }

            return new Predicate(name, ids);
        }


        public override string ToString()
        {
            var idsInString = ids.Select((id)=>id.ToString());
            return "" + name + "(" + string.Join(", ", idsInString) + ")";
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Predicate predicate = (Predicate)o;

            if (name != null ? !name.Equals(predicate.name) : predicate.name != null) return false;
            return ids != null ? ids.SequenceEqual(predicate.ids) : predicate.ids == null;
        }

        public override int GetHashCode()
        {
            int result = name != null ? name.GetHashCode() : 0;
            result = 31 * result + (ids != null ? ids.GetHashCode() : 0);
            return result;
        }
    }
}
