using System;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class Fact
    {
        public Predicate Predicate { get; }

        public bool MatchPredicate(Predicate predicate)
        {
            return this.Predicate.Match(predicate);
        }

        public Fact(Predicate predicate)
        {
            this.Predicate = predicate;
        }
        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            Fact fact = (Fact)obj;
            return Equals(Predicate, fact.Predicate);
        }

        public override int GetHashCode()
        {
            return Predicate.GetHashCode();
        }

        public override string ToString()
        {
            return Predicate.ToString();
        }

        public Format.Schema.FactV1 Serialize()
        {
            return new Format.Schema.FactV1
            {
                Predicate = this.Predicate.Serialize()
            };
        }

        static public Either<Errors.FormatError, Fact> DeserializeV0(Format.Schema.FactV0 fact)
        {
            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV0(fact.Predicate);
            if (res.IsLeft)
            {
                return res.Left;
            }
            else
            {
                return new Fact(res.Right);
            }
        }

        static public Either<Errors.FormatError, Fact> DeserializeV1(Format.Schema.FactV1 fact)
        {
            Either<Errors.FormatError, Predicate> res = Predicate.DeserializeV1(fact.Predicate);
            if (res.IsLeft)
            {
                return res.Left;
            }
            else
            {
                return new Fact(res.Right);
            }
        }
    }
}
