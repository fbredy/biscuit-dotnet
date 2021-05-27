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
            ulong parent = syms.insert("parent");
            ulong grandparent = syms.insert("grandparent");
            ulong sibling = syms.insert("siblings");

            w.add_fact(new Fact(new Predicate(parent, Arrays.asList(a, b))));
            w.add_fact(new Fact(new Predicate(parent, Arrays.asList(b, c))));
            w.add_fact(new Fact(new Predicate(parent, Arrays.asList(c, d))));

            Rule r1 = new Rule(new Predicate(grandparent,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")), new ID.Variable(syms.insert("grandchild")))), Arrays.asList(
                  new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")), new ID.Variable(syms.insert("parent")))),
                  new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("parent")), new ID.Variable(syms.insert("grandchild"))))
            ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing r1: " + syms.print_rule(r1));
            var query_rule_result = w.query_rule(r1);
            Console.WriteLine("grandparents query_rules: [" + string.Join(", ", query_rule_result.Select((f)=>syms.print_fact(f))) + "]");
            Console.WriteLine("current facts: [" + string.Join(", ", w.facts.Select((f)=>syms.print_fact(f))) + "]");

            Rule r2 = new Rule(new Predicate(grandparent,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")), new ID.Variable(syms.insert("grandchild")))), Arrays.asList(
                 new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")), new ID.Variable(syms.insert("parent")))),
                 new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("parent")), new ID.Variable(syms.insert("grandchild"))))
           ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("adding r2: " + syms.print_rule(r2));
            w.add_rule(r2);
            w.run(new HashSet<ulong>());

            Console.WriteLine("parents:");
            foreach (Fact fact in w.query(new Predicate(parent,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("parent")), new ID.Variable(syms.insert("child"))))))
            {
                Console.WriteLine("\t" + syms.print_fact(fact));
            }
            Console.WriteLine("parents of B: [" + string.Join(", ",
                    w.query(new Predicate(parent, Arrays.asList(new ID.Variable(syms.insert("parent")), b)))
                            .Select((f)=>syms.print_fact(f))) + "]");
            Console.WriteLine("grandparents: [" + string.Join(", ",
                    w.query(new Predicate(grandparent, Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")),
                            new ID.Variable(syms.insert("grandchild")))))
                            .Select((f)=>syms.print_fact(f))) + "]");

            w.add_fact(new Fact(new Predicate(parent, Arrays.asList(c, e))));
            w.run(new HashSet<ulong>());

            HashSet<Fact> res = w.query(new Predicate(grandparent,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("grandparent")), new ID.Variable(syms.insert("grandchild")))));
            Console.WriteLine("grandparents after inserting parent(C, E): [" + string.Join(", ",
                    res.Select((f)=>syms.print_fact(f))) + "]");

            HashSet<Fact> expected = new HashSet<Fact>(Arrays.asList<Fact>(
                   new Fact(new Predicate(grandparent, Arrays.asList(a, c))),
                   new Fact(new Predicate(grandparent, Arrays.asList(b, d))),
                   new Fact(new Predicate(grandparent, Arrays.asList(b, e)))));
            
            Assert.IsTrue(expected.SequenceEqual(res));

            w.add_rule(new Rule(new Predicate(sibling,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("sibling1")), new ID.Variable(syms.insert("sibling2")))), Arrays.asList(
                  new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("parent")), new ID.Variable(syms.insert("sibling1")))),
                  new Predicate(parent, Arrays.asList<ID>(new ID.Variable(syms.insert("parent")), new ID.Variable(syms.insert("sibling2"))))
            ), new List<Biscuit.Datalog.Expressions.Expression>()));
            w.run(new HashSet<ulong>());

            Console.WriteLine("siblings: [" + string.Join(", ",
                    w.query(new Predicate(sibling, Arrays.asList<ID>(
                            new ID.Variable(syms.insert("sibling1")),
                            new ID.Variable(syms.insert("sibling2")))))
                            .Select((f)=>syms.print_fact(f))) + "]");
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
            ulong t1 = syms.insert("t1");
            ulong t2 = syms.insert("t2");
            ulong join = syms.insert("join");

            w.add_fact(new Fact(new Predicate(t1, Arrays.asList(new ID.Integer(0), abc))));
            w.add_fact(new Fact(new Predicate(t1, Arrays.asList(new ID.Integer(1), def))));
            w.add_fact(new Fact(new Predicate(t1, Arrays.asList(new ID.Integer(2), ghi))));
            w.add_fact(new Fact(new Predicate(t1, Arrays.asList(new ID.Integer(3), jkl))));
            w.add_fact(new Fact(new Predicate(t1, Arrays.asList(new ID.Integer(4), mno))));

            w.add_fact(new Fact(new Predicate(t2, Arrays.asList(new ID.Integer(0), aaa, new ID.Integer(0)))));
            w.add_fact(new Fact(new Predicate(t2, Arrays.asList(new ID.Integer(1), bbb, new ID.Integer(0)))));
            w.add_fact(new Fact(new Predicate(t2, Arrays.asList(new ID.Integer(2), ccc, new ID.Integer(1)))));

            HashSet<Fact> res = w.query_rule(new Rule(new Predicate(join,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("left")), new ID.Variable(syms.insert("right")))
                  ),
                  Arrays.asList(new Predicate(t1, Arrays.asList<ID>(new ID.Variable(syms.insert("id")), new ID.Variable(syms.insert("left")))),
                          new Predicate(t2,
                                  Arrays.asList<ID>(
                                          new ID.Variable(syms.insert("t2_id")),
                                          new ID.Variable(syms.insert("right")),
                                          new ID.Variable(syms.insert("id"))))), new List<Biscuit.Datalog.Expressions.Expression>()));
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            var expected = new HashSet<Fact>(Arrays.asList<Fact>(new Fact(new Predicate(join, Arrays.asList(abc, aaa))), new Fact(new Predicate(join, Arrays.asList(abc, bbb))), new Fact(new Predicate(join, Arrays.asList(def, ccc)))));
            Assert.IsTrue(expected.SequenceEqual(res));

            res = w.query_rule(new Rule(new Predicate(join,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("left")), new ID.Variable(syms.insert("right")))),
                    Arrays.asList(new Predicate(t1, Arrays.asList<ID>(new ID.Variable(syms.insert("id")), new ID.Variable(syms.insert("left")))),
                            new Predicate(t2,
                                    Arrays.asList<ID>(
                                            new ID.Variable(syms.insert("t2_id")),
                                            new ID.Variable(syms.insert("right")),
                                            new ID.Variable(syms.insert("id"))))),
                    Arrays.asList(new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList<Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("id"))),
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Integer(1)),
                            new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessThan)
                            ))))
            ));
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(join, Arrays.asList(abc, aaa))), new Fact(new Predicate(join, Arrays.asList(abc, bbb)))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        private HashSet<Fact> testSuffix(World w, SymbolTable syms, ulong suff, ulong route, String suffix)
        {
            return w.query_rule(new Rule(new Predicate(suff,
                    Arrays.asList<ID>(new ID.Variable(syms.insert("app_id")), new ID.Variable(syms.insert("domain")))),
                    Arrays.asList(
                  new Predicate(route, Arrays.asList<ID>(
                          new ID.Variable(syms.insert("route_id")),
                          new ID.Variable(syms.insert("app_id")),
                          new ID.Variable(syms.insert("domain"))))
            ),
                    Arrays.asList(new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                            new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("domain"))),
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
            ulong route = syms.insert("route");
            ulong suff = syms.insert("route suffix");

            w.add_fact(new Fact(new Predicate(route, Arrays.asList(new ID.Integer(0), app_0, new ID.Str("example.com")))));
            w.add_fact(new Fact(new Predicate(route, Arrays.asList(new ID.Integer(1), app_1, new ID.Str("test.com")))));
            w.add_fact(new Fact(new Predicate(route, Arrays.asList(new ID.Integer(2), app_2, new ID.Str("test.fr")))));
            w.add_fact(new Fact(new Predicate(route, Arrays.asList(new ID.Integer(3), app_0, new ID.Str("www.example.com")))));
            w.add_fact(new Fact(new Predicate(route, Arrays.asList(new ID.Integer(4), app_1, new ID.Str("mx.example.com")))));

            HashSet<Fact> res = testSuffix(w, syms, suff, route, ".fr");
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            var expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(suff, Arrays.asList(app_2, new ID.Str("test.fr"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            res = testSuffix(w, syms, suff, route, "example.com");
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(suff,
                    Arrays.asList(
                            app_0,
                            new ID.Str("example.com")))),
                    new Fact(new Predicate(suff,
                            Arrays.asList(app_0, new ID.Str("www.example.com")))),
                    new Fact(new Predicate(suff, Arrays.asList(app_1, new ID.Str("mx.example.com"))))));
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
            ulong x = syms.insert("x");
            ulong before = syms.insert("before");
            ulong after = syms.insert("after");

            w.add_fact(new Fact(new Predicate(x, Arrays.asList(new ID.Date((ulong)t1.ToUnixTimeSeconds()), abc))));
            w.add_fact(new Fact(new Predicate(x, Arrays.asList(new ID.Date((ulong)t3.ToUnixTimeSeconds()), def))));

            Rule r1 = new Rule(new Predicate(
                   before,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("date")), new ID.Variable(syms.insert("val")))),
                   Arrays.asList(
                           new Predicate(x, Arrays.asList<ID>(new ID.Variable(syms.insert("date")), new ID.Variable(syms.insert("val"))))
                   ),
                   Arrays.asList(
                        new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList < Biscuit.Datalog.Expressions.Op>(
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("date"))),
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(t2_timestamp)),
                                new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.LessOrEqual)
                        ))),
                        new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("date"))),
                                new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(0)),
                                new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                        )))
                   )
           );

            Console.WriteLine("testing r1: " + syms.print_rule(r1));
            var res = w.query_rule(r1);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            var expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(before, Arrays.asList(new ID.Date((ulong)t1.ToUnixTimeSeconds()), abc)))));
            Assert.IsTrue(expected.SequenceEqual(res));

            Rule r2 = new Rule(new Predicate(
                   after,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("date")), new ID.Variable(syms.insert("val")))),
                   Arrays.asList(
                           new Predicate(x, Arrays.asList<ID>(new ID.Variable(syms.insert("date")), new ID.Variable(syms.insert("val"))))
                   ),
                   Arrays.asList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList<Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("date"))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(t2_timestamp)),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                           ))),
                          new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("date"))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Date(0)),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.GreaterOrEqual)
                           )))
                   )
           );

            Console.WriteLine("testing r2: " + syms.print_rule(r2));
            res = w.query_rule(r2);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(after, Arrays.asList(new ID.Date((ulong)t3.ToUnixTimeSeconds()), def)))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        [TestMethod]
        public void testSet()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID abc = syms.Add("abc");
            ID def = syms.Add("def");
            ulong x = syms.insert("x");
            ulong int_set = syms.insert("int_set");
            ulong symbol_set = syms.insert("symbol_set");
            ulong string_set = syms.insert("string_set");

            w.add_fact(new Fact(new Predicate(x, Arrays.asList(abc, new ID.Integer(0), new ID.Str("test")))));
            w.add_fact(new Fact(new Predicate(x, Arrays.asList(def, new ID.Integer(2), new ID.Str("hello")))));

            Rule r1 = new Rule(new Predicate(
                   int_set,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("str")))
           ),
                   Arrays.asList(new Predicate(x,
                           Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("int")), new ID.Variable(syms.insert("str"))))
           ),
                   Arrays.asList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.asList(new ID.Integer(0l), new ID.Integer(1l))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("int"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains)
                           )))
                   )
           );
            Console.WriteLine("testing r1: " + syms.print_rule(r1));
            var res = w.query_rule(r1);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            HashSet<Fact> expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(int_set, Arrays.asList(abc, new ID.Str("test"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            ulong abc_sym_id = syms.insert("abc");
            ulong ghi_sym_id = syms.insert("ghi");

            Rule r2 = new Rule(new Predicate(symbol_set,
                   Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("int")), new ID.Variable(syms.insert("str")))),
                   Arrays.asList(new Predicate(x, Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("int")), new ID.Variable(syms.insert("str"))))
                   ),
                   Arrays.asList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.asList<ID>(new ID.Symbol(abc_sym_id), new ID.Symbol(ghi_sym_id))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("sym"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains),
                                   new Biscuit.Datalog.Expressions.Op.Unary(Biscuit.Datalog.Expressions.Op.UnaryOp.Negate)
                           )))
                   )
           );

            Console.WriteLine("testing r2: " + syms.print_rule(r2));
            res = w.query_rule(r2);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(symbol_set, Arrays.asList(def, new ID.Integer(2), new ID.Str("hello"))))));
            Assert.IsTrue(expected.SequenceEqual(res));

            Rule r3 = new Rule(
                   new Predicate(string_set, Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("int")), new ID.Variable(syms.insert("str")))),
                   Arrays.asList(new Predicate(x, Arrays.asList<ID>(new ID.Variable(syms.insert("sym")), new ID.Variable(syms.insert("int")), new ID.Variable(syms.insert("str"))))),
                   Arrays.asList(
                           new Biscuit.Datalog.Expressions.Expression(new List<Biscuit.Datalog.Expressions.Op>(Arrays.asList< Biscuit.Datalog.Expressions.Op>(
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Set(new HashSet<ID>(Arrays.asList(new ID.Str("test"), new ID.Str("aaa"))))),
                                   new Biscuit.Datalog.Expressions.Op.Value(new ID.Variable(syms.insert("str"))),
                                   new Biscuit.Datalog.Expressions.Op.Binary(Biscuit.Datalog.Expressions.Op.BinaryOp.Contains)
                           )))
                   )
           );
            Console.WriteLine("testing r3: " + syms.print_rule(r3));
            res = w.query_rule(r3);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            expected = new HashSet<Fact>(Arrays.asList(new Fact(new Predicate(string_set, Arrays.asList(abc, new ID.Integer(0), new ID.Str("test"))))));
            Assert.IsTrue(expected.SequenceEqual(res));
        }

        [TestMethod]
        public void testResource()
        {
            World w = new World();
            SymbolTable syms = new SymbolTable();

            ID authority = syms.Add("authority");
            ID ambient = syms.Add("ambient");
            ulong resource = syms.insert("resource");
            ulong operation = syms.insert("operation");
            ulong right = syms.insert("right");
            ID file1 = syms.Add("file1");
            ID file2 = syms.Add("file2");
            ID read = syms.Add("read");
            ID write = syms.Add("write");


            w.add_fact(new Fact(new Predicate(resource, Arrays.asList(ambient, file2))));
            w.add_fact(new Fact(new Predicate(operation, Arrays.asList(ambient, write))));
            w.add_fact(new Fact(new Predicate(right, Arrays.asList(authority, file1, read))));
            w.add_fact(new Fact(new Predicate(right, Arrays.asList(authority, file2, read))));
            w.add_fact(new Fact(new Predicate(right, Arrays.asList(authority, file1, write))));

            ulong caveat1 = syms.insert("caveat1");
            //r1: caveat2(#file1) <- resource(#ambient, #file1)
            Rule r1 = new Rule(
                   new Predicate(caveat1, Arrays.asList(file1)),
                   Arrays.asList(new Predicate(resource, Arrays.asList(ambient, file1))
           ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing caveat 1(should return nothing): " + syms.print_rule(r1));
            var res = w.query_rule(r1);
            Console.WriteLine(res);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            Assert.IsTrue(res.isEmpty());

            ulong caveat2 = syms.insert("caveat2");
            ulong var0_id = syms.insert("var0");
            ID var0 = new ID.Variable(var0_id);
            //r2: caveat1(0?) <- resource(#ambient, 0?) && operation(#ambient, #read) && right(#authority, 0?, #read)
            Rule r2 = new Rule(
                   new Predicate(caveat2, Arrays.asList(var0)),
                   Arrays.asList(
                           new Predicate(resource, Arrays.asList(ambient, var0)),
                           new Predicate(operation, Arrays.asList(ambient, read)),
                           new Predicate(right, Arrays.asList(authority, var0, read))
                   ), new List<Biscuit.Datalog.Expressions.Expression>());

            Console.WriteLine("testing caveat 2: " + syms.print_rule(r2));
            res = w.query_rule(r2);
            Console.WriteLine(res);
            foreach (Fact f in res)
            {
                Console.WriteLine("\t" + syms.print_fact(f));
            }
            Assert.IsTrue(res.isEmpty());
        }
    }

}
