using System;
using System.Collections.Generic;

namespace Biscuit.Errors
{
    public class Error
    {
        public virtual Option<List<FailedCheck>> FailedCheck()
        {
            return Option<List<FailedCheck>>.Some(new List<FailedCheck>());
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            return o != null && this.GetType() == o.GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class FormatError : Error
    { }

    public class Signature : FormatError
    { }
    public class InvalidFormat : Signature
    { }

    public class InvalidSignature : Signature
    {
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            return o != null && this.GetType() == o.GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SealedSignature : FormatError
    {

    }

    public class EmptyKeys : FormatError
    { }

    public class UnknownPublicKey : FormatError
    { }
    public class DeserializationError : FormatError
    {
        public readonly string e;

        public DeserializationError(string e)
        {
            this.e = e;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || this.GetType() != o.GetType()) return false;
            DeserializationError other = (DeserializationError)o;
            return e.Equals(other.e);
        }

        public override int GetHashCode()
        {
            return e.GetHashCode();
        }

        public override string ToString()
        {
            return "Error.FormatError.DeserializationError{ error: " + e + " }";
        }
    }

    public class SerializationError : FormatError
    {
        public readonly string e;

        public SerializationError(String e)
        {
            this.e = e;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            SerializationError other = (SerializationError)o;
            return e.Equals(other.e);
        }


        public override int GetHashCode()
        {
            return e.GetHashCode();
        }

        public override string ToString()
        {
            return "Error.FormatError.SerializationError{ error: " + e + " }";
        }
    }
    public class BlockDeserializationError : FormatError
    {
        public readonly string e;

        public BlockDeserializationError(string e)
        {
            this.e = e;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            BlockDeserializationError other = (BlockDeserializationError)o;
            return e.Equals(other.e);
        }

        public override int GetHashCode()
        {
            return e.GetHashCode();
        }

        public override string ToString()
        {
            return "Error.FormatError.BlockDeserializationError{ error: " + e + " }";
        }
    }
    public class BlockSerializationError : FormatError
    {
        public readonly string e;

        public BlockSerializationError(string e)
        {
            this.e = e;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            BlockSerializationError other = (BlockSerializationError)o;
            return e.Equals(other.e);
        }


        public override int GetHashCode()
        {
            return e.GetHashCode();
        }

        public override string ToString()
        {
            return "Error.FormatError.BlockSerializationError{ error: " + e + " }";
        }
    }

    public class VersionError : FormatError
    {
        public readonly uint maximum;
        public readonly uint actual;

        public VersionError(uint maximum, uint actual)
        {
            this.maximum = maximum;
            this.actual = actual;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            VersionError version = (VersionError)o;

            if (maximum != version.maximum) return false;
            return actual == version.actual;
        }


        public override int GetHashCode()
        {
            uint result = maximum;
            result = 31 * result + actual;
            return (int)result;
        }


        public override string ToString()
        {
            return $"Version{{maximum={maximum}, actual={actual}}}";
        }
    }

    public class InvalidAuthorityIndex : Error
    {
        public long index;

        public InvalidAuthorityIndex(long index)
        {
            this.index = index;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is InvalidAuthorityIndex)) return false;
            InvalidAuthorityIndex other = (InvalidAuthorityIndex)o;
            return index == other.index;
        }


        public override int GetHashCode()
        {
            return index.GetHashCode();
        }


        public override string ToString()
        {
            return "Error.InvalidAuthorityIndex{ index: " + index + " }";
        }
    }
    public class InvalidBlockIndex : Error
    {
        public long expected;
        public long found;

        public InvalidBlockIndex(long expected, long found)
        {
            this.expected = expected;
            this.found = found;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is InvalidBlockIndex)) return false;
            InvalidBlockIndex other = (InvalidBlockIndex)o;
            return expected == other.expected && found == other.found;
        }


        public override int GetHashCode()
        {
            return Objects.Hash(expected, found);
        }


        public override string ToString()
        {
            return "Error.InvalidBlockIndex{ expected: " + expected + ", found: " + found + " }";
        }
    }
    public class SymbolTableOverlap : Error
    {

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is SymbolTableOverlap)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class MissingSymbols : Error
    {

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is MissingSymbols)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Sealed : Error
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is Sealed)) return false;
            return true;
        }
    }
    public class FailedLogic : Error
    {
        public LogicError error;

        public FailedLogic(LogicError error)
        {
            this.error = error;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is FailedLogic)) return false;
            FailedLogic other = (FailedLogic)o;
            return error.Equals(other.error);
        }


        public override int GetHashCode()
        {
            return Objects.Hash(error);
        }


        public override string ToString()
        {
            return "Error.FailedLogic{ error: " + error + " }";
        }


        public override Option<List<FailedCheck>> FailedCheck()
        {
            return this.error.GetFailedChecks();
        }

    }

    public class TooManyFacts : Error
    {

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is TooManyFacts)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TooManyIterationsError : Error
    {

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is TooManyIterationsError)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TimeoutError : Error
    {
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is TimeoutError)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ParserError : Error
    {
        public Token.Builder.Parser.Error error;

        public ParserError(Token.Builder.Parser.Error error)
        {
            this.error = error;
        }


        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is ParserError)) return false;

            ParserError parser = (ParserError)o;

            return error.Equals(parser.error);
        }


        public override int GetHashCode()
        {
            return error.GetHashCode();
        }


        public override string ToString()
        {
            return $"Parser{{error={error}}}";
        }
    }
}

