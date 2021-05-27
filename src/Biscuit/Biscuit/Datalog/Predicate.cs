using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class Predicate
    {

        public ulong name
        {
            get;
        }

        public List<ID> ids
        {
            get;
        }

        public IEnumerator<ID> ids_iterator()
        {
            return this.ids.GetEnumerator();
        }

        public bool match(Predicate other)
        {
            if (this.name != other.name)
            {
                return false;
            }
            if (this.ids.Count != other.ids.Count)
            {
                return false;
            }
            for (int i = 0; i < this.ids.Count; ++i)
            {
                if (!this.ids[i].match(other.ids[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public Predicate clone()
        {
            List<ID> ids = new List<ID>();
            ids.AddRange(this.ids);
            return new Predicate(this.name, ids);
        }

        public Predicate(ulong name, List<ID> ids)
        {
            this.name = name;
            this.ids = ids;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            Predicate predicate = (Predicate)obj;
            return name == predicate.name && ids.SequenceEqual(predicate.ids); 
        }

        public override int GetHashCode()
        {
            return 31 * name.GetHashCode() + ids.GetSequenceHashCode();
            //return Objects.hash(name, ids);
        }

        public override string ToString()
        {
            return this.name + "(" + string.Join(", ", this.ids.Select((i)=> (i == null) ? "(null)" : i.ToString()).ToList()) + ")";
        }


        public Format.Schema.PredicateV1 serialize()
        {
            Format.Schema.PredicateV1 predicate = new Format.Schema.PredicateV1()
            {
                Name = this.name
            };

            for (int i = 0; i < this.ids.Count; i++)
            {
                predicate.Ids.Add(this.ids[i].serialize());
            }

            return predicate;
        }

        static public Either<Errors.FormatError, Predicate> deserializeV0(Format.Schema.PredicateV0 predicate)
        {
            List<ID> ids = new List<ID>();
            foreach (Format.Schema.IDV0 id in predicate.Ids)
            {
                Either<Errors.FormatError, ID> res = ID.deserialize_enumV0(id);
                if (res.IsLeft)
                {
                    Errors.FormatError e = res.Left;
                    return new Left(e);
                }
                else
                {
                    ids.Add(res.Right);
                }
            }

            return new Right(new Predicate(predicate.Name, ids));
        }

        static public Either<Errors.FormatError, Predicate> deserializeV1(Format.Schema.PredicateV1 predicate)
        {
            List<ID> ids = new List<ID>();
            foreach (Format.Schema.IDV1 id in predicate.Ids)
            {
                Either<Errors.FormatError, ID> res = ID.deserialize_enumV1(id);
                if (res.IsLeft)
                {
                    Errors.FormatError e = res.Left;
                    return new Left(e);
                }
                else
                {
                    ids.Add(res.Get());
                }
            }

            return new Right(new Predicate(predicate.Name, ids));
        }
    }
}
