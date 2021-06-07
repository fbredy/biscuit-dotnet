using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder.Parser
{
    public class Parser
    {
        public static Either<Error, Tuple<string, FactBuilder>> Fact(string s)
        {
            Either<Error, Tuple<string, PredicateBuilder>> res = FactPredicate(s);
            if (res.IsLeft)
            {
                return res.Left;
            }
            else
            {
                Tuple<string, PredicateBuilder> t = res.Right;
                if (!t.Item1.IsEmpty())
                {
                    return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
                }
                return new Tuple<string, FactBuilder>(t.Item1, new FactBuilder(t.Item2));
            }
        }

        public static Either<Error, Tuple<string, RuleBuilder>> Rule(string s)
        {
            Either<Error, Tuple<string, PredicateBuilder>> predicate = Predicate(s);
            if (predicate.IsLeft)
            {
                return predicate.Left;
            }

            Tuple<string, PredicateBuilder> builder = predicate.Right;
            s = builder.Item1;
            PredicateBuilder head = builder.Item2;

            s = Space(s);
            if (s.Length < 2 || s[0] != '<' || s[1] != '-')
            {
                return new Error(s, "rule arrow not found");
            }

            s = s.Substring(2);

            Either<Error, Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>>> bodyRes = RuleBody(s);
            if (bodyRes.IsLeft)
            {
                return bodyRes.Left;
            }

            Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>> body = bodyRes.Right;

            if (!body.Item1.IsEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + body.Item1);
            }

            return new Tuple<string, RuleBuilder>(body.Item1, new RuleBuilder(head, body.Item2, body.Item3));
        }

        public static Either<Error, Tuple<string, CheckBuilder>> Check(string s)
        {
            string prefix = "check if";
            if (!s.StartsWith(prefix))
            {
                return new Error(s, "missing check prefix");
            }

            s = s.Substring(prefix.Length);

            Either<Error, Tuple<string, List<RuleBuilder>>> bodyRes = CheckBody(s);
            if (bodyRes.IsLeft)
            {
                return bodyRes.Left;
            }

            Tuple<string, List<RuleBuilder>> t = bodyRes.Right;
            if (!t.Item1.IsEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
            }
            return new Tuple<string, CheckBuilder>(t.Item1, new CheckBuilder(t.Item2));
        }

        public static Either<Error, Tuple<string, Policy>> Policy(string s)
        {
            Policy.Kind p = Token.Policy.Kind.Allow;

            string allow = "allow if";
            string deny = "deny if";
            if (s.StartsWith(allow))
            {
                s = s.Substring(allow.Length);
            }
            else if (s.StartsWith(deny))
            {
                p = Token.Policy.Kind.Deny;
                s = s.Substring(deny.Length);
            }
            else
            {
                return new Error(s, "missing policy prefix");
            }

            Either<Error, Tuple<string, List<RuleBuilder>>> bodyRes = CheckBody(s);
            if (bodyRes.IsLeft)
            {
                return bodyRes.Left;
            }

            Tuple<string, List<RuleBuilder>> t = bodyRes.Right;

            if (!t.Item1.IsEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
            }

            return new Tuple<string, Policy>(t.Item1, new Policy(t.Item2, p));
        }

        public static Either<Error, Tuple<string, List<RuleBuilder>>> CheckBody(string s)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();
            Either<Error, Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>>> bodyRes = RuleBody(s);
            if (bodyRes.IsLeft)
            {
                return bodyRes.Left;
            }

            Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>> body = bodyRes.Right;

            s = body.Item1;
            queries.Add(new RuleBuilder(new PredicateBuilder("query", new List<Term>()), body.Item2, body.Item3));
            bool isBreak = false;
            while (!isBreak)
            {
                if (s.Length != 0)
                {
                    s = Space(s);

                    if (s.StartsWith("or"))
                    {
                        s = s.Substring(2);

                        Either<Error, Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>>> bodyRes2 = RuleBody(s);
                        if (bodyRes2.IsLeft)
                        {
                            return bodyRes2.Left;
                        }

                        Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>> body2 = bodyRes2.Right;

                        s = body2.Item1;
                        queries.Add(new RuleBuilder(new PredicateBuilder("query", new List<Term>()), body2.Item2, body2.Item3));
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else
                {
                    isBreak = true;
                }
            }

            return new Tuple<string, List<RuleBuilder>>(s, queries);
        }

        public static Either<Error, Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>>> RuleBody(string s)
        {
            List<PredicateBuilder> predicates = new List<PredicateBuilder>();
            List<ExpressionBuilder> expressions = new List<ExpressionBuilder>();
            bool isError = false;
            bool hasNext = true;

            while (!isError && hasNext)
            {
                s = Space(s);

                Either<Error, Tuple<string, PredicateBuilder>> res = Predicate(s);
                if (res.IsRight)
                {
                    Tuple<string, PredicateBuilder> t = res.Right;
                    s = t.Item1;
                    predicates.Add(t.Item2);
                }
                else
                {
                    Either<Error, Tuple<string, ExpressionBuilder>> res2 = Expression(s);
                    if (res2.IsRight)
                    {
                        Tuple<string, ExpressionBuilder> t2 = res2.Right;
                        s = t2.Item1;
                        expressions.Add(t2.Item2);
                    }
                    else
                    {
                        isError = true;
                    }
                }

                s = Space(s);

                if (s.Length != 0 && s[0] == ',')
                {
                    s = s.Substring(1);
                }
                else
                {
                    hasNext = false;
                }
            }

            //FIXME: handle constraints

            return new Tuple<string, List<PredicateBuilder>, List<ExpressionBuilder>>(s, predicates, expressions);
        }

        public static Either<Error, Tuple<string, PredicateBuilder>> Predicate(string s)
        {
            Tuple<string, string> tn = TakeWhile(s, c => char.IsLetter(c) || c == '_');
            string name = tn.Item1;
            s = tn.Item2;

            s = Space(s);
            if (s.Length == 0 || s[0] != '(')
            {
                return new Error(s, "opening parens not found");
            }
            s = s.Substring(1);

            List<Term> terms = new List<Term>();
            bool isBreak = false;
            while (!isBreak)
            {
                s = Space(s);

                Either<Error, Tuple<string, Term>> res = Term(s);
                if (!res.IsLeft)
                {
                    Tuple<string, Term> t = res.Right;
                    s = t.Item1;
                    terms.Add(t.Item2);

                    s = Space(s);

                    if (s.Length == 0 || s[0] != ',')
                    {
                        isBreak = true;
                    }
                    else
                    {
                        s = s.Substring(1);
                    }
                }
                else
                {
                    isBreak = true;
                }
            }

            s = Space(s);
            if (0 == s.Length || s[0] != ')')
            {
                return new Error(s, "closing parens not found");
            }
            string remaining = s.Substring(1);

            return new Tuple<string, PredicateBuilder>(remaining, new PredicateBuilder(name, terms));
        }

        public static Either<Error, Tuple<string, PredicateBuilder>> FactPredicate(string s)
        {
            Tuple<string, string> tn = TakeWhile(s, c => char.IsLetter(c) || c == '_');
            string name = tn.Item1;
            s = tn.Item2;

            s = Space(s);
            if (s.Length == 0 || s[0] != '(')
            {
                return new Error(s, "opening parens not found");
            }
            s = s.Substring(1);

            List<Term> terms = new List<Term>();
            bool isBreak = false;
            while (!isBreak)
            {
                s = Space(s);

                Either<Error, Tuple<string, Term>> res = FactTerm(s);
                if (!res.IsLeft)
                {
                    Tuple<string, Term> t = res.Right;
                    s = t.Item1;
                    terms.Add(t.Item2);

                    s = Space(s);

                    if (s.Length != 0 && s[0] == ',')
                    {
                        s = s.Substring(1);
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else
                {
                    isBreak = true;
                }
            }

            s = Space(s);
            if (0 == s.Length || s[0] != ')')
            {
                return new Error(s, "closing parens not found");
            }
            string remaining = s.Substring(1);

            return new Tuple<string, PredicateBuilder>(remaining, new PredicateBuilder(name, terms));
        }

        public static Either<Error, Tuple<string, string>> Name(string s)
        {
            Tuple<string, string> t = TakeWhile(s, (c) => char.IsLetter(c) || c == '_');
            string name = t.Item1;
            string remaining = t.Item2;

            return new Tuple<string, string>(remaining, name);
        }

        public static Either<Error, Tuple<string, Term>> Term(string s)
        {
            Either<Error, Tuple<string, Term.Symbol>> symbolTerm = Symbol(s);
            if (symbolTerm.IsRight)
            {
                var t = symbolTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Variable>> variableTerm = Variable(s);
            if (variableTerm.IsRight)
            {
                var t = variableTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Str>> stringTerm = Strings(s);
            if (stringTerm.IsRight)
            {
                var t = stringTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Set>> setTerm = Set(s);
            if (setTerm.IsRight)
            {
                var t = setTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Bool>> boolTerm = Boolean(s);
            if (boolTerm.IsRight)
            {
                var t = boolTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Date>> dateTerm = Date(s);
            if (dateTerm.IsRight)
            {
                var t = dateTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Integer>> intTerm = Integer(s);
            if (intTerm.IsRight)
            {
                var t = intTerm.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            return new Error(s, "unrecognized value");
        }

        public static Either<Error, Tuple<string, Term>> FactTerm(string s)
        {
            if (s.Length > 0 && s[0] == '$')
            {
                return new Error(s, "variables are not allowed in facts");
            }

            Either<Error, Tuple<string, Term.Symbol>> res1 = Symbol(s);
            if (res1.IsRight)
            {
                Tuple<string, Term.Symbol> t = res1.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Str>> res2 = Strings(s);
            if (res2.IsRight)
            {
                Tuple<string, Term.Str> t = res2.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Set>> res7 = Set(s);
            if (res7.IsRight)
            {
                Tuple<string, Term.Set> t = res7.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Bool>> res6 = Boolean(s);
            if (res6.IsRight)
            {
                Tuple<string, Term.Bool> t = res6.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Date>> res4 = Date(s);
            if (res4.IsRight)
            {
                Tuple<string, Term.Date> t = res4.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Integer>> res3 = Integer(s);
            if (res3.IsRight)
            {
                Tuple<string, Term.Integer> t = res3.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            return new Error(s, "unrecognized value");
        }

        public static Either<Error, Tuple<string, Term.Symbol>> Symbol(string s)
        {
            if (s[0] != '#')
            {
                return new Error(s, "not a symbol");
            }

            Tuple<string, string> t = TakeWhile(s.Substring(1), c => char.IsLetterOrDigit(c) || c == '_');
            string name = t.Item1;
            string remaining = t.Item2;

            return new Tuple<string, Term.Symbol>(remaining, (Term.Symbol)Utils.Symbol(name));
        }

        public static Either<Error, Tuple<string, Term.Str>> Strings(string s)
        {
            if (s[0] != '"')
            {
                return new Error(s, "not a string");
            }

            int index = s.Length;
            bool found = false;
            for (int i = 1; i < s.Length && !found; i++)
            {
                char c = s[i];

                if (c == '\\' && s[i + 1] == '"')
                {
                    i += 1;
                }
                else if (c == '"')
                {
                    index = i - 1;
                    found = true;
                }
            }

            if (index == s.Length)
            {
                return new Error(s, "end of string not found");
            }

            if (s[index + 1] != '"')
            {
                return new Error(s, "ending double quote not found");
            }

            //Be careful s.Substring(int,int) is différent between java and cs
            // java : takes two indexes
            // CS : takes index and length
            string substring = s.Substring(1, index);
            string remaining = s.Substring(index + 2);

            return new Tuple<string, Term.Str>(remaining, (Term.Str)Utils.Strings(substring));
        }

        public static Either<Error, Tuple<string, Term.Integer>> Integer(string s)
        {
            int index = 0;
            if (s[0] == '-')
            {
                index += 1;
            }

            int index2 = s.Length;
            bool found = false;
            for (int i = index; i < s.Length && !found; i++)
            {
                char c = s[i];

                if (!char.IsDigit(c))
                {
                    index2 = i;
                    found = true;
                }
            }

            if (index2 == 0)
            {
                return new Error(s, "not an integer");
            }

            long j = long.Parse(s.Substring(0, index2));
            string remaining = s.Substring(index2);


            return new Tuple<string, Term.Integer>(remaining, (Term.Integer)Utils.Integer(j));
        }

        public static Either<Error, Tuple<string, Term.Date>> Date(string s)
        {
            Tuple<string, string> t = TakeWhile(s, (c) => c != ' ' && c != ',' && c != ')');

            try
            {
                DateTimeOffset d = DateTimeOffset.Parse(t.Item1);

                string remaining = t.Item2;
                return new Tuple<string, Term.Date>(remaining, new Term.Date((ulong)d.ToUnixTimeSeconds()));
            }
            catch (FormatException)
            {
                return new Error(s, "not a date");

            }
        }

        public static Either<Error, Tuple<string, Term.Variable>> Variable(string s)
        {
            if (s[0] != '$')
            {
                return new Error(s, "not a variable");
            }

            Tuple<string, string> t = TakeWhile(s.Substring(1), (c) => char.IsLetterOrDigit(c) || c == '_');

            return new Tuple<string, Term.Variable>(t.Item2, (Term.Variable)Utils.Var(t.Item1));
        }

        public static Either<Error, Tuple<string, Term.Bool>> Boolean(string s)
        {
            bool b;
            if (s.StartsWith("true"))
            {
                b = true;
                s = s.Substring(4);
            }
            else if (s.StartsWith("false"))
            {
                b = false;
                s = s.Substring(5);
            }
            else
            {
                return new Error(s, "not a boolean");
            }

            return new Tuple<string, Term.Bool>(s, new Term.Bool(b));
        }

        public static Either<Error, Tuple<string, Term.Set>> Set(string s)
        {
            if (s.Length == 0 || s[0] != '[')
            {
                return new Error(s, "not a set");
            }

            s = s.Substring(1);

            HashSet<Term> terms = new HashSet<Term>();
            bool isBreak = false;
            while (!isBreak)
            {

                s = Space(s);

                Either<Error, Tuple<string, Term>> res = FactTerm(s);
                if (res.IsRight)
                {
                    Tuple<string, Term> t = res.Right;

                    if (t.Item2 is Term.Variable)
                    {
                        return new Error(s, "sets cannot contain variables");
                    }

                    s = t.Item1;
                    terms.Add(t.Item2);

                    s = Space(s);

                    if (s.Length != 0 && s[0] == ',')
                    {
                        s = s.Substring(1);
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else
                {
                    isBreak = true;
                }
            }

            s = Space(s);
            if (0 == s.Length || s[0] != ']')
            {
                return new Error(s, "closing square bracket not found");
            }

            string remaining = s.Substring(1);

            return new Tuple<string, Term.Set>(remaining, new Term.Set(terms));
        }

        public static Either<Error, Tuple<string, ExpressionBuilder>> Expression(string s)
        {
            return ExpressionParser.Parse(s);
        }

        public static string Space(string s)
        {
            int index = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
                {
                    break;
                }
                index += 1;
            }

            return s.Substring(index);
        }

        public static Tuple<string, string> TakeWhile(string s, Func<char, bool> f)
        {
            int index = s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (!f(c))
                {
                    index = i;
                    break;
                }
            }

            return new Tuple<string, string>(s.Substring(0, index), s.Substring(index));
        }
    }
}
