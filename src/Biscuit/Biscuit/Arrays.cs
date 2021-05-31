using System.Collections.Generic;
using System.Linq;

namespace Biscuit
{
    public class Arrays
    {
        public static List<T> AsList<T>(params T[] obj)
        {
            return obj.ToList();
        }

        public static bool Equals<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            bool result = (a == null && b == null);

            if (!result && a != null && b != null && a.Count() == b.Count())
            {
                for (int i = 0; i < a.Count() && !result; i++)
                {
                    result = a.ElementAt(i).Equals(b.ElementAt(i));
                }
            }

            return result;
        }


    }
}