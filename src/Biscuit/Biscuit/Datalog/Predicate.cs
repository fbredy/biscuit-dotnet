using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class Predicate
    {
        public ulong Name
        {
            get;
        }

        public IList<ID> Ids
        {
            get;
        }

        public IEnumerator<ID> IdsEnumerator()
        {
            return this.Ids.GetEnumerator();
        }

        public bool Match(Predicate other)
        {
            if (this.Name != other.Name)
            {
                return false;
            }
            if (this.Ids.Count != other.Ids.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Ids.Count; ++i)
            {
                if (!this.Ids[i].Match(other.Ids[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public Predicate Clone()
        {
            List<ID> ids = new List<ID>(this.Ids);
            return new Predicate(this.Name, ids);
        }

        public Predicate(ulong name, IList<ID> ids)
        {
            this.Name = name;
            this.Ids = ids;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            Predicate predicate = (Predicate)obj;
            return Name == predicate.Name && Ids.SequenceEqual(predicate.Ids);
        }

        public override int GetHashCode()
        {
            return 31 * Name.GetHashCode() + Ids.GetSequenceHashCode();
        }

        public override string ToString()
        {
            return this.Name + "(" + string.Join(", ", this.Ids.Select((i) => (i == null) ? "(null)" : i.ToString()).ToList()) + ")";
        }

        public Format.Schema.PredicateV1 Serialize()
        {
            Format.Schema.PredicateV1 predicate = new Format.Schema.PredicateV1()
            {
                Name = this.Name
            };

            var serializedIds = this.Ids.Select(i => i.Serialize());
            predicate.Ids.AddRange(serializedIds);

            return predicate;
        }

        static public Either<Errors.FormatError, Predicate> DeserializeV0(Format.Schema.PredicateV0 predicate)
        {
            IList<ID> ids = new List<ID>();
            foreach (Format.Schema.IDV0 id in predicate.Ids)
            {
                Either<Errors.FormatError, ID> res = ID.DeserializeEnumV0(id);
                if (res.IsLeft)
                {
                    return res.Left;
                }
                else
                {
                    ids.Add(res.Right);
                }
            }

            return new Predicate(predicate.Name, ids);
        }

        static public Either<Errors.FormatError, Predicate> DeserializeV1(Format.Schema.PredicateV1 predicate)
        {
            IList<ID> ids = new List<ID>();
            foreach (Format.Schema.IDV1 id in predicate.Ids)
            {
                Either<Errors.FormatError, ID> res = ID.DeserializeEnumV1(id);
                if (res.IsLeft)
                {
                    return res.Left;
                }
                else
                {
                    ids.Add(res.Get());
                }
            }

            return new Predicate(predicate.Name, ids);
        }
    }
}
