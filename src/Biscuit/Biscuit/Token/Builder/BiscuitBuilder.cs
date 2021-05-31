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
        RNGCryptoServiceProvider rng;
        KeyPair root;
        int symbol_start;
        SymbolTable symbols;
        string context;
        List<Datalog.Fact> facts;
        List<Datalog.Rule> rules;
        List<Datalog.Check> checks;

        public BiscuitBuilder(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable base_symbols)
        {
            this.rng = rng;
            this.root = root;
            this.symbol_start = base_symbols.Symbols.Count;
            this.symbols = new SymbolTable(base_symbols);
            this.context = "";
            this.facts = new List<Datalog.Fact>();
            this.rules = new List<Datalog.Rule>();
            this.checks = new List<Datalog.Check>();
        }

        public void add_authority_fact(FactBuilder f)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (f.Predicate.Ids.Count == 0 || !(f.Predicate.Ids[0].Equals(authority_symbol)))
            {
                List<Term> ids = new List<Term>();
                ids.Add(authority_symbol);
                foreach (Term id in f.Predicate.Ids)
                {
                    ids.Add(id);
                }
                f.Predicate.Ids = ids;
            }

            this.facts.Add(f.Convert(this.symbols));
        }

        public Either<Error, Void> add_authority_fact(string s)
        {
            Either<Parser.Error, Tuple<string, FactBuilder>> res = Parser.Parser.Fact(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.ParserError(res.Left));
            }

            Tuple<string, Token.Builder.FactBuilder> t = res.Right;

            add_authority_fact(t.Item2);

            return new Right(null);
        }

        public void add_authority_rule(Builder.RuleBuilder rule)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (rule.Head.Ids.Count == 0 || !(rule.Head.Ids[0].Equals(authority_symbol)))
            {
                rule.Head.Ids.Insert(0, authority_symbol);
            }

            this.rules.Add(rule.Convert(this.symbols));
        }

        public Either<Error, Void> add_authority_rule(String s)
        {
            Either<Builder.Parser.Error, Tuple<String, Token.Builder.RuleBuilder>> res =
                    Token.Builder.Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.ParserError(res.Left));
            }

            Tuple<String, Token.Builder.RuleBuilder> t = res.Right;

            add_authority_rule(t.Item2);

            return new Right(null);
        }

        public void add_authority_check(Token.Builder.CheckBuilder c)
        {
            this.checks.Add(c.convert(this.symbols));
        }

        public Either<Error, Void> add_authority_check(String s)
        {
            Either<Parser.Error, Tuple<string, Token.Builder.CheckBuilder>> res =
                    Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.ParserError(res.Left));
            }

            Tuple<string, Token.Builder.CheckBuilder> t = res.Right;

            add_authority_check(t.Item2);

            return new Right(null);
        }

        public void set_context(String context)
        {
            this.context = context;
        }

        public Either<Error, Token.Biscuit> build()
        {
            SymbolTable base_symbols = new SymbolTable();
            SymbolTable symbols = new SymbolTable();

            for (int i = 0; i < this.symbol_start; i++)
            {
                base_symbols.Add(this.symbols.Symbols[i]);
            }

            for (int i = this.symbol_start; i < this.symbols.Symbols.Count; i++)
            {
                symbols.Add(this.symbols.Symbols[i]);
            }

            Token.Block authority_block = new Token.Block(0, symbols, context, this.facts, this.rules, this.checks);
            return Biscuit.Make(this.rng, this.root, base_symbols, authority_block);
        }

        public void add_right(string resource, string right)
        {
            this.add_authority_fact(Utils.Fact("right", Arrays.AsList(Utils.Symbol("authority"), Utils.Strings(resource), Utils.Symbol(right))));
        }
    }

}
