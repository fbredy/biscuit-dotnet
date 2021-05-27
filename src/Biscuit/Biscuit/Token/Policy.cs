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
        private List<Rule> queries;
        public Kind kind;

        public Policy(List<Rule> queries, Kind kind)
        {
            this.queries = queries;
            this.kind = kind;
        }

        public Policy(Rule query, Kind kind)
        {
            List<Rule> r = new List<Rule>();
            r.Add(query);

            this.queries = r;
            this.kind = kind;
        }

        public Datalog.Check convert(Datalog.SymbolTable symbols)
        {
            List<Datalog.Rule> queries = new List<Datalog.Rule>();

            foreach (Rule q in this.queries)
            {
                queries.Add(q.convert(symbols));
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
