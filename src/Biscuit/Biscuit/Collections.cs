using System;
using System.Collections.Generic;
using System.Linq;

namespace Biscuit
{
    public static class Collections
    {
        /// <summary>
        /// It is used to check whether two specified collections are disjoint or not. More formally, two collections are disjoint if they have no elements in common.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection1"></param>
        /// <param name="colletion2"></param>
        /// <returns>true if the two specified collections have no elements in common.</returns>
        public static bool Disjoint<T>(IEnumerable<T> collection1, IEnumerable<T> colletion2)
        {
            if(collection1 == null)
            {
                throw new ArgumentNullException(nameof(collection1));
            }

            if(colletion2 == null)
            {
                throw new ArgumentNullException(nameof(colletion2));
            }

            foreach(var s in collection1)
            {
                if (colletion2.Contains(s))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection.Count() == 0;
        }        
    }
}
