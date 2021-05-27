using Biscuit.Datalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biscuit.Test
{
    [TestClass]
    public class HashSetTest
    {
        [TestMethod]
        public void plop()
        {
            HashSet<Fact> expected = new HashSet<Fact>(Arrays.asList<Fact>(
                   new Fact(new Predicate(1, Arrays.asList<ID>(new ID.Str("a"), new ID.Str("c"))))
                   ));
            expected.Add(new Fact(new Predicate(1, Arrays.asList<ID>(new ID.Str("a"), new ID.Str("c")))));

            Assert.AreEqual(1, expected.Count);
        }
    }
}
