using System.Collections.Generic;
using System.Linq;

namespace Biscuit.Datalog
{
    public class Check
    {
        public Check(IList<Rule> queries)
        {
            this.Queries = queries;
        }

        public IList<Rule> Queries { get; }

        public override int GetHashCode()
        {
            return Queries.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public Format.Schema.CheckV1 Serialize()
        {
            Format.Schema.CheckV1 check = new Format.Schema.CheckV1();

            var querySerialized = this.Queries.Select(q => q.Serialize());
            check.Queries.AddRange(querySerialized);

            return check;
        }

        static public Either<Errors.FormatError, Check> DeserializeV0(Format.Schema.CaveatV0 caveat)
        {
            IList<Rule> queries = new List<Rule>();

            foreach (Format.Schema.RuleV0 query in caveat.Queries)
            {
                Either<Errors.FormatError, Rule> deserializedRule = Rule.DeserializeV0(query);
                if (deserializedRule.IsLeft)
                {
                    return deserializedRule.Left;
                }
                else
                {
                    queries.Add(deserializedRule.Right);
                }
            }

            return new Check(queries);
        }

        static public Either<Errors.FormatError, Check> DeserializeV1(Format.Schema.CheckV1 check)
        {
            IList<Rule> queries = new List<Rule>();

            foreach (Format.Schema.RuleV1 query in check.Queries)
            {
                Either<Errors.FormatError, Rule> res = Rule.DeserializeV1(query);
                if (res.IsLeft)
                {
                    return res.Left;
                }
                else
                {
                    queries.Add(res.Right);
                }
            }

            return new Check(queries);
        }
    }
}
