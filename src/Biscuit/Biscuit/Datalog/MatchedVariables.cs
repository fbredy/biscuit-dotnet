using Biscuit.Datalog.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class MatchedVariables
    {
        private Dictionary<ulong, Optional<ID>> variables;

        public bool insert(ulong key, ID value)
        {
            if (this.variables.ContainsKey(key))
            {
                Optional<ID> val = this.variables[key];
                if (val.isPresent())
                {
                    return val.get().Equals(value);
                }
                else
                {
                    this.variables[key] = Optional<ID>.of(value);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool is_complete()
        {
            return this.variables.Values.All(v => v.isPresent());
        }

        public Optional<Dictionary<ulong, ID>> complete()
        {
            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            foreach (var entry in this.variables)
            {
                if (entry.Value.isPresent())
                {
                    variables.Add(entry.Key, entry.Value.get());
                }
                else
                {
                    return Optional<Dictionary<ulong, ID>>.empty();
                }
            }
            return Optional<Dictionary<ulong, ID>>.of(variables);
        }

        public MatchedVariables clone()
        {
            MatchedVariables other = new MatchedVariables(this.variables.Keys.ToArray());
            foreach (var entry in this.variables)
            {
                if (entry.Value.isPresent())
                {
                    other.variables[entry.Key] = entry.Value;
                }
            }
            return other;
        }

        public MatchedVariables(IEnumerable<ulong> ids)
        {
            this.variables = new Dictionary<ulong, Optional<ID>>();
            foreach (ulong id in ids)
            {
                this.variables.Add(id, Optional<ID>.empty());
            }
        }

        public Option<Dictionary<ulong, ID>> check_expressions(List<Expression> expressions)
        {
            Optional<Dictionary<ulong, ID>> vars = this.complete();
            if (vars.isPresent())
            {
                Dictionary<ulong, ID> variables = vars.get();

                foreach (Expression e in expressions)
                {
                    Option<ID> res = e.evaluate(variables);

                    if (res.isEmpty())
                    {
                        return Option<Dictionary<ulong, ID>>.none();
                    }

                    if (!res.get().Equals(new ID.Bool(true)))
                    {
                        return Option<Dictionary<ulong, ID>>.none();
                    }
                }

                return Option<Dictionary<ulong, ID>>.some(variables);
            }
            else
            {
                return Option<Dictionary<ulong, ID>>.none();
            }
        }
    }
}