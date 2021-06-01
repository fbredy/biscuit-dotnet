using Biscuit.Datalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Biscuit.Test
{
    [TestClass]
    public class HashSetTest
    {
        [TestMethod]
        public void ShouldNotAddToTheHashSetIfAlreadyIn()
        {
            HashSet<Fact> expected = new HashSet<Fact>(Arrays.AsList<Fact>(
                   new Fact(new Predicate(1, Arrays.AsList<ID>(new ID.Str("a"), new ID.Str("c"))))
                   ));
            expected.Add(new Fact(new Predicate(1, Arrays.AsList<ID>(new ID.Str("a"), new ID.Str("c")))));

            Assert.AreEqual(1, expected.Count);
        }
    }
}
