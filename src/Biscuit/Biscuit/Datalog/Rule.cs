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
        public Predicate head
        {
            get;
        }

        public List<Predicate> body
        {
            get;
        }

        public List<Expression> expressions
        {
            get;
        }

        public void apply(HashSet<Fact> facts, HashSet<Fact> new_facts, HashSet<ulong> restricted_symbols)
        {
            HashSet<ulong> variables_set = new HashSet<ulong>();
            foreach (Predicate pred in this.body)
            {
                variables_set.addAll(pred.ids.Where(id=>id is ID.Variable).Select(id=> ((ID.Variable)id).value));
            }
            MatchedVariables variables = new MatchedVariables(variables_set);

            if (!this.body.Any())
            {
                Option<Dictionary<ulong, ID>> h_opt = variables.check_expressions(this.expressions);
                if (h_opt.isDefined())
                {
                    Dictionary<ulong, ID> h = h_opt.get();
                    Predicate p = this.head.clone(); 

                    for (int i = 0;i<p.ids.Count; i++)
                    {
                        ID id = p.ids[i];
                        if (id is ID.Variable)
                        {
                            ID value = h[((ID.Variable)id).value];
                            p.ids[i] = value;
                            
                        }
                    }
                    
                    new_facts.Add(new Fact(p));                                        
                }
            }

            foreach (Dictionary<ulong, ID> h in new Combinator(variables, this.body, this.expressions, facts).combine())
            {
                Predicate p = this.head.clone();
                bool unbound_variable = false;

                for (int i=0; i<p.ids.Count; i++)
                {
                    ID id = p.ids[i];
                    if (id is ID.Variable)
                    {
                        bool isInDictionnary = h.TryGetValue(((ID.Variable)id).value, out ID value);
                        //ID value = h[((ID.Variable)id).value];
                        p.ids[i] = value;
                        // variables that appear in the head should appear in the body and constraints as well
                        if (value == null)
                        {
                            unbound_variable = true;
                        }
                    }
                }
                // if the generated fact has #authority or #ambient as first element and we're n ot in a privileged rule
                // do not generate it
                ID first = p.ids.FirstOrDefault();
                if (first != null && first is ID.Symbol)
                {
                    if (restricted_symbols.Contains(((ID.Symbol)first).value))
                    {
                        continue;
                    }
                }
                if (!unbound_variable)
                {
                    new_facts.Add(new Fact(p));
                }
            }
        }

        // do not produce new facts, only find one matching set of facts
        public bool test(HashSet<Fact> facts)
        {
            HashSet<ulong> variables_set = new HashSet<ulong>();
            foreach (Predicate pred in this.body)
            {
                variables_set.addAll(pred.ids.Where((id)=>id is ID.Variable).Select(id => ((ID.Variable)id).value));
            }
            MatchedVariables variables = new MatchedVariables(variables_set);

            if (!this.body.Any())
            {
                return variables.check_expressions(this.expressions).isDefined();
            }

            Combinator c = new Combinator(variables, this.body, this.expressions, facts);

            return c.next().isDefined();
        }

        public Rule(Predicate head, List<Predicate> body, List<Expression> expressions)
        {
            this.head = head;
            this.body = body;
            this.expressions = expressions;
        }

        public Format.Schema.RuleV1 serialize()
        {
            Format.Schema.RuleV1 b = new Format.Schema.RuleV1(){ Head = this.head.serialize() };

            for (int i = 0; i < this.body.Count; i++)
            {
                b.Body.Add(this.body[i].serialize());
            }

            for (int i = 0; i < this.expressions.Count; i++)
            {
                b.Expressions.Add(this.expressions[i].serialize());
            }

            return b;
        }

        static public Either<Errors.FormatError, Rule> deserializeV0(Format.Schema.RuleV0 rule)
        {
            List<Predicate> body = new List<Predicate>();
            foreach (Format.Schema.PredicateV0 predicate in rule.Body)
            {
                Either<Errors.FormatError, Predicate> result = Predicate.deserializeV0(predicate);
                if (result.IsLeft)
                {
                    Errors.FormatError e = result.Left;
                    return new Left(e);
                }
                else
                {
                    body.Add(result.Right);
                }
            }

            List<Expression> expressions = new List<Expression>();
            foreach (Format.Schema.ConstraintV0 constraint in rule.Constraints)
            {
                Either<Errors.FormatError, Expression> result = Constraint.deserializeV0(constraint);
                if (result.IsLeft)
                {
                    Errors.FormatError e = result.Left;
                    return new Left(e);
                }
                else
                {
                    expressions.Add(result.Right);
                }
            }

            Either<Errors.FormatError, Predicate> res = Predicate.deserializeV0(rule.Head);
            if (res.IsLeft)
            {
                Errors.FormatError e = res.Left;
                return new Left(e);
            }
            else
            {
                return new Right(new Rule(res.Right, body, expressions));
            }
        }

        static public Either<Errors.FormatError, Rule> deserializeV1(Format.Schema.RuleV1 rule)
        {
            List<Predicate> body = new List<Predicate>();
            foreach (Format.Schema.PredicateV1 predicate in rule.Body)
            {
                Either<Errors.FormatError, Predicate> result = Predicate.deserializeV1(predicate);
                if (result.IsLeft)
                {
                    Errors.FormatError e = result.Left;
                    return new Left(e);
                }
                else
                {
                    body.Add(result.Right);
                }
            }

            List<Expression> expressions = new List<Expression>();
            foreach (Format.Schema.ExpressionV1 expression in rule.Expressions)
            {
                Either<Errors.FormatError, Expression> result = Expression.deserializeV1(expression);
                if (result.IsLeft)
                {
                    Errors.FormatError e = result.Left;
                    return new Left(e);
                }
                else
                {
                    expressions.Add(result.Right);
                }
            }

            Either<Errors.FormatError, Predicate> res = Predicate.deserializeV1(rule.Head);
            if (res.IsLeft)
            {
                Errors.FormatError e = res.Left;
                return new Left(e);
            }
            else
            {
                return new Right(new Rule(res.Right, body, expressions));
            }
        }
    }
}
