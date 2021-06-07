using System.Collections.Generic;
using System.Linq;

namespace Biscuit
{
    public static class Objects
    {
        public static int Hash(params object[] a)
        {
            if (a == null)
            { return 0; }

            int result = 1;

            foreach (object element in a)
            {
                result = 31 * result + (element == null ? 0 : element.GetHashCode());
            }

            return result;
        }

        public static int GetSequenceHashCode<T>(this ICollection<T> sequence)
        {
            return sequence
                .Select(item => item.GetHashCode())
                .Aggregate((total, nextCode) => total ^ nextCode);
        }
    }
}
