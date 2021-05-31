using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Token.Builder
{
    public class RuleBuilder
    {
        public PredicateBuilder Head { get; }
        public List<PredicateBuilder> Body { get; }
        public List<ExpressionBuilder> Expressions { get; }

        public RuleBuilder(PredicateBuilder head, List<PredicateBuilder> body, List<ExpressionBuilder> expressions)
        {
            this.Head = head;
            this.Body = body;
            this.Expressions = expressions;
        }

        public Datalog.Rule Convert(Datalog.SymbolTable symbols)
        {
            Datalog.Predicate head = this.Head.Convert(symbols);
            List<Datalog.Predicate> body = new List<Datalog.Predicate>();
            List<Datalog.Expressions.Expression> expressions = new List<Datalog.Expressions.Expression>();

            foreach (var p in this.Body)
            {
                body.Add(p.Convert(symbols));
            }

            foreach (var e in this.Expressions)
            {
                expressions.Add(e.Convert(symbols));
            }

            return new Datalog.Rule(head, body, expressions);
        }

        public static RuleBuilder ConvertFrom(Datalog.Rule r, Datalog.SymbolTable symbols)
        {
            PredicateBuilder head = PredicateBuilder.ConvertFrom(r.Head, symbols);

            List<PredicateBuilder> body = new List<PredicateBuilder>();
            List<ExpressionBuilder> expressions = new List<ExpressionBuilder>();

            foreach (Datalog.Predicate p in r.Body)
            {
                body.Add(PredicateBuilder.ConvertFrom(p, symbols));
            }

            foreach (Datalog.Expressions.Expression e in r.Expressions)
            {
                expressions.Add(ExpressionBuilder.ConvertFrom(e, symbols));
            }

            return new RuleBuilder(head, body, expressions);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is RuleBuilder)) return false;

            RuleBuilder rule = (RuleBuilder)o;

            if (Head != null ? !Head.Equals(rule.Head) : rule.Head != null) return false;
            if (Body != null ? !Body.SequenceEqual(rule.Body) : rule.Body != null) return false;
            return Expressions != null ? Expressions.SequenceEqual(rule.Expressions) : rule.Expressions == null;
        }

        public override int GetHashCode()
        {
            int result = Head != null ? Head.GetHashCode() : 0;
            result = 31 * result + (Body != null ? Body.GetHashCode() : 0);
            result = 31 * result + (Expressions != null ? Expressions.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            var b = Body.Select((pred)=>pred.ToString());
            string res = Head.ToString() + " <- " + string.Join(", ", b);

            if (!Expressions.IsEmpty())
            {
                var e = Expressions.Select((expression) => expression.ToString());
                res += ", " + string.Join(", ", e);
            }

            return res;
        }
    }
}
