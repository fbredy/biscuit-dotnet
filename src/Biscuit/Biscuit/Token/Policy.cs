using Biscuit.Token.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biscuit.Token
{
    public class Policy
    {
        public enum Kind
        {
            Allow,
            Deny,
        }

        //TODO Rule  ou Rule
        private List<RuleBuilder> queries;
        public Kind kind;

        public Policy(List<RuleBuilder> queries, Kind kind)
        {
            this.queries = queries;
            this.kind = kind;
        }

        public Policy(RuleBuilder query, Kind kind)
        {
            List<RuleBuilder> r = new List<RuleBuilder>();
            r.Add(query);

            this.queries = r;
            this.kind = kind;
        }

        public Datalog.Check convert(Datalog.SymbolTable symbols)
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
            switch (this.kind)
            {
                case Kind.Allow:
                    return "allow if " + queries;
                case Kind.Deny:
                    return "deny if " + queries;
            }
            return null;
        }

    }
}
