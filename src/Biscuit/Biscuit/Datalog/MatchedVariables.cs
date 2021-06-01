using Biscuit.Datalog.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class MatchedVariables
    {
        private readonly Dictionary<ulong, Option<ID>> variables;

        public bool Insert(ulong key, ID value)
        {
            if (this.variables.ContainsKey(key))
            {
                Option<ID> val = this.variables[key];
                if (val.IsDefined)
                {
                    return val.Get().Equals(value);
                }
                else
                {
                    this.variables[key] = Option<ID>.Some(value);
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
            return this.variables.Values.All(v => v.IsDefined);
        }

        public Option<Dictionary<ulong, ID>> Complete()
        {
            Dictionary<ulong, ID> variables = new Dictionary<ulong, ID>();
            foreach (var entry in this.variables)
            {
                if (entry.Value.IsDefined)
                {
                    variables.Add(entry.Key, entry.Value.Get());
                }
                else
                {
                    return Option<Dictionary<ulong, ID>>.None();
                }
            }
            return Option.Some(variables);
        }

        public MatchedVariables Clone()
        {
            MatchedVariables other = new MatchedVariables(this.variables.Keys.ToArray());
            foreach (var entry in this.variables)
            {
                if (entry.Value.IsDefined)
                {
                    other.variables[entry.Key] = entry.Value;
                }
            }
            return other;
        }

        public MatchedVariables(IEnumerable<ulong> ids)
        {
            this.variables = new Dictionary<ulong, Option<ID>>();
            foreach (ulong id in ids)
            {
                this.variables.Add(id, Option<ID>.None());
            }
        }

        public Option<Dictionary<ulong, ID>> CheckExpressions(IList<Expression> expressions)
        {
            Option<Dictionary<ulong, ID>> vars = this.Complete();
            if (vars.IsDefined)
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

                return Option.Some(variables);
            }
            else
            {
                return Option<Dictionary<ulong, ID>>.None();
            }
        }
    }
}