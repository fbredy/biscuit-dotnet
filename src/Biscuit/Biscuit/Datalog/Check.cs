using System.Collections.Generic;

namespace Biscuit.Datalog
{
    public class Check
    {
        public Check(List<Rule> queries)
        {
            this.queries = queries;
        }

        public List<Rule> queries
        { get; }

        public override int GetHashCode()
        {
            return queries.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public Format.Schema.CheckV1 serialize()
        {
            Format.Schema.CheckV1 check = new Format.Schema.CheckV1();

            for (int i = 0; i < this.queries.Count; i++)
            {
                check.Queries.Add(this.queries[i].serialize());
            }

            return check;
        }

        static public Either<Errors.FormatError, Check> deserializeV0(Format.Schema.CaveatV0 caveat)
        {
            List<Rule> queries = new List<Rule>();

            foreach (Format.Schema.RuleV0 query in caveat.Queries)
            {
                Either<Errors.FormatError, Rule> res = Rule.deserializeV0(query);
                if (res.IsLeft)
                {
                    Errors.FormatError e = res.Left;
                    return new Left(e);
                }
                else
                {
                    queries.Add(res.Right);
                }
            }

            return new Right(new Check(queries));
        }

        static public Either<Errors.FormatError, Check> deserializeV1(Format.Schema.CheckV1 check)
        {
            List<Rule> queries = new List<Rule>();

            foreach (Format.Schema.RuleV1 query in check.Queries)
            {
                Either<Errors.FormatError, Rule> res = Rule.deserializeV1(query);
                if (res.IsLeft)
                {
                    Errors.FormatError e = res.Left;
                    return new Left(e);
                }
                else
                {
                    queries.Add(res.Right);
                }
            }

            return new Right(new Check(queries));
        }
    }
}
