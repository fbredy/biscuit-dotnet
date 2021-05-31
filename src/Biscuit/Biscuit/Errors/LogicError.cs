using System;
using System.Collections.Generic;
using System.Text;

namespace Biscuit.Errors
{
    public class LogicError
    {
        public Option<List<FailedCheck>> GetFailedChecks()
        {
            return Option<List<FailedCheck>>.Some(new List<FailedCheck>());
        }

        public class InvalidAuthorityFact : LogicError
        {
            public string e;

            public InvalidAuthorityFact(string e)
            {
                this.e = e;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;
                if (obj == null || !(obj is InvalidAuthorityFact)) return false;
                InvalidAuthorityFact other = (InvalidAuthorityFact)obj;
                return e.Equals(other.e);
            }

            public override int GetHashCode()
            {
                return e.GetHashCode();
            }

            public override string ToString()
            {
                return "LogicError.InvalidAuthorityFact{ error: " + e + " }";
            }
        }

        public class InvalidAmbientFact : LogicError
        {
            public string e;

            public InvalidAmbientFact(string e)
            {
                this.e = e;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is InvalidAmbientFact)) return false;
                InvalidAmbientFact other = (InvalidAmbientFact)o;
                return e.Equals(other.e);
            }
            public override int GetHashCode()
            {
                return e.GetHashCode();
            }

            public override string ToString()
            {
                return "LogicError.InvalidAmbientFact{ error: " + e + " }";
            }
        }

        public class InvalidBlockFact : LogicError
        {
            public long id;
            public string e;

            public InvalidBlockFact(long id, string e)
            {
                this.id = id;
                this.e = e;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is InvalidBlockFact)) return false;
                InvalidBlockFact other = (InvalidBlockFact)o;
                return id == other.id && e.Equals(other.e);
            }

            public override int GetHashCode()
            {
                return Objects.Hash(id, e);
            }

            public override string ToString()
            {
                return "LogicError.InvalidBlockFact{ id: " + id + ", error: " + e + " }";
            }
        }
        public class FailedChecks : LogicError
        {
            public List<FailedCheck> errors;

            public FailedChecks(List<FailedCheck> errors)
            {
                this.errors = errors;
            }

            public Option<List<FailedCheck>> failed_checks()
            {
                return Option.Some(errors);
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is FailedChecks)) return false;
                FailedChecks other = (FailedChecks)o;
                if (errors.Count != other.errors.Count)
                {
                    return false;
                }
                for (int i = 0; i < errors.Count; i++)
                {
                    if (!errors[i].Equals(other.errors[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return Objects.Hash(errors);
            }

            public override string ToString()
            {
                return "LogicError.FailedCaveats{ errors: " + errors + " }";
            }
        }

        public class NoMatchingPolicy : LogicError
        {
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override string ToString()
            {
                return "NoMatchingPolicy{}";
            }
        }

        public class Denied : LogicError
        {
            private long id;

            public Denied(long id)
            {
                this.id = id;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override string ToString()
            {
                return "Denied(" + id + ")";
            }
        }

        public class VerifierNotEmpty : LogicError
        {
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(Object obj)
            {
                return base.Equals(obj);
            }

            public override string ToString()
            {
                return "VerifierNotEmpty";
            }
        }
    }
}
