using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Biscuit.Token.Builder
{
    public class BiscuitBuilder
    {
        readonly RNGCryptoServiceProvider RandomNumberGenerator;
        readonly KeyPair Root;
        readonly int SymbolStart;
        readonly SymbolTable Symbols;
        string context;
        readonly List<Fact> Facts;
        readonly List<Rule> Rules;
        readonly List<Check> Checks;

        public BiscuitBuilder(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable base_symbols)
        {
            this.RandomNumberGenerator = rng;
            this.Root = root;
            this.SymbolStart = base_symbols.Symbols.Count;
            this.Symbols = new SymbolTable(base_symbols);
            this.context = string.Empty;
            this.Facts = new List<Fact>();
            this.Rules = new List<Rule>();
            this.Checks = new List<Check>();
        }

        public void AddAuthorityFact(FactBuilder f)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (f.Predicate.Ids.Count == 0 || !(f.Predicate.Ids[0].Equals(authority_symbol)))
            {
                List<Term> ids = new List<Term>
                {
                    authority_symbol
                };
                ids.AddRange(f.Predicate.Ids);
                f.Predicate.Ids = ids;
            }

            this.Facts.Add(f.Convert(this.Symbols));
        }

        public Either<Error, Void> AddAuthorityFact(string s)
        {
            var res = Parser.Parser.Fact(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, FactBuilder> t = res.Right;

            AddAuthorityFact(t.Item2);

            return new Right(null);
        }

        public void AddAuthorityRule(RuleBuilder rule)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (rule.Head.Ids.Count == 0 || !(rule.Head.Ids[0].Equals(authority_symbol)))
            {
                rule.Head.Ids.Insert(0, authority_symbol);
            }

            this.Rules.Add(rule.Convert(this.Symbols));
        }

        public Either<Error, Void> AddAuthorityRule(string s)
        {
            var res = Token.Builder.Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, RuleBuilder> t = res.Right;

            AddAuthorityRule(t.Item2);

            return new Right(null);
        }

        public void AddAuthorityCheck(CheckBuilder c)
        {
            this.Checks.Add(c.Convert(this.Symbols));
        }

        public Either<Error, Void> AddAuthorityCheck(string s)
        {
            var res = Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, CheckBuilder> t = res.Right;

            AddAuthorityCheck(t.Item2);

            return new Right(null);
        }

        public void SetContext(string context)
        {
            this.context = context;
        }

        public Either<Error, Biscuit> Build()
        {
            SymbolTable baseSymbols = new SymbolTable();
            SymbolTable symbols = new SymbolTable();

            for (int i = 0; i < this.SymbolStart; i++)
            {
                baseSymbols.Add(this.Symbols.Symbols[i]);
            }

            for (int i = this.SymbolStart; i < this.Symbols.Symbols.Count; i++)
            {
                symbols.Add(this.Symbols.Symbols[i]);
            }

            Block authority_block = new Block(0, symbols, context, this.Facts, this.Rules, this.Checks);
            return Biscuit.Make(this.RandomNumberGenerator, this.Root, baseSymbols, authority_block);
        }

        public void AddRight(string resource, string right)
        {
            this.AddAuthorityFact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Strings(resource), Utils.Symbol(right))));
        }
    }

}
