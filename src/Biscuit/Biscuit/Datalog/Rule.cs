using Biscuit.Datalog.Constraints;
using Biscuit.Datalog.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public class Rule
    {
        public Predicate Head
        {
            get;
        }

        public IList<Predicate> Body
        {
            get;
        }

        public IList<Expression> Expressions
        {
            get;
        }

        public void Apply(HashSet<Fact> facts, HashSet<Fact> newFacts, HashSet<ulong> restrictedSymbols)
        {
            HashSet<ulong> variablesSet = new HashSet<ulong>();
            foreach (Predicate pred in this.Body)
            {
                variablesSet.AddAll(pred.Ids.Where(id => id is ID.Variable).Select(id => ((ID.Variable)id).Value));
            }
            MatchedVariables variables = new MatchedVariables(variablesSet);

            if (!this.Body.Any())
            {
                Option<Dictionary<ulong, ID>> h_opt = variables.CheckExpressions(this.Expressions);
                if (h_opt.IsDefined)
                {
                    Dictionary<ulong, ID> h = h_opt.Get();
                    Predicate predicate = this.Head.Clone(); 

                    for (int i = 0; i < predicate.Ids.Count; i++)
                    {
                        ID id = predicate.Ids[i];
                        if (id is ID.Variable)
                        {
                            ID value = h[((ID.Variable)id).Value];
                            predicate.Ids[i] = value;
                        }
                    }
                    
                    newFacts.Add(new Fact(predicate));                                        
                }
            }

            var combined = new Combinator(variables, this.Body, this.Expressions, facts).Combine();

            foreach (Dictionary<ulong, ID> h in combined)
            {
                Predicate predicate = this.Head.Clone();
                bool unbound_variable = false;

                for (int i=0; i < predicate.Ids.Count; i++)
                {
                    ID id = predicate.Ids[i];
                    if (id is ID.Variable)
                    {
                        bool isInDictionnary = h.TryGetValue(((ID.Variable)id).Value, out ID value);
                        
                        predicate.Ids[i] = value;
                        // variables that appear in the head should appear in the body and constraints as well
                        if (value == null)
                        {
                            unbound_variable = true;
                        }
                    }
                }
                // if the generated fact has #authority or #ambient as first element and we're n ot in a privileged rule
                // do not generate it
                ID first = predicate.Ids.FirstOrDefault();
                if (first != null && first is ID.Symbol)
                {
                    if (restrictedSymbols.Contains(((ID.Symbol)first).Value))
                    {
                        continue;
                    }
                }
                if (!unbound_variable)
                {
                    newFacts.Add(new Fact(predicate));
                }
            }
        }

        // do not produce new facts, only find one matching set of facts
        public bool Test(HashSet<Fact> facts)
        {
            HashSet<ulong> variables_set = new HashSet<ulong>();
            foreach (Predicate pred in this.Body)
            {
                variables_set.AddAll(pred.Ids.Where((id)=>id is ID.Variable).Select(id => ((ID.Variable)id).Value));
            }
            MatchedVariables variables = new MatchedVariables(variables_set);

            if (!this.Body.Any())
            {
                return variables.CheckExpressions(this.Expressions).IsDefined;
            }

            Combinator c = new Combinator(variables, this.Body, this.Expressions, facts);

            return c.Next().IsDefined;
        }

        public Rule(Predicate head, List<Predicate> body, List<Expression> expressions)
        {
            this.Head = head;
            this.Body = body;
            this.Expressions = expressions;
        }

        public Format.Schema.RuleV1 Serialize()
        {
            Format.Schema.RuleV1 b = new Format.Schema.RuleV1(){ Head = this.Head.Serialize() };

            for (int i = 0; i < this.Body.Count; i++)
            {
                b.Body.Add(this.Body[i].Serialize());
            }

            for (int i = 0; i < this.Expressions.Count; i++)
            {
                b.Expressions.Add(this.Expressions[i].Serialize());
            }

            return b;
        }

        static public Either<Errors.FormatError, Rule> DeserializeV0(Format.Schema.RuleV0 rule)
        {
            List<Predicate> body = new List<Predicate>();
            foreach (Format.Schema.PredicateV0 predicate in rule.Body)
            {
                Either<Errors.FormatError, Predicate> result = Predicate.DeserializeV0(predicate);
                if (result.IsLeft)
                {
                    return result.Left;
                }
                else
                {
                    body.Add(result.Right);
                }
            }

            List<Expression> expressions = new List<Expression>();
            foreach (Format.Schema.ConstraintV0 constraint in rule.Constraints)
            {
                Either<Errors.FormatError, Expression> result = Constraint.DeserializeV0(constraint);
                if (result.IsLeft)
                {
                    return result.Left;
                }
                else
                {
                    expressions.Add(result.Right);
                }
            }

            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV0(rule.Head);
            if (res.IsLeft)
            {
                return res.Left;
            }
            else
            {
                return new Rule(res.Right, body, expressions);
            }
        }

        static public Either<Errors.FormatError, Rule> DeserializeV1(Format.Schema.RuleV1 rule)
        {
            List<Predicate> body = new List<Predicate>();
            foreach (Format.Schema.PredicateV1 predicate in rule.Body)
            {
                Either<Errors.FormatError, Predicate> result = Predicate.DeserializeV1(predicate);
                if (result.IsLeft)
                {
                    return result.Left;
                }
                else
                {
                    body.Add(result.Right);
                }
            }

            List<Expression> expressions = new List<Expression>();
            foreach (Format.Schema.ExpressionV1 expression in rule.Expressions)
            {
                Either<Errors.FormatError, Expression> result = Expression.DeserializeV1(expression);
                if (result.IsLeft)
                {
                    return result.Left;
                }
                else
                {
                    expressions.Add(result.Right);
                }
            }

            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV1(rule.Head);
            if (res.IsLeft)
            {
                return res.Left;
            }
            else
            {
                return new Rule(res.Right, body, expressions);
            }
        }
    }
}
