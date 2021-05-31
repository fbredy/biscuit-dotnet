using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit
{
    public static class HashSetExtension
    {
        public static void AddAll<T>(this HashSet<T> source, IEnumerable<T> collectionsToAdd)
        {
            if(source != null && collectionsToAdd != null)
            {
                foreach (T item in collectionsToAdd)
                {
                    if (!source.Contains(item))
                    {
                        source.Add(item);
                    }
                }
            }
        }

        public static bool ContainsAll<T>(this HashSet<T> superset, HashSet<T> subset)
        {
            return !subset.Except(superset).Any();
        }
    }
}
