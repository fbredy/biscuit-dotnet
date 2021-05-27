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
        private readonly List<Predicate> next_predicates;
        private readonly List<Expression> expressions;
        private readonly HashSet<Fact> all_facts;
        private readonly Predicate pred;
        private readonly IEnumerator<Fact> fit;
        private Combinator current_it;


        public Option<MatchedVariables> next()
        {
            while (true)
            {
                if (this.current_it != null)
                {
                    Option<MatchedVariables> next_vars_opt = this.current_it.next();
                    // the iterator is empty, try with the next fact
                    if (next_vars_opt.isEmpty())
                    {
                        this.current_it = null;
                        continue;
                    }

                    MatchedVariables next_vars = next_vars_opt.get();

                    Option<Dictionary<ulong, ID>> v_opt = next_vars.check_expressions(this.expressions);
                    if (v_opt.isEmpty())
                    {
                        continue;
                    }
                    else
                    {
                        return Option<MatchedVariables>.some(next_vars);
                    }
                }

                // we iterate over the facts that match the current predicate
                if (this.fit.MoveNext())
                {
                    Fact current_fact = this.fit.Current;

                    // create a new MatchedVariables in which we fix variables we could unify from our first predicate and the current fact
                    MatchedVariables vars = this.variables.clone();
                    bool match_ids = true;

                    // we know the fact matches the predicate's format so they have the same number of terms
                    // fill the MatchedVariables before creating the next combinator
                    for (int i = 0; i < pred.ids.Count; ++i)
                    {
                        ID id = pred.ids[i];
                        if (id is ID.Variable)
                        {
                            ulong key = ((ID.Variable)id).value;
                            ID value = current_fact.predicate.ids[i];

                            if (!vars.insert(key, value))
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
                    if (!next_predicates.Any())
                    {
                        Option<Dictionary<ulong, ID>> v_opt = vars.check_expressions(this.expressions);
                        if (v_opt.isEmpty())
                        {
                            continue;
                        }
                        else
                        {
                            return Option<MatchedVariables>.some(vars);
                        }
                    }
                    else
                    {
                        // we found a matching fact, we create a new combinator over the rest of the predicates
                        // no need to copy all of the expressions at all levels
                        this.current_it = new Combinator(vars, next_predicates, new List<Expression>(), this.all_facts);
                    }
                }
                else
                {
                    break;
                }
            }

            return Option<MatchedVariables>.none();
        }

        public List<Dictionary<ulong, ID>> combine()
        {
            List<Dictionary<ulong, ID>> variables = new List<Dictionary<ulong, ID>>();

            while (true)
            {
                Option<MatchedVariables> res = this.next();

                if (res.isEmpty())
                {
                    return variables;
                }

                Optional<Dictionary<ulong, ID>> vars = res.get().complete();
                if (vars.isPresent())
                {
                    variables.Add(vars.get());
                }
            }
        }

        public Combinator(MatchedVariables variables, List<Predicate> predicates,
            List<Expression> expressions, HashSet<Fact> all_facts)
        {
            this.variables = variables;
            this.expressions = expressions;
            this.all_facts = all_facts;
            this.current_it = null;
            this.pred = predicates.FirstOrDefault();
            this.fit = all_facts.Where(fact => fact.match_predicate(this.pred)).GetEnumerator();

            List<Predicate> next_predicates = predicates.Skip(1).ToList();

            this.next_predicates = next_predicates;
        }
    }
}
