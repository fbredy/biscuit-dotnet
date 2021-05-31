using Biscuit.Datalog;
using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class BlockBuilder
    {
        readonly long index;
        readonly int symbolStart;
        readonly SymbolTable symbols;
        private string context;
        readonly List<Fact> facts;
        readonly List<Rule> rules;
        readonly List<Check> checks;

        public BlockBuilder(long index, SymbolTable baseSymbols)
        {
            this.index = index;
            this.symbolStart = baseSymbols.Symbols.Count;
            this.symbols = new SymbolTable(baseSymbols);
            this.context = "";
            this.facts = new List<Fact>();
            this.rules = new List<Rule>();
            this.checks = new List<Check>();
        }

        public void AddFact(FactBuilder f)
        {
            this.facts.Add(f.Convert(this.symbols));
        }

        public Either<Parser.Error, Void> AddFact(string s)
        {
            Either<Parser.Error, Tuple<string, FactBuilder>> res = Parser.Parser.Fact(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.ParserError(res.Left));
            }

            Tuple<string, FactBuilder> t = res.Right;

            AddFact(t.Item2);

            return new Right(null);
        }

        public void AddRule(RuleBuilder rule)
        {
            this.rules.Add(rule.Convert(this.symbols));
        }

        public Either<Parser.Error, Void> AddRule(string s)
        {
            var res = Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return res.Left;
            }

            Tuple<string, RuleBuilder> t = res.Right;

            AddRule(t.Item2);

            return new Right(null);
        }

        public void AddCheck(CheckBuilder check)
        {
            this.checks.Add(check.convert(this.symbols));
        }

        public Either<Parser.Error, Void> AddCheck(string s)
        {
            var res = Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return res.Left;
            }

            Tuple<string, CheckBuilder> t = res.Right;

            AddCheck(t.Item2);

            return new Right(null);
        }

        public void SetContext(string context)
        {
            this.context = context;
        }

        public Token.Block Build()
        {
            SymbolTable symbols = new SymbolTable();

            for (int i = this.symbolStart; i < this.symbols.Symbols.Count; i++)
            {
                symbols.Add(this.symbols.Symbols[i]);
            }

            return new Token.Block(this.index, symbols, this.context, this.facts, this.rules, this.checks);
        }

        public void CheckRight(string right)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();
            queries.Add(Utils.Rule(
                    "check_right",
                    Arrays.AsList(Utils.Symbol(right)),
                    Arrays.AsList(
                            Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource"))),
                            Utils.Pred("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol(right))),
                            Utils.Pred("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Var("resource"), Utils.Symbol(right)))
                    )
            ));
            this.AddCheck(new CheckBuilder(queries));
        }

        public void ResourcePrefix(string prefix)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();

            queries.Add(Utils.ConstrainedRule(
                    "prefix",
                    Arrays.AsList(Utils.Var("resource")),
                    Arrays.AsList(Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource")))),
                    Arrays.AsList<ExpressionBuilder>(new ExpressionBuilder.Binary(ExpressionBuilder.Op.Prefix, new ExpressionBuilder.Value(Utils.Var("resource")), new ExpressionBuilder.Value(Utils.Strings(prefix))))
            ));
            this.AddCheck(new CheckBuilder(queries));
        }

        public void ResourceSuffix(string suffix)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();

            queries.Add(Utils.ConstrainedRule(
                    "suffix",
                    Arrays.AsList(Utils.Var("resource")),
                    Arrays.AsList(Utils.Pred("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("resource")))),
                    Arrays.AsList<ExpressionBuilder>(new ExpressionBuilder.Binary(ExpressionBuilder.Op.Suffix, new ExpressionBuilder.Value(Utils.Var("resource")),
                            new ExpressionBuilder.Value(Utils.Strings(suffix))))
            ));
            this.AddCheck(new CheckBuilder(queries));
        }

        public void ExpirationDate(DateTime d)
        {
            List<RuleBuilder> queries = new List<RuleBuilder>();

            queries.Add(Utils.ConstrainedRule(
                    "expiration",
                    Arrays.AsList(Utils.Var("date")),
                    Arrays.AsList(Utils.Pred("time", Arrays.AsList(Utils.Symbol("ambient"), Utils.Var("date")))),
                    Arrays.AsList<ExpressionBuilder>(new ExpressionBuilder.Binary(ExpressionBuilder.Op.LessOrEqual, new ExpressionBuilder.Value(Utils.Var("date")),
                            new ExpressionBuilder.Value(Utils.Date(d))))
            ));
            this.AddCheck(new CheckBuilder(queries));
        }
    }
}
