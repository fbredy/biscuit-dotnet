using Biscuit.Datalog.Expressions;
using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace Biscuit.Datalog.Constraints
{
    public sealed class Constraint
    {
        static public Either<Errors.FormatError, Expression> deserializeV0(Format.Schema.ConstraintV0 c)
        {
            List<Op> ops = new List<Op>();

            long id = c.Id;
            ops.Add(new Op.Value(new ID.Variable(id)));

            switch (c.Kind)
            {
                case Format.Schema.ConstraintV0.Types.Kind.Int:
                    if (c.Int != null)
                    {
                        Biscuit.Format.Schema.IntConstraintV0 ic = c.Int;

                        switch (ic.Kind)
                        {
                            case Format.Schema.IntConstraintV0.Types.Kind.Lower:
                                ops.Add(new Op.Value(new ID.Integer(ic.Lower)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessThan));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.Larger:
                                ops.Add(new Op.Value(new ID.Integer(ic.Larger)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterThan));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.LowerOrEqual:
                                ops.Add(new Op.Value(new ID.Integer(ic.LowerOrEqual)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessOrEqual));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.LargerOrEqual:
                                ops.Add(new Op.Value(new ID.Integer(ic.LargerOrEqual)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterOrEqual));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Integer(ic.Equal)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (long l in ic.InSet)
                                {
                                    set.Add(new ID.Integer(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Right(new Expression(ops));
                            case Format.Schema.IntConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (long l in ic.NotInSet)
                                {
                                    set2.Add(new ID.Integer(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Right(new Expression(ops));
                        }
                    }
                    return new Left(new Errors.DeserializationError("invalid Int constraint"));
                case Format.Schema.ConstraintV0.Types.Kind.Date:
                    if (c.Date == null)
                    {
                        return new Left(new Errors.DeserializationError("invalid Date constraint"));
                    }
                    else
                    {
                        Format.Schema.DateConstraintV0 ic = c.Date;

                        switch (ic.Kind)
                        {
                            case Format.Schema.DateConstraintV0.Types.Kind.Before:
                                ops.Add(new Op.Value(new ID.Date(ic.Before)));
                                ops.Add(new Op.Binary(Op.BinaryOp.LessOrEqual));
                                return new Right(new Expression(ops));
                            case Format.Schema.DateConstraintV0.Types.Kind.After:
                                ops.Add(new Op.Value(new ID.Date(ic.Before)));
                                ops.Add(new Op.Binary(Op.BinaryOp.GreaterOrEqual));
                                return new Right(new Expression(ops));
                        }
                        return new Left(new Errors.DeserializationError("invalid Int constraint"));
                    }
                case Format.Schema.ConstraintV0.Types.Kind.Bytes:
                    if (c.Bytes!=null)
                    {
                        Format.Schema.BytesConstraintV0 ic = c.Bytes;

                        switch (ic.Kind)
                        {
                            case Format.Schema.BytesConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Bytes(ic.Equal.ToByteArray())));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Right(new Expression(ops));
                            case Format.Schema.BytesConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (ByteString l in ic.InSet)
                                {
                                    set.Add(new ID.Bytes(l.ToByteArray()));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Right(new Expression(ops));
                            case Format.Schema.BytesConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (ByteString l in ic.NotInSet)
                                {
                                    set2.Add(new ID.Bytes(l.ToByteArray()));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Right(new Expression(ops));
                        }

                    }
                    return new Left(new Errors.DeserializationError("invalid Bytes constraint"));
                case Format.Schema.ConstraintV0.Types.Kind.String:
                    if (c.Str != null)
                    {
                        Format.Schema.StringConstraintV0 ic = c.Str;

                        switch (ic.Kind)
                        {
                            case Format.Schema.StringConstraintV0.Types.Kind.Equal:
                                ops.Add(new Op.Value(new ID.Str(ic.Equal)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Equal));
                                return new Right(new Expression(ops));
                            case Format.Schema.StringConstraintV0.Types.Kind.Prefix:
                                ops.Add(new Op.Value(new ID.Str(ic.Prefix)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Prefix));
                                return new Right(new Expression(ops));
                            case Format.Schema.StringConstraintV0.Types.Kind.Suffix:
                                ops.Add(new Op.Value(new ID.Str(ic.Suffix)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Suffix));
                                return new Right(new Expression(ops));
                            case Format.Schema.StringConstraintV0.Types.Kind.Regex:
                                ops.Add(new Op.Value(new ID.Str(ic.Regex)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Regex));
                                return new Right(new Expression(ops));
                            case Format.Schema.StringConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (string l in ic.InSet)
                                {
                                    set.Add(new ID.Str(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Right(new Expression(ops));
                            case Format.Schema.StringConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (string l in ic.NotInSet)
                                {
                                    set2.Add(new ID.Str(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Right(new Expression(ops));
                        }
                    }
                    return new Left(new Errors.DeserializationError("invalid String constraint"));
                case Format.Schema.ConstraintV0.Types.Kind.Symbol:
                    if (c.Symbol != null)
                    {
                        Format.Schema.SymbolConstraintV0 ic = c.Symbol;

                        switch (ic.Kind)
                        {
                            case Format.Schema.SymbolConstraintV0.Types.Kind.In:
                                HashSet<ID> set = new HashSet<ID>();
                                foreach (ulong l in ic.InSet)
                                {
                                    set.Add(new ID.Symbol(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                return new Right(new Expression(ops));
                            case Format.Schema.SymbolConstraintV0.Types.Kind.NotIn:
                                HashSet<ID> set2 = new HashSet<ID>();
                                foreach (ulong l in ic.NotInSet)
                                {
                                    set2.Add(new ID.Symbol(l));
                                }
                                ops.Add(new Op.Value(new ID.Set(set2)));
                                ops.Add(new Op.Binary(Op.BinaryOp.Contains));
                                ops.Add(new Op.Unary(Op.UnaryOp.Negate));
                                return new Right(new Expression(ops));
                        }
                    }
                    return new Left(new Errors.DeserializationError("invalid Symbol constraint"));
            }
            return new Left(new Errors.DeserializationError("invalid constraint kind"));
        }
    }
}
