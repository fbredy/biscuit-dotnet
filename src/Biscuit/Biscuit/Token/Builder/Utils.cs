using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class Utils
    {
        public static Term Set(HashSet<Term> s)
        {
            return new Term.Set(s);
        }

        public static FactBuilder Fact(string name, List<Term> ids)
        {
            return new FactBuilder(name, ids);
        }

        public static PredicateBuilder Pred(string name, List<Term> ids)
        {
            return new PredicateBuilder(name, ids);
        }

        public static RuleBuilder Rule(string headName, List<Term> headIds,
                                                                      List<PredicateBuilder> predicates)
        {
            return new RuleBuilder(Pred(headName, headIds), predicates, new List<ExpressionBuilder>());
        }

        public static RuleBuilder ConstrainedRule(string headName, List<Term> headIds,
                                                    List<PredicateBuilder> predicates,
                                                    List<ExpressionBuilder> expressions)
        {
            return new RuleBuilder(Pred(headName, headIds), predicates, expressions);
        }

        public static CheckBuilder Check(RuleBuilder rule)
        {
            return new CheckBuilder(rule);
        }

        public static Term Integer(long i)
        {
            return new Term.Integer(i);
        }

        public static Term Strings(string str)
        {
            return new Term.Str(str);
        }

        public static Term Symbol(string str)
        {
            return new Term.Symbol(str);
        }

        public static Term Date(DateTime d)
        {
            return new Term.Date((ulong)((DateTimeOffset)d).ToUnixTimeSeconds());
        }

        public static Term Var(string name)
        {
            return new Term.Variable(name);
        }
    }
}
