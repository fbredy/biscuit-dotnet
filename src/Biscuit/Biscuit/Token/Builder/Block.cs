using Biscuit.Datalog;
using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class Block
    {
        long index;
        int symbol_start;
        SymbolTable symbols;
        string context;
        List<Datalog.Fact> facts;
        List<Datalog.Rule> rules;
        List<Datalog.Check> checks;

        public Block(long index, SymbolTable base_symbols)
        {
            this.index = index;
            this.symbol_start = base_symbols.symbols.Count;
            this.symbols = new SymbolTable(base_symbols);
            this.context = "";
            this.facts = new List<Datalog.Fact>();
            this.rules = new List<Datalog.Rule>();
            this.checks = new List<Datalog.Check>();
        }

        public void add_fact(Builder.Fact f)
        {
            this.facts.Add(f.convert(this.symbols));
        }

        public Either<Parser.Error, Void> add_fact(string s)
        {
            Either<Parser.Error, Tuple<string, Fact>> res = Parser.Parser.fact(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.Parser(res.Left));
            }

            Tuple<string, Fact> t = res.Right;

            add_fact(t.Item2);

            return new Right(null);
        }

        public void add_rule(Builder.Rule rule)
        {
            this.rules.Add(rule.convert(this.symbols));
        }

        public Either<Parser.Error, Void> add_rule(string s)
        {
            Either<Builder.Parser.Error, Tuple<string, Builder.Rule>> res =
                    Builder.Parser.Parser.rule(s);

            if (res.IsLeft)
            {
                return res.Left;
            }

            Tuple<string, Builder.Rule> t = res.Right;

            add_rule(t.Item2);

            return new Right(null);
        }

        public void add_check(Builder.Check check)
        {
            this.checks.Add(check.convert(this.symbols));
        }

        public Either<Parser.Error, Void> add_check(string s)
        {
            Either<Builder.Parser.Error, Tuple<string, Builder.Check>> res =
                    Builder.Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return res.Left;
            }

            Tuple<string, Builder.Check> t = res.Right;

            add_check(t.Item2);

            return new Right(null);
        }

        public void set_context(string context)
        {
            this.context = context;
        }

        public Token.Block build()
        {
            SymbolTable symbols = new SymbolTable();

            for (int i = this.symbol_start; i < this.symbols.symbols.Count; i++)
            {
                symbols.Add(this.symbols.symbols[i]);
            }

            return new Token.Block(this.index, symbols, this.context, this.facts, this.rules, this.checks);
        }

        public void check_right(string right)
        {
            List<Builder.Rule> queries = new List<Builder.Rule>();
            queries.Add(Utils.rule(
                    "check_right",
                    Arrays.asList(Utils.s(right)),
                    Arrays.asList(
                            Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource"))),
                            Utils.pred("operation", Arrays.asList(Utils.s("ambient"), Utils.s(right))),
                            Utils.pred("right", Arrays.asList(Utils.s("authority"), Utils.var("resource"), Utils.s(right)))
                    )
            ));
            this.add_check(new Builder.Check(queries));
        }

        public void resource_prefix(string prefix)
        {
            List<Builder.Rule> queries = new List<Builder.Rule>();

            queries.Add(Utils.constrained_rule(
                    "prefix",
                    Arrays.asList(Utils.var("resource")),
                    Arrays.asList(Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource")))),
                    Arrays.asList<Expression>(new Expression.Binary(Expression.Op.Prefix, new Expression.Value(Utils.var("resource")), new Expression.Value(Utils.strings(prefix))))
            ));
            this.add_check(new Builder.Check(queries));
        }

        public void resource_suffix(string suffix)
        {
            List<Builder.Rule> queries = new List<Builder.Rule>();

            queries.Add(Utils.constrained_rule(
                    "suffix",
                    Arrays.asList(Utils.var("resource")),
                    Arrays.asList(Utils.pred("resource", Arrays.asList(Utils.s("ambient"), Utils.var("resource")))),
                    Arrays.asList<Expression>(new Expression.Binary(Expression.Op.Suffix, new Expression.Value(Utils.var("resource")),
                            new Expression.Value(Utils.strings(suffix))))
            ));
            this.add_check(new Builder.Check(queries));
        }

        public void expiration_date(DateTime d)
        {
            List<Builder.Rule> queries = new List<Builder.Rule>();

            queries.Add(Utils.constrained_rule(
                    "expiration",
                    Arrays.asList(Utils.var("date")),
                    Arrays.asList(Utils.pred("time", Arrays.asList(Utils.s("ambient"), Utils.var("date")))),
                    Arrays.asList<Expression>(new Expression.Binary(Expression.Op.LessOrEqual, new Expression.Value(Utils.var("date")),
                            new Expression.Value(Utils.date(d))))
            ));
            this.add_check(new Builder.Check(queries));
        }
    }
}
