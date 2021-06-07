using Biscuit.Token.Builder;
using System.Collections.Generic;

namespace Biscuit.Token
{
    public class Policy
    {
        public enum Kind
        {
            Allow,
            Deny,
        }

        private readonly List<RuleBuilder> queries;
        public Kind kind;

        public Policy(List<RuleBuilder> queries, Kind kind)
        {
            this.queries = queries;
            this.kind = kind;
        }

        public Policy(RuleBuilder query, Kind kind)
        {
            this.queries = new List<RuleBuilder>
            {
                query
            };

            this.kind = kind;
        }

        public Datalog.Check Convert(Datalog.SymbolTable symbols)
        {
            List<Datalog.Rule> queries = new List<Datalog.Rule>();

            foreach (RuleBuilder q in this.queries)
            {
                queries.Add(q.Convert(symbols));
            }
            return new Datalog.Check(queries);
        }

        public override string ToString()
        {
            return this.kind switch
            {
                Kind.Allow => "allow if " + queries,
                Kind.Deny => "deny if " + queries,
                _ => null,
            };
        }

    }
}
