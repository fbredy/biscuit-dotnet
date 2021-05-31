using Biscuit.Datalog.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class Combinator
    {
        private readonly MatchedVariables variables;
        private readonly IList<Predicate> nextPredicates;
        private readonly IList<Expression> expressions;
        private readonly HashSet<Fact> allFacts;
        private readonly Predicate pred;
        private readonly IEnumerator<Fact> fit;
        private Combinator currentIt;


        public Option<MatchedVariables> Next()
        {
            while (true)
            {
                if (this.currentIt != null)
                {
                    Option<MatchedVariables> next_vars_opt = this.currentIt.Next();
                    // the iterator is empty, try with the next fact
                    if (next_vars_opt.IsEmpty())
                    {
                        this.currentIt = null;
                        continue;
                    }

                    MatchedVariables nextVars = next_vars_opt.Get();

                    Option<Dictionary<ulong, ID>> v_opt = nextVars.CheckExpressions(this.expressions);
                    if (v_opt.IsEmpty())
                    {
                        continue;
                    }
                    else
                    {
                        return Option<MatchedVariables>.Some(nextVars);
                    }
                }

                // we iterate over the facts that match the current predicate
                if (this.fit.MoveNext())
                {
                    Fact current_fact = this.fit.Current;

                    // create a new MatchedVariables in which we fix variables we could unify from our first predicate and the current fact
                    MatchedVariables vars = this.variables.Clone();
                    bool match_ids = true;

                    // we know the fact matches the predicate's format so they have the same number of terms
                    // fill the MatchedVariables before creating the next combinator
                    for (int i = 0; i < pred.Ids.Count; ++i)
                    {
                        ID id = pred.Ids[i];
                        if (id is ID.Variable idVariable)
                        {
                            ulong key = idVariable.Value;
                            ID value = current_fact.predicate.Ids[i];

                            if (!vars.Insert(key, value))
                            {
                                match_ids = false;
                            }
                            if (!match_ids)
                            {
                                break;
                            }
                        }
                    }

                    if (!match_ids)
                    {
                        continue;
                    }

                    // there are no more predicates to check
                    if (!nextPredicates.Any())
                    {
                        Option<Dictionary<ulong, ID>> v_opt = vars.CheckExpressions(this.expressions);
                        if (v_opt.IsEmpty())
                        {
                            continue;
                        }
                        else
                        {
                            return Option<MatchedVariables>.Some(vars);
                        }
                    }
                    else
                    {
                        // we found a matching fact, we create a new combinator over the rest of the predicates
                        // no need to copy all of the expressions at all levels
                        this.currentIt = new Combinator(vars, nextPredicates, new List<Expression>(), this.allFacts);
                    }
                }
                else
                {
                    break;
                }
            }

            return Option<MatchedVariables>.None();
        }

        public IList<Dictionary<ulong, ID>> Combine()
        {
            IList<Dictionary<ulong, ID>> variables = new List<Dictionary<ulong, ID>>();

            while (true)
            {
                Option<MatchedVariables> res = this.Next();

                if (res.IsEmpty())
                {
                    return variables;
                }

                Optional<Dictionary<ulong, ID>> vars = res.Get().Complete();
                if (vars.IsPresent())
                {
                    variables.Add(vars.Get());
                }
            }
        }

        public Combinator(MatchedVariables variables, IList<Predicate> predicates,
            IList<Expression> expressions, HashSet<Fact> all_facts)
        {
            this.variables = variables;
            this.expressions = expressions;
            this.allFacts = all_facts;
            this.currentIt = null;
            this.pred = predicates.FirstOrDefault();
            this.fit = all_facts.Where(fact => fact.match_predicate(this.pred)).GetEnumerator();

            IList<Predicate> next_predicates = predicates.Skip(1).ToList();

            this.nextPredicates = next_predicates;
        }
    }
}
