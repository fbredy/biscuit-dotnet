using System;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class Fact
    {
        public Predicate predicate { get; }

        public bool match_predicate(Predicate predicate)
        {
            return this.predicate.Match(predicate);
        }

        public Fact(Predicate predicate)
        {
            this.predicate = predicate;
        }
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            Fact fact = (Fact)obj;
            return Object.Equals(predicate,fact.predicate);
        }

        public override int GetHashCode()
        {
            return predicate.GetHashCode();
        }

        public override string ToString()
        {
            return predicate.ToString();
        }

        public Format.Schema.FactV1 serialize()
        {
            return new Format.Schema.FactV1
            {
                Predicate = this.predicate.Serialize()
            };
        }

        static public Either<Errors.FormatError, Fact> deserializeV0(Format.Schema.FactV0 fact)
        {
            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV0(fact.Predicate);
            if (res.IsLeft)
            {
                Errors.FormatError e = res.Left;
                return new Left(e);
            }
            else
            {
                return new Right(new Fact(res.Right));
            }
        }

        static public Either<Errors.FormatError, Fact> deserializeV1(Format.Schema.FactV1 fact)
        {
            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV1(fact.Predicate);
            if (res.IsLeft)
            {
                Errors.FormatError e = res.Left;
                return new Left (e);
            }
            else
            {
                return new Right(new Fact(res.Right));
            }
        }
    }
}
