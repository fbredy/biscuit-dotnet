using System.Collections.Generic;

namespace Biscuit.Datalog.Expressions
{
    public class Expression
    {
        private List<Op> ops;

        public Expression(List<Op> ops)
        {
            this.ops = ops;
        }

        public List<Op> getOps()
        {
            return ops;
        }

        public Option<ID> evaluate(Dictionary<ulong, ID> variables)
        {
            Stack<ID> stack = new Stack<ID>(16); //Default value
            foreach (Op op in ops)
            {
                if (!op.evaluate(stack, variables))
                {
                    return Option<ID>.none();
                }
            }
            if (stack.Count == 1)
            {
                return Option<ID>.some(stack.Pop());
            }
            else
            {
                return Option<ID>.none();
            }
        }

        public Option<string> print(SymbolTable symbols)
        {
            Stack<string> stack = new Stack<string>();
            foreach (Op op in ops)
            {
                op.print(stack, symbols);
            }
            if (stack.Count == 1)
            {
                return Option<string>.some(stack.Pop());
            }
            else
            {
                return Option<string>.none();
            }
        }

        public Format.Schema.ExpressionV1 serialize()
        {
            Format.Schema.ExpressionV1  b = new Format.Schema.ExpressionV1();
            
            foreach (Op op in this.ops)
            {
                b.Ops.Add(op.serialize());
            }

            return b;
        }

        static public Either<Errors.FormatError, Expression> deserializeV1(Format.Schema.ExpressionV1 e)
        {
            List<Op> ops = new List<Op>();

            foreach (Format.Schema.Op op in e.Ops)
            {
                Either<Errors.FormatError, Op> res = Op.deserializeV1(op);

                if (res.IsLeft)
                {
                    Errors.FormatError err = res.Left;
                    return new Left(err);
                }
                else
                {
                    ops.Add(res.Right);
                }
            }

            return new Right(new Expression(ops));
        }
    }
}
