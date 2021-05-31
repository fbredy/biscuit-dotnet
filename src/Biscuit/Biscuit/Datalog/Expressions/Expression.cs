using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog.Expressions
{
    public class Expression
    {
        private readonly List<Op> ops;

        public Expression(List<Op> ops)
        {
            this.ops = ops;
        }

        public List<Op> GetOps()
        {
            return ops;
        }

        public Option<ID> Evaluate(Dictionary<ulong, ID> variables)
        {
            Stack<ID> stack = new Stack<ID>(16); //Default value
            foreach (Op op in ops)
            {
                if (!op.Evaluate(stack, variables))
                {
                    return Option<ID>.None();
                }
            }
            if (stack.Count == 1)
            {
                return Option<ID>.Some(stack.Pop());
            }
            else
            {
                return Option<ID>.None();
            }
        }

        public Option<string> Print(SymbolTable symbols)
        {
            Stack<string> stack = new Stack<string>();
            foreach (Op op in ops)
            {
                op.Print(stack, symbols);
            }
            if (stack.Count == 1)
            {
                return Option<string>.Some(stack.Pop());
            }
            else
            {
                return Option<string>.None();
            }
        }

        public Format.Schema.ExpressionV1 Serialize()
        {
            Format.Schema.ExpressionV1  expression = new Format.Schema.ExpressionV1();
            
            var serializedOps = this.ops.Select(op => op.Serialize());
            expression.Ops.AddRange(serializedOps);

            return expression;
        }

        static public Either<Errors.FormatError, Expression> DeserializeV1(Format.Schema.ExpressionV1 expression)
        {
            List<Op> ops = new List<Op>();

            foreach (Format.Schema.Op op in expression.Ops)
            {
                Either<Errors.FormatError, Op> deserialized = Op.DeserializeV1(op);

                if (deserialized.IsLeft)
                {
                    return deserialized.Left;
                }
                else
                {
                    ops.Add(deserialized.Right);
                }
            }

            return new Expression(ops);
        }
    }
}
