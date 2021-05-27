using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder.Parser
{
    public class Parser
    {
        public static Either<Error, Tuple<string, Fact>> fact(string s)
        {
            Either<Error, Tuple<string, Predicate>> res = fact_predicate(s);
            if (res.IsLeft)
            {
                return new Left(res.Left);
            }
            else
            {
                Tuple<string, Predicate> t = res.Right;
                if (!t.Item1.isEmpty())
                {
                    return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
                }
                return new Right(new Tuple<string, Fact>(t.Item1, new Fact(t.Item2)));
            }
        }

        public static Either<Error, Tuple<string, Rule>> rule(string s)
        {
            Either<Error, Tuple<string, Predicate>> res0 = predicate(s);
            if (res0.IsLeft)
            {
                return new Left(res0.Left);
            }

            Tuple<string, Predicate> t0 = res0.Right;
            s = t0.Item1;
            Predicate head = t0.Item2;

            s = space(s);
            if (s.Length < 2 || s[0] != '<' || s[1] != '-')
            {
                return new Left(new Error(s, "rule arrow not found"));
            }

            List<Predicate> predicates = new List<Predicate>();
            s = s.Substring(2);

            Either<Error, Tuple<string, List<Predicate>, List<Expression>>> bodyRes = rule_body(s);
            if (bodyRes.IsLeft)
            {
                return new Left(bodyRes.Left);
            }

            Tuple<string, List<Predicate>, List<Expression>> body = bodyRes.Right;

            if (!body.Item1.isEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + body.Item1);
            }

            return new Right(new Tuple<string, Rule>(body.Item1, new Rule(head, body.Item2, body.Item3)));
        }

        public static Either<Error, Tuple<string, Check>> Check(string s)
        {
            string prefix = "check if";
            if (!s.StartsWith(prefix))
            {
                return new Left(new Error(s, "missing check prefix"));
            }

            s = s.Substring(prefix.Length);

            List<Rule> queries = new List<Rule>();
            Either<Error, Tuple<string, List<Rule>>> bodyRes = check_body(s);
            if (bodyRes.IsLeft)
            {
                return new Left(bodyRes.Left);
            }

            Tuple<string, List<Rule>> t = bodyRes.Right;
            if (!t.Item1.isEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
            }
            return new Right(new Tuple<string, Check>(t.Item1, new Check(t.Item2)));
        }

        public static Either<Error, Tuple<string, Policy>> policy(string s)
        {
            Policy.Kind p = Policy.Kind.Allow;

            string allow = "allow if";
            string deny = "deny if";
            if (s.StartsWith(allow))
            {
                s = s.Substring(allow.Length);
            }
            else if (s.StartsWith(deny))
            {
                p = Policy.Kind.Deny;
                s = s.Substring(deny.Length);
            }
            else
            {
                return new Left(new Error(s, "missing policy prefix"));
            }

            List<Rule> queries = new List<Rule>();
            Either<Error, Tuple<string, List<Rule>>> bodyRes = check_body(s);
            if (bodyRes.IsLeft)
            {
                return new Left(bodyRes.Left);
            }

            Tuple<string, List<Rule>> t = bodyRes.Right;

            if (!t.Item1.isEmpty())
            {
                return new Error(s, "the string was not entirely parsed, remaining: " + t.Item1);
            }

            return new Right(new Tuple<string, Policy>(t.Item1, new Policy(t.Item2, p)));
        }

        public static Either<Error, Tuple<string, List<Rule>>> check_body(string s)
        {
            List<Rule> queries = new List<Rule>();
            Either<Error, Tuple<string, List<Predicate>, List<Expression>>> bodyRes = rule_body(s);
            if (bodyRes.IsLeft)
            {
                return new Left(bodyRes.Left);
            }

            Tuple<string, List<Predicate>, List<Expression>> body = bodyRes.Right;

            s = body.Item1;
            queries.Add(new Rule(new Predicate("query", new List<Term>()), body.Item2, body.Item3));

            int i = 0;
            while (true)
            {
                if (s.Length == 0)
                {
                    break;
                }

                s = space(s);

                if (!s.StartsWith("or"))
                {
                    break;
                }
                s = s.Substring(2);

                Either<Error, Tuple<string, List<Predicate>, List<Expression>>> bodyRes2 = rule_body(s);
                if (bodyRes2.IsLeft)
                {
                    return new Left(bodyRes2.Left);
                }

                Tuple<string, List<Predicate>, List<Expression>> body2 = bodyRes2.Right;

                s = body2.Item1;
                queries.Add(new Rule(new Predicate("query", new List<Term>()), body2.Item2, body2.Item3));
            }

            return new Right(new Tuple<string, List<Rule>>(s, queries));
        }

        public static Either<Error, Tuple<string, List<Predicate>, List<Expression>>> rule_body(string s)
        {
            List<Predicate> predicates = new List<Predicate>();
            List<Expression> expressions = new List<Expression>();

            while (true)
            {
                s = space(s);

                Either<Error, Tuple<string, Predicate>> res = predicate(s);
                if (res.IsRight)
                {
                    Tuple<string, Predicate> t = res.Right;
                    s = t.Item1;
                    predicates.Add(t.Item2);
                }
                else
                {
                    Either<Error, Tuple<string, Expression>> res2 = expression(s);
                    if (res2.IsRight)
                    {
                        Tuple<string, Expression> t2 = res2.Right;
                        s = t2.Item1;
                        expressions.Add(t2.Item2);
                    }
                    else
                    {
                        break;
                    }
                }

                s = space(s);

                if (s.Length == 0 || s[0] != ',')
                {
                    break;
                }
                else
                {
                    s = s.Substring(1);
                }
            }

            //FIXME: handle constraints

            return new Right(new Tuple<string, List<Predicate>, List<Expression>>(s, predicates, expressions));
        }

        public static Either<Error, Tuple<string, Predicate>> predicate(string s)
        {
            Tuple<string, string> tn = take_while(s, c => char.IsLetter(c) || c == '_');
            string name = tn.Item1;
            s = tn.Item2;

            s = space(s);
            if (s.Length == 0 || s[0] != '(')
            {
                return new Left(new Error(s, "opening parens not found"));
            }
            s = s.Substring(1);

            List<Term> terms = new List<Term>();
            while (true)
            {

                s = space(s);

                Either<Error, Tuple<string, Term>> res = term(s);
                if (res.IsLeft)
                {
                    break;
                }

                Tuple<string, Term> t = res.Right;
                s = t.Item1;
                terms.Add(t.Item2);

                s = space(s);

                if (s.Length == 0 || s[0] != ',')
                {
                    break;
                }
                else
                {
                    s = s.Substring(1);
                }
            }

            s = space(s);
            if (0 == s.Length || s[0] != ')')
            {
                return new Left(new Error(s, "closing parens not found"));
            }
            string remaining = s.Substring(1);

            return new Right(new Tuple<string, Predicate>(remaining, new Predicate(name, terms)));
        }

        public static Either<Error, Tuple<string, Predicate>> fact_predicate(string s)
        {
            Tuple<string, string> tn = take_while(s, c => char.IsLetter(c) || c == '_');
            string name = tn.Item1;
            s = tn.Item2;

            s = space(s);
            if (s.Length == 0 || s[0] != '(')
            {
                return new Left(new Error(s, "opening parens not found"));
            }
            s = s.Substring(1);

            List<Term> terms = new List<Term>();
            while (true)
            {

                s = space(s);

                Either<Error, Tuple<string, Term>> res = fact_term(s);
                if (res.IsLeft)
                {
                    break;
                }

                Tuple<string, Term> t = res.Right;
                s = t.Item1;
                terms.Add(t.Item2);

                s = space(s);

                if (s.Length == 0 || s[0] != ',')
                {
                    break;
                }
                else
                {
                    s = s.Substring(1);
                }
            }

            s = space(s);
            if (0 == s.Length || s[0] != ')')
            {
                return new Left(new Error(s, "closing parens not found"));
            }
            string remaining = s.Substring(1);

            return new Right(new Tuple<string, Predicate>(remaining, new Predicate(name, terms)));
        }

        public static Either<Error, Tuple<string, string>> name(string s)
        {
            Tuple<string, string> t = take_while(s, (c)=> char.IsLetter(c) || c == '_');
            string name = t.Item1;
            string remaining = t.Item2;

            return new Right(new Tuple<string, string>(remaining, name));
        }

        public static Either<Error, Tuple<string, Term>> term(string s)
        {
            Either<Error, Tuple<string, Term.Symbol>> res1 = symbol(s);
            if (res1.IsRight)
            {
                Tuple<string, Term.Symbol> t = res1.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Variable>> res5 = variable(s);
            if (res5.IsRight)
            {
                Tuple<string, Term.Variable> t = res5.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Str>> res2 = strings(s);
            if (res2.IsRight)
            {
                Tuple<string, Term.Str> t = res2.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Set>> res7 = set(s);
            if (res7.IsRight)
            {
                Tuple<string, Term.Set> t = res7.Right;
                return new Tuple<string, Term>(t.Item1, t.Item2);
            }

            Either<Error, Tuple<string, Term.Bool>> res6 = boolean(s);
            if (res6.IsRight)
            {
                Tuple<string, Term.Bool> t = res6.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Date>> res4 = date(s);
            if (res4.IsRight)
            {
                Tuple<string, Term.Date> t = res4.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Integer>> res3 = integer(s);
            if (res3.IsRight)
            {
                Tuple<string, Term.Integer> t = res3.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            return new Left(new Error(s, "unrecognized value"));
        }

        public static Either<Error, Tuple<string, Term>> fact_term(string s)
        {
            if (s.Length > 0 && s[0] == '$')
            {
                return new Left(new Error(s, "variables are not allowed in facts"));
            }

            Either<Error, Tuple<string, Term.Symbol>> res1 = symbol(s);
            if (res1.IsRight)
            {
                Tuple<string, Term.Symbol> t = res1.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Str>> res2 = strings(s);
            if (res2.IsRight)
            {
                Tuple<string, Term.Str> t = res2.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Set>> res7 = set(s);
            if (res7.IsRight)
            {
                Tuple<string, Term.Set> t = res7.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Bool>> res6 = boolean(s);
            if (res6.IsRight)
            {
                Tuple<string, Term.Bool> t = res6.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Date>> res4 = date(s);
            if (res4.IsRight)
            {
                Tuple<string, Term.Date> t = res4.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }

            Either<Error, Tuple<string, Term.Integer>> res3 = integer(s);
            if (res3.IsRight)
            {
                Tuple<string, Term.Integer> t = res3.Right;
                return new Right(new Tuple<string, Term>(t.Item1, t.Item2));
            }


            return new Left(new Error(s, "unrecognized value"));
        }

        public static Either<Error, Tuple<string, Term.Symbol>> symbol(string s)
        {
            if (s[0] != '#')
            {
                return new Left(new Error(s, "not a symbol"));
            }

            Tuple<string, string> t = take_while(s.Substring(1), c => char.IsLetterOrDigit(c) || c == '_');
            string name = t.Item1;
            string remaining = t.Item2;

            return new Right(new Tuple<string, Term.Symbol>(remaining, (Term.Symbol)Utils.s(name)));
        }

        public static Either<Error, Tuple<string, Term.Str>> strings(string s)
        {
            if (s[0] != '"')
            {
                return new Left(new Error(s, "not a string"));
            }

            int index = s.Length;
            for (int i = 1; i < s.Length; i++)
            {
                char c = s[i];

                if (c == '\\' && s[i + 1] == '"')
                {
                    i += 1;
                    continue;
                }

                if (c == '"')
                {
                    index = i - 1;
                    break;
                }
            }

            if (index == s.Length)
            {
                return new Left(new Error(s, "end of string not found"));
            }

            if (s[index + 1] != '"')
            {
                return new Left(new Error(s, "ending double quote not found"));
            }

            //Be careful s.Substring(int,int) is différent between java and cs
            // java : takes two indexes
            // CS : takes index and length
            string substring = s.Substring(1, index);
            string remaining = s.Substring(index + 2);

            return new Right(new Tuple<string, Term.Str>(remaining, (Term.Str)Utils.strings(substring)));
        }

        public static Either<Error, Tuple<string, Term.Integer>> integer(string s)
        {
            int index = 0;
            if (s[0] == '-')
            {
                index += 1;
            }

            int index2 = s.Length;
            for (int i = index; i < s.Length; i++)
            {
                char c = s[i];

                if (!char.IsDigit(c))
                {
                    index2 = i;
                    break;
                }
            }

            if (index2 == 0)
            {
                return new Left(new Error(s, "not an integer"));
            }
            
            long j = long.Parse(s.Substring(0, index2));
            string remaining = s.Substring(index2);


            return new Right(new Tuple<string, Term.Integer>(remaining, (Term.Integer)Utils.integer(j)));
        }

        public static Either<Error, Tuple<string, Term.Date>> date(string s)
        {
            Tuple<string, string> t = take_while(s, (c)=>c != ' ' && c != ',' && c != ')');

            try
            {
                DateTimeOffset d = DateTimeOffset.Parse(t.Item1);
                
                string remaining = t.Item2;
                return new Right(new Tuple<string, Term.Date>(remaining, new Term.Date((ulong)d.ToUnixTimeSeconds())));
            }
            catch (FormatException)
            {
                return new Left(new Error(s, "not a date"));

            }
        }

        public static Either<Error, Tuple<string, Term.Variable>> variable(string s)
        {
            if (s[0] != '$')
            {
                return new Left(new Error(s, "not a variable"));
            }

            Tuple<string, string> t = take_while(s.Substring(1), (c)=>char.IsLetterOrDigit(c) || c == '_');

            return new Right(new Tuple<string, Term.Variable>(t.Item2, (Term.Variable)Utils.var(t.Item1)));
        }

        public static Either<Error, Tuple<string, Term.Bool>> boolean(string s)
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
                return new Left(new Error(s, "not a boolean"));
            }

            return new Right(new Tuple<string, Term.Bool>(s, new Term.Bool(b)));
        }

        public static Either<Error, Tuple<string, Term.Set>> set(string s)
        {
            if (s.Length == 0 || s[0] != '[')
            {
                return new Left(new Error(s, "not a set"));
            }

            s = s.Substring(1);

            HashSet<Term> terms = new HashSet<Term>();
            while (true)
            {

                s = space(s);

                Either<Error, Tuple<string, Term>> res = fact_term(s);
                if (res.IsLeft)
                {
                    break;
                }

                Tuple<string, Term> t = res.Right;

                if (t.Item2 is Term.Variable) {
                    return new Left(new Error(s, "sets cannot contain variables"));
                }

                s = t.Item1;
                terms.Add(t.Item2);

                s = space(s);

                if (s.Length == 0 || s[0] != ',')
                {
                    break;
                }
                else
                {
                    s = s.Substring(1);
                }
            }

            s = space(s);
            if (0 == s.Length || s[0] != ']')
            {
                return new Left(new Error(s, "closing square bracket not found"));
            }

            string remaining = s.Substring(1);

            return new Right(new Tuple<string, Term.Set>(remaining, new Term.Set(terms)));
        }

        public static Either<Error, Tuple<string, Expression>> expression(string s)
        {
            return ExpressionParser.parse(s);
        }

        public static string space(string s)
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

        public static Tuple<string, string> take_while(string s, Func<char, bool> f)
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
