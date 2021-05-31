namespace Biscuit.Token.Builder.Parser
{
    public class Error
    {
        readonly string input;
        readonly string message;

        public Error(string input, string message)
        {
            this.input = input;
            this.message = message;
        }

        public override string ToString()
        {
            return $"Error{{input='{input}\', message='{message}\'}}";
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Error error = (Error)o;

            if (input != null ? !input.Equals(error.input) : error.input != null) return false;
            return message != null ? message.Equals(error.message) : error.message == null;
        }

        public override int GetHashCode()
        {
            int result = input != null ? input.GetHashCode() : 0;
            result = 31 * result + (message != null ? message.GetHashCode() : 0);
            return result;
        }
    }
}
