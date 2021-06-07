using Biscuit.Datalog.Expressions;
using Biscuit.Errors;
using Google.Protobuf;
using System.Collections.Generic;

namespace Biscuit.Datalog.Constraints
{
    public sealed class Constraint
    {
        static public Either<FormatError, Expression> DeserializeV0(Format.Schema.ConstraintV0 c)
        {
            List<Op> ops = new List<Op>();

            long id = c.Id;
            ops.Add(new Op.Value(new ID.Variable(id)));

            switch (c.Kind)
            {
                case Format.Schema.ConstraintV0.Types.Kind.Int:
                    if (c.Int != null)
                    {
                        Format.Schema.IntConstraintV0 constraint = c.Int;

                        switch (constraint.Kind)
                        {
                            case Format.Schema.IntConstraintV0.Types.Kind.Lower:
                                ops.Add(new Op.Value(new ID.Integer(constraint.Lower)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessThan));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.Larger:
                                ops.Add(new Op.Value(new ID.Integer(constraint.Larger)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterThan));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.LowerOrEqual:
                                ops.Add(new Op.Value(new ID.Integer(constraint.LowerOrEqual)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessOrEqual));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.LargerOrEqual:
                                ops.Add(new Op.Value(new ID.Integer(constraint.LargerOrEqual)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterOrEqual));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Integer(constraint.Equal)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (long item in constraint.InSet)
                                {
                                    set.Add(new ID.Integer(item));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Expression(ops);
                            case Format.Schema.IntConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (long item in constraint.NotInSet)
                                {
                                    set2.Add(new ID.Integer(item));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Expression(ops);
                        }
                    }
                    return new DeserializationError("invalid Int constraint");
                case Format.Schema.ConstraintV0.Types.Kind.Date:
                    if (c.Date == null)
                    {
                        return new DeserializationError("invalid Date constraint");
                    }
                    else
                    {
                        Format.Schema.DateConstraintV0 constraint = c.Date;

                        switch (constraint.Kind)
                        {
                            case Format.Schema.DateConstraintV0.Types.Kind.Before:
                                ops.Add(new Op.Value(new ID.Date(constraint.Before)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessOrEqual));
                                return new Expression(ops);
                            case Format.Schema.DateConstraintV0.Types.Kind.After:
                                ops.Add(new Op.Value(new ID.Date(constraint.Before)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterOrEqual));
                                return new Expression(ops);
                        }
                        return new DeserializationError("invalid Int constraint");
                    }
                case Format.Schema.ConstraintV0.Types.Kind.Bytes:
                    if (c.Bytes != null)
                    {
                        Format.Schema.BytesConstraintV0 constraint = c.Bytes;

                        switch (constraint.Kind)
                        {
                            case Format.Schema.BytesConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Bytes(constraint.Equal.ToByteArray())));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Expression(ops);
                            case Format.Schema.BytesConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (ByteString l in constraint.InSet)
                                {
                                    set.Add(new ID.Bytes(l.ToByteArray()));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Expression(ops);
                            case Format.Schema.BytesConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (ByteString l in constraint.NotInSet)
                                {
                                    set2.Add(new ID.Bytes(l.ToByteArray()));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Expression(ops);
                        }

                    }
                    return new DeserializationError("invalid Bytes constraint");
                case Format.Schema.ConstraintV0.Types.Kind.String:
                    if (c.Str != null)
                    {
                        Format.Schema.StringConstraintV0 constraint = c.Str;

                        switch (constraint.Kind)
                        {
                            case Format.Schema.StringConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Str(constraint.Equal)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Expression(ops);
                            case Format.Schema.StringConstraintV0.Types.Kind.Prefix:
                                ops.Add(new Op.Value(new ID.Str(constraint.Prefix)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Prefix));
                                return new Expression(ops);
                            case Format.Schema.StringConstraintV0.Types.Kind.Suffix:
                                ops.Add(new Op.Value(new ID.Str(constraint.Suffix)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Suffix));
                                return new Expression(ops);
                            case Format.Schema.StringConstraintV0.Types.Kind.Regex:
                                ops.Add(new Op.Value(new ID.Str(constraint.Regex)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Regex));
                                return new Expression(ops);
                            case Format.Schema.StringConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (string l in constraint.InSet)
                                {
                                    set.Add(new ID.Str(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Expression(ops);
                            case Format.Schema.StringConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (string l in constraint.NotInSet)
                                {
                                    set2.Add(new ID.Str(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Expression(ops);
                        }
                    }
                    return new DeserializationError("invalid String constraint");
                case Format.Schema.ConstraintV0.Types.Kind.Symbol:
                    if (c.Symbol != null)
                    {
                        Format.Schema.SymbolConstraintV0 constraint = c.Symbol;

                        switch (constraint.Kind)
                        {
                            case Format.Schema.SymbolConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (ulong l in constraint.InSet)
                                {
                                    set.Add(new ID.Symbol(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Expression(ops);
                            case Format.Schema.SymbolConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (ulong item in constraint.NotInSet)
                                {
                                    set2.Add(new ID.Symbol(item));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Expression(ops);
                        }
                    }
                    return new DeserializationError("invalid Symbol constraint");
            }
            return new DeserializationError("invalid constraint kind");
        }
    }
}
