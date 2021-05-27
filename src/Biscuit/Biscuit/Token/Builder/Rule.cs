using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class Rule
    {
        public Predicate head { get; }
        public List<Predicate> body { get; }
        public List<Expression> expressions { get; }

        public Rule(Predicate head, List<Predicate> body, List<Expression> expressions)
        {
            this.head = head;
            this.body = body;
            this.expressions = expressions;
        }

        public Datalog.Rule convert(Datalog.SymbolTable symbols)
        {
            Datalog.Predicate head = this.head.convert(symbols);
            List<Datalog.Predicate> body = new List<Datalog.Predicate>();
            List<Datalog.Expressions.Expression> expressions = new List<Datalog.Expressions.Expression>();

            foreach (var p in this.body)
            {
                body.Add(p.convert(symbols));
            }

            foreach (var e in this.expressions)
            {
                expressions.Add(e.convert(symbols));
            }

            return new Datalog.Rule(head, body, expressions);
        }

        public static Rule convert_from(Datalog.Rule r, Datalog.SymbolTable symbols)
        {
            Predicate head = Predicate.convert_from(r.head, symbols);

            List<Predicate> body = new List<Predicate>();
            List<Expression> expressions = new List<Expression>();

            foreach (Datalog.Predicate p in r.body)
            {
                body.Add(Predicate.convert_from(p, symbols));
            }

            foreach (Datalog.Expressions.Expression e in r.expressions)
            {
                expressions.Add(Expression.convert_from(e, symbols));
            }

            return new Rule(head, body, expressions);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is Rule)) return false;

            Rule rule = (Rule)o;

            if (head != null ? !head.Equals(rule.head) : rule.head != null) return false;
            if (body != null ? !body.SequenceEqual(rule.body) : rule.body != null) return false;
            return expressions != null ? expressions.SequenceEqual(rule.expressions) : rule.expressions == null;
        }

        public override int GetHashCode()
        {
            int result = head != null ? head.GetHashCode() : 0;
            result = 31 * result + (body != null ? body.GetHashCode() : 0);
            result = 31 * result + (expressions != null ? expressions.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            var b = body.Select((pred)=>pred.ToString());
            string res = head.ToString() + " <- " + string.Join(", ", b);

            if (!expressions.isEmpty())
            {
                var e = expressions.Select((expression) => expression.ToString());
                res += ", " + string.Join(", ", e);
            }

            return res;
        }
    }
}
