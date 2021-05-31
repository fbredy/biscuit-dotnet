using Biscuit.Datalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Test.Datalog
{
    [TestClass]
    public class WorldTest
    {

        [TestMethod]
        public void testFamily()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();
            ID a = syms.Add("A");
            ID b = syms.Add("B");
            ID c = syms.Add("C");
            ID d = syms.Add("D");
            ID e = syms.Add("E");
            ulong parent = syms.Insert("parent");
            ulong grandparent = syms.Insert("grandparent");
            ulong sibling = syms.Insert("siblings");

            w.AddFact(new Fact(new Predicate(parent, Arrays.AsList(a, b))));
            w.AddFact(new Fact(new Predicate(parent, Arrays.AsList(b, c))));
            w.AddFact(new Fact(new Predicate(parent, Arrays.AsList(c, d))));

            Rule r1 = new Rule(new Predicate(grandparent,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")), new ID.Variable(syms.Insert("grandchild")))), Arrays.AsList(
                  new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")), new ID.Variable(syms.Insert("parent")))),
                  new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("parent")), new ID.Variable(syms.Insert("grandchild"))))
            ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing r1: " + syms.PrintRule(r1));
            var query_rule_result = w.QueryRule(r1);
            Console.WriteLine("grandparents query_rules: [" + string.Join(", ", query_rule_result.Select((f)=>syms.PrintFact(f))) + "]");
            Console.WriteLine("current facts: [" + string.Join(", ", w.Facts.Select((f)=>syms.PrintFact(f))) + "]");

            Rule r2 = new Rule(new Predicate(grandparent,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")), new ID.Variable(syms.Insert("grandchild")))), Arrays.AsList(
                 new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")), new ID.Variable(syms.Insert("parent")))),
                 new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("parent")), new ID.Variable(syms.Insert("grandchild"))))
           ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("adding r2: " + syms.PrintRule(r2));
            w.AddRule(r2);
            w.Run(new HashSet<ulong>());

            Console.WriteLine("parents:");
            foreach (Fact fact in w.Query(new Predicate(parent,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("parent")), new ID.Variable(syms.Insert("child"))))))
            {
                Console.WriteLine("\t" + syms.PrintFact(fact));
            }
            Console.WriteLine("parents of B: [" + string.Join(", ",
                    w.Query(new Predicate(parent, Arrays.AsList(new ID.Variable(syms.Insert("parent")), b)))
                            .Select((f)=>syms.PrintFact(f))) + "]");
            Console.WriteLine("grandparents: [" + string.Join(", ",
                    w.Query(new Predicate(grandparent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")),
                            new ID.Variable(syms.Insert("grandchild")))))
                            .Select((f)=>syms.PrintFact(f))) + "]");

            w.AddFact(new Fact(new Predicate(parent, Arrays.AsList(c, e))));
            w.Run(new HashSet<ulong>());

            HashSet<Fact> res = w.Query(new Predicate(grandparent,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("grandparent")), new ID.Variable(syms.Insert("grandchild")))));
            Console.WriteLine("grandparents after inserting parent(C, E): [" + string.Join(", ",
                    res.Select((f)=>syms.PrintFact(f))) + "]");

            HashSet<Fact> expected = new HashSet<Fact>(Arrays.AsList<Fact>(
                   new Fact(new Predicate(grandparent, Arrays.AsList(a, c))),
                   new Fact(new Predicate(grandparent, Arrays.AsList(b, d))),
                   new Fact(new Predicate(grandparent, Arrays.AsList(b, e)))));
            
            Assert.IsTrue(expected.SequenceEqual(res));

            w.AddRule(new Rule(new Predicate(sibling,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("sibling1")), new ID.Variable(syms.Insert("sibling2")))), Arrays.AsList(
                  new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("parent")), new ID.Variable(syms.Insert("sibling1")))),
                  new Predicate(parent, Arrays.AsList<ID>(new ID.Variable(syms.Insert("parent")), new ID.Variable(syms.Insert("sibling2"))))
            ), new List<Biscuit.Datalog.Expressions.Expression>()));
            w.Run(new HashSet<ulong>());

            Console.WriteLine("siblings: [" + string.Join(", ",
                    w.Query(new Predicate(sibling, Arrays.AsList<ID>(
                            new ID.Variable(syms.Insert("sibling1")),
                            new ID.Variable(syms.Insert("sibling2")))))
                            .Select((f)=>syms.PrintFact(f))) + "]");
        }

        [TestMethod]
        public void testNumbers()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID abc = syms.Add("abc");
            ID def = syms.Add("def");
            ID ghi = syms.Add("ghi");
            ID jkl = syms.Add("jkl");
            ID mno = syms.Add("mno");
            ID aaa = syms.Add("AAA");
            ID bbb = syms.Add("BBB");
            ID ccc = syms.Add("CCC");
            ulong t1 = syms.Insert("t1");
            ulong t2 = syms.Insert("t2");
            ulong join = syms.Insert("join");

            w.AddFact(new Fact(new Predicate(t1, Arrays.AsList(new ID.Integer(0), abc))));
            w.AddFact(new Fact(new Predicate(t1, Arrays.AsList(new ID.Integer(1), def))));
            w.AddFact(new Fact(new Predicate(t1, Arrays.AsList(new ID.Integer(2), ghi))));
            w.AddFact(new Fact(new Predicate(t1, Arrays.AsList(new ID.Integer(3), jkl))));
            w.AddFact(new Fact(new Predicate(t1, Arrays.AsList(new ID.Integer(4), mno))));

            w.AddFact(new Fact(new Predicate(t2, Arrays.AsList(new ID.Integer(0), aaa, new ID.Integer(0)))));
            w.AddFact(new Fact(new Predicate(t2, Arrays.AsList(new ID.Integer(1), bbb, new ID.Integer(0)))));
            w.AddFact(new Fact(new Predicate(t2, Arrays.AsList(new ID.Integer(2), ccc, new ID.Integer(1)))));

            HashSet<Fact> res = w.QueryRule(new Rule(new Predicate(join,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("left")), new ID.Variable(syms.Insert("right")))
                  ),
                  Arrays.AsList(new Predicate(t1, Arrays.AsList<ID>(new ID.Variable(syms.Insert("id")), new ID.Variable(syms.Insert("left")))),
                          new Predicate(t2,
                                  Arrays.AsList<ID>(
                                          new ID.Variable(syms.Insert("t2_id")),
                                          new ID.Variable(syms.Insert("right")),
                                          new ID.Variable(syms.Insert("id"))))), new List<Biscuit.Datalog.Expressions.Expression>()));
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            var expected = new HashSet<Fact>(Arrays.AsList<Fact>(new Fact(new Predicate(join, Arrays.AsList(abc, aaa))), new Fact(new Predicate(join, Arrays.AsList(abc, bbb))), new Fact(new Predicate(join, Arrays.AsList(def, ccc)))));
            Assert.IsTrue(expected.SequenceEqual(res));

            res = w.QueryRule(new Rule(new Predicate(join,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("left")), new ID.Variable(syms.Insert("right")))),
                    Arrays.AsList(new Predicate(t1, Arrays.AsList<ID>(new ID.Variable(syms.Insert("id")), new ID.Variable(syms.Insert("left")))),
                            new Predicate(t2,
                                    Arrays.AsList<ID>(
                                            new ID.Variable(syms.Insert("t2_id")),
                                            new ID.Variable(syms.Insert("right")),
                                            new ID.Variable(syms.Insert("id"))))),
                    Arrays.AsList(new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("id"))),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessThan)
                            ))))
            ));
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(join, Arrays.AsList(abc, aaa))), new Fact(new Predicate(join, Arrays.AsList(abc, bbb)))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        private HashSet<Fact> testSuffix(World w, SymbolTable syms, ulong suff, ulong route, String suffix)
        {
            return w.QueryRule(new Rule(new Predicate(suff,
                    Arrays.AsList<ID>(new ID.Variable(syms.Insert("app_id")), new ID.Variable(syms.Insert("domain")))),
                    Arrays.AsList(
                  new Predicate(route, Arrays.AsList<ID>(
                          new ID.Variable(syms.Insert("route_id")),
                          new ID.Variable(syms.Insert("app_id")),
                          new ID.Variable(syms.Insert("domain"))))
            ),
                    Arrays.AsList(new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("domain"))),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Str(suffix)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Suffix)
                    ))))
            ));
        }

        public void testStr()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID app_0 = syms.Add("app_0");
            ID app_1 = syms.Add("app_1");
            ID app_2 = syms.Add("app_2");
            ulong route = syms.Insert("route");
            ulong suff = syms.Insert("route suffix");

            w.AddFact(new Fact(new Predicate(route, Arrays.AsList(new ID.Integer(0), app_0, new ID.Str("example.com")))));
            w.AddFact(new Fact(new Predicate(route, Arrays.AsList(new ID.Integer(1), app_1, new ID.Str("test.com")))));
            w.AddFact(new Fact(new Predicate(route, Arrays.AsList(new ID.Integer(2), app_2, new ID.Str("test.fr")))));
            w.AddFact(new Fact(new Predicate(route, Arrays.AsList(new ID.Integer(3), app_0, new ID.Str("www.example.com")))));
            w.AddFact(new Fact(new Predicate(route, Arrays.AsList(new ID.Integer(4), app_1, new ID.Str("mx.example.com")))));

            HashSet<Fact> res = testSuffix(w, syms, suff, route, ".fr");
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            var expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(suff, Arrays.AsList(app_2, new ID.Str("test.fr"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            res = testSuffix(w, syms, suff, route, "example.com");
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(suff,
                    Arrays.AsList(
                            app_0,
                            new ID.Str("example.com")))),
                    new Fact(new Predicate(suff,
                            Arrays.AsList(app_0, new ID.Str("www.example.com")))),
                    new Fact(new Predicate(suff, Arrays.AsList(app_1, new ID.Str("mx.example.com"))))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        [TestMethod]
        public void testDate()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            DateTimeOffset t1 = DateTimeOffset.Now;
            Console.WriteLine("t1 = " + t1);
            DateTimeOffset t2 = t1.AddSeconds(10);
            Console.WriteLine("t2 = " + t2);
            DateTimeOffset t3 = t2.AddSeconds(30);
            Console.WriteLine("t3 = " + t3);

            
            ulong t2_timestamp = (ulong)t2.ToUnixTimeSeconds();

             ID abc = syms.Add("abc");
            ID def = syms.Add("def");
            ulong x = syms.Insert("x");
            ulong before = syms.Insert("before");
            ulong after = syms.Insert("after");

            w.AddFact(new Fact(new Predicate(x, Arrays.AsList(new ID.Date((ulong)t1.ToUnixTimeSeconds()), abc))));
            w.AddFact(new Fact(new Predicate(x, Arrays.AsList(new ID.Date((ulong)t3.ToUnixTimeSeconds()), def))));

            Rule r1 = new Rule(new Predicate(
                   before,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("date")), new ID.Variable(syms.Insert("val")))),
                   Arrays.AsList(
                           new Predicate(x, Arrays.AsList<ID>(new ID.Variable(syms.Insert("date")), new ID.Variable(syms.Insert("val"))))
                   ),
                   Arrays.AsList(
                        new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList < Biscuit.Datalog.Expressions.Op>(
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("date"))),
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(t2_timestamp)),
                                new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessOrEqual)
                        ))),
                        new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("date"))),
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(0)),
                                new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                        )))
                   )
           );

            Console.WriteLine("testing r1: " + syms.PrintRule(r1));
            var res = w.QueryRule(r1);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            var expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(before, Arrays.AsList(new ID.Date((ulong)t1.ToUnixTimeSeconds()), abc)))));
            Assert.IsTrue(expected.SequenceEqual(res));

            Rule r2 = new Rule(new Predicate(
                   after,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("date")), new ID.Variable(syms.Insert("val")))),
                   Arrays.AsList(
                           new Predicate(x, Arrays.AsList<ID>(new ID.Variable(syms.Insert("date")), new ID.Variable(syms.Insert("val"))))
                   ),
                   Arrays.AsList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList<Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("date"))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(t2_timestamp)),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                           ))),
                          new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("date"))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(0)),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                           )))
                   )
           );

            Console.WriteLine("testing r2: " + syms.PrintRule(r2));
            res = w.QueryRule(r2);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(after, Arrays.AsList(new ID.Date((ulong)t3.ToUnixTimeSeconds()), def)))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        [TestMethod]
        public void testSet()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID abc = syms.Add("abc");
            ID def = syms.Add("def");
            ulong x = syms.Insert("x");
            ulong int_set = syms.Insert("int_set");
            ulong symbol_set = syms.Insert("symbol_set");
            ulong string_set = syms.Insert("string_set");

            w.AddFact(new Fact(new Predicate(x, Arrays.AsList(abc, new ID.Integer(0), new ID.Str("test")))));
            w.AddFact(new Fact(new Predicate(x, Arrays.AsList(def, new ID.Integer(2), new ID.Str("hello")))));

            Rule r1 = new Rule(new Predicate(
                   int_set,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("str")))
           ),
                   Arrays.AsList(new Predicate(x,
                           Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("int")), new ID.Variable(syms.Insert("str"))))
           ),
                   Arrays.AsList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.AsList(new ID.Integer(0l), new ID.Integer(1l))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("int"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains)
                           )))
                   )
           );
            Console.WriteLine("testing r1: " + syms.PrintRule(r1));
            var res = w.QueryRule(r1);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            HashSet<Fact> expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(int_set, Arrays.AsList(abc, new ID.Str("test"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            ulong abc_sym_id = syms.Insert("abc");
            ulong ghi_sym_id = syms.Insert("ghi");

            Rule r2 = new Rule(new Predicate(symbol_set,
                   Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("int")), new ID.Variable(syms.Insert("str")))),
                   Arrays.AsList(new Predicate(x, Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("int")), new ID.Variable(syms.Insert("str"))))
                   ),
                   Arrays.AsList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.AsList<ID>(new ID.Symbol(abc_sym_id), new ID.Symbol(ghi_sym_id))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("sym"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains),
                                   new Biscuit.Datalog.Expressions.Op.Unary(Biscuit.Datalog.Expressions.Op.UnaryOp.Negate)
                           )))
                   )
           );

            Console.WriteLine("testing r2: " + syms.PrintRule(r2));
            res = w.QueryRule(r2);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(symbol_set, Arrays.AsList(def, new ID.Integer(2), new ID.Str("hello"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            Rule r3 = new Rule(
                   new Predicate(string_set, Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("int")), new ID.Variable(syms.Insert("str")))),
                   Arrays.AsList(new Predicate(x, Arrays.AsList<ID>(new ID.Variable(syms.Insert("sym")), new ID.Variable(syms.Insert("int")), new ID.Variable(syms.Insert("str"))))),
                   Arrays.AsList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.AsList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.AsList(new ID.Str("test"), new ID.Str("aaa"))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.Insert("str"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains)
                           )))
                   )
           );
            Console.WriteLine("testing r3: " + syms.PrintRule(r3));
            res = w.QueryRule(r3);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            expected = new HashSet<Fact>(Arrays.AsList(new Fact(new Predicate(string_set, Arrays.AsList(abc, new ID.Integer(0), new ID.Str("test"))))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        [TestMethod]
        public void testResource()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID authority = syms.Add("authority");
            ID ambient = syms.Add("ambient");
            ulong resource = syms.Insert("resource");
            ulong operation = syms.Insert("operation");
            ulong right = syms.Insert("right");
            ID file1 = syms.Add("file1");
            ID file2 = syms.Add("file2");
            ID read = syms.Add("read");
            ID write = syms.Add("write");


            w.AddFact(new Fact(new Predicate(resource, Arrays.AsList(ambient, file2))));
            w.AddFact(new Fact(new Predicate(operation, Arrays.AsList(ambient, write))));
            w.AddFact(new Fact(new Predicate(right, Arrays.AsList(authority, file1, read))));
            w.AddFact(new Fact(new Predicate(right, Arrays.AsList(authority, file2, read))));
            w.AddFact(new Fact(new Predicate(right, Arrays.AsList(authority, file1, write))));

            ulong caveat1 = syms.Insert("caveat1");
            //r1: caveat2(#file1) <- resource(#ambient, #file1)
            Rule r1 = new Rule(
                   new Predicate(caveat1, Arrays.AsList(file1)),
                   Arrays.AsList(new Predicate(resource, Arrays.AsList(ambient, file1))
           ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing caveat 1(should return nothing): " + syms.PrintRule(r1));
            var res = w.QueryRule(r1);
            Console.WriteLine(res);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            Assert.IsTrue(res.IsEmpty());

            ulong caveat2 = syms.Insert("caveat2");
            ulong var0_id = syms.Insert("var0");
            ID var0 = new ID.Variable(var0_id);
            //r2: caveat1(0?) <- resource(#ambient, 0?) && operation(#ambient, #read) && right(#authority, 0?, #read)
            Rule r2 = new Rule(
                   new Predicate(caveat2, Arrays.AsList(var0)),
                   Arrays.AsList(
                           new Predicate(resource, Arrays.AsList(ambient, var0)),
                           new Predicate(operation, Arrays.AsList(ambient, read)),
                           new Predicate(right, Arrays.AsList(authority, var0, read))
                   ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing caveat 2: " + syms.PrintRule(r2));
            res = w.QueryRule(r2);
            Console.WriteLine(res);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.PrintFact(f));
            }
            Assert.IsTrue(res.IsEmpty());
        }
    }

}
