using Biscuit.Datalog;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class Check
    {
        List<Rule> queries;

        public Check(List<Rule> queries)
        {
            this.queries = queries;
        }
        public Check(Rule query)
        {
            List<Rule> r = new List<Rule>();
            r.Add(query);
            queries = r;
        }


        public Datalog.Check convert(SymbolTable symbols)
        {
            List<Datalog.Rule> queries = new List<Datalog.Rule>();

            foreach (Rule q in this.queries)
            {
                queries.Add(q.convert(symbols));
            }
            return new Datalog.Check(queries);
        }

        public static Check convert_from(Datalog.Check r, SymbolTable symbols)
        {
            List<Rule> queries = new List<Rule>();

            foreach (Datalog.Rule q in r.queries)
            {
                queries.Add(Rule.convert_from(q, symbols));
            }

            return new Check(queries);
        }

        public override string ToString()
        {
            IEnumerable<string> qs = 
                queries.Select((q)=> {
                IEnumerable<string> b = q.body.Select((pred)=>pred.ToString());
                string res = string.Join(", ", b);

                if (!q.expressions.isEmpty())
                {
                    IEnumerable<string> e = q.expressions.Select((expression)=>expression.ToString());
                    res += ", " + string.Join(", ", e);
                }

                return res;
            });

            return "check if " + string.Join(" or ", qs);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Check check = (Check)o;

            return queries != null ? queries.SequenceEqual(check.queries) : check.queries == null;
        }

        public override int GetHashCode()
        {
            return queries != null ? queries.GetHashCode() : 0;
        }
    }
}
