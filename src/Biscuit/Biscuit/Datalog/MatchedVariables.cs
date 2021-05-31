using Biscuit.Datalog.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class MatchedVariables
    {
        private readonly Dictionary<ulong, Optional<ID>> variables;

        public bool Insert(ulong key, ID value)
        {
            if (this.variables.ContainsKey(key))
            {
                Optional<ID> val = this.variables[key];
                if (val.IsPresent())
                {
                    return val.Get().Equals(value);
                }
                else
                {
                    this.variables[key] = Optional<ID>.Of(value);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsComplete()
        {
            return this.variables.Values.All(v => v.IsPresent());
        }

        public Optional<Dictionary<ulong, ID>> Complete()
        {
            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            foreach (var entry in this.variables)
            {
                if (entry.Value.IsPresent())
                {
                    variables.Add(entry.Key, entry.Value.Get());
                }
                else
                {
                    return Optional<Dictionary<ulong, ID>>.Empty();
                }
            }
            return Optional<Dictionary<ulong, ID>>.Of(variables);
        }

        public MatchedVariables Clone()
        {
            MatchedVariables other = new MatchedVariables(this.variables.Keys.ToArray());
            foreach (var entry in this.variables)
            {
                if (entry.Value.IsPresent())
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
                this.variables.Add(id, Optional<ID>.Empty());
            }
        }

        public Option<Dictionary<ulong, ID>> CheckExpressions(IList<Expression> expressions)
        {
            Optional<Dictionary<ulong, ID>> vars = this.Complete();
            if (vars.IsPresent())
            {
                Dictionary<ulong, ID> variables = vars.Get();

                foreach (Expression expression in expressions)
                {
                    Option<ID> res = expression.Evaluate(variables);

                    if (res.IsEmpty())
                    {
                        return Option<Dictionary<ulong, ID>>.None();
                    }

                    if (!res.Get().Equals(new ID.Bool(true)))
                    {
                        return Option<Dictionary<ulong, ID>>.None();
                    }
                }

                return Option<Dictionary<ulong, ID>>.Some(variables);
            }
            else
            {
                return Option<Dictionary<ulong, ID>>.None();
            }
        }
    }
}