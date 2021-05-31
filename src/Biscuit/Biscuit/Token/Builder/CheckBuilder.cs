using Biscuit.Datalog;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class CheckBuilder
    {
        readonly List<RuleBuilder> queries;

        public CheckBuilder(List<RuleBuilder> queries)
        {
            this.queries = queries;
        }
        public CheckBuilder(RuleBuilder query)
        {
            List<RuleBuilder> r = new List<RuleBuilder> { query };
            queries = r;
        }


        public Datalog.Check convert(SymbolTable symbols)
        {
            List<Datalog.Rule> queries = new List<Datalog.Rule>();

            foreach (RuleBuilder q in this.queries)
            {
                queries.Add(q.Convert(symbols));
            }
            return new Datalog.Check(queries);
        }

        public static CheckBuilder convert_from(Datalog.Check r, SymbolTable symbols)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();

            foreach (Datalog.Rule q in r.Queries)
            {
                queries.Add(RuleBuilder.ConvertFrom(q, symbols));
            }

            return new CheckBuilder(queries);
        }

        public override string ToString()
        {
            IEnumerable<string> qs = 
                queries.Select((q)=> {
                IEnumerable<string> b = q.Body.Select((pred)=>pred.ToString());
                string res = string.Join(", ", b);

                if (!q.Expressions.IsEmpty())
                {
                    IEnumerable<string> e = q.Expressions.Select((expression)=>expression.ToString());
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

            CheckBuilder check = (CheckBuilder)o;

            return queries != null ? queries.SequenceEqual(check.queries) : check.queries == null;
        }

        public override int GetHashCode()
        {
            return queries != null ? queries.GetHashCode() : 0;
        }
    }
}
