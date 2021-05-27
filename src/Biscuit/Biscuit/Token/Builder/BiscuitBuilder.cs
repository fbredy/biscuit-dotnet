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
            this.symbol_start = base_symbols.symbols.Count;
            this.symbols = new SymbolTable(base_symbols);
            this.context = "";
            this.facts = new List<Datalog.Fact>();
            this.rules = new List<Datalog.Rule>();
            this.checks = new List<Datalog.Check>();
        }

        public void add_authority_fact(Fact f)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (f.predicate.ids.Count == 0 || !(f.predicate.ids[0].Equals(authority_symbol)))
            {
                List<Term> ids = new List<Term>();
                ids.Add(authority_symbol);
                foreach (Term id in f.predicate.ids)
                {
                    ids.Add(id);
                }
                f.predicate.ids = ids;
            }

            this.facts.Add(f.convert(this.symbols));
        }

        public Either<Error, Void> add_authority_fact(string s)
        {
            Either<Parser.Error, Tuple<string, Fact>> res = Parser.Parser.fact(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.Parser(res.Left));
            }

            Tuple<string, Token.Builder.Fact> t = res.Right;

            add_authority_fact(t.Item2);

            return new Right(null);
        }

        public void add_authority_rule(Builder.Rule rule)
        {
            Term.Symbol authority_symbol = new Term.Symbol("authority");
            if (rule.head.ids.Count == 0 || !(rule.head.ids[0].Equals(authority_symbol)))
            {
                rule.head.ids.Insert(0, authority_symbol);
            }

            this.rules.Add(rule.convert(this.symbols));
        }

        public Either<Error, Void> add_authority_rule(String s)
        {
            Either<Builder.Parser.Error, Tuple<String, Token.Builder.Rule>> res =
                    Token.Builder.Parser.Parser.rule(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.Parser(res.Left));
            }

            Tuple<String, Token.Builder.Rule> t = res.Right;

            add_authority_rule(t.Item2);

            return new Right(null);
        }

        public void add_authority_check(Token.Builder.Check c)
        {
            this.checks.Add(c.convert(this.symbols));
        }

        public Either<Error, Void> add_authority_check(String s)
        {
            Either<Parser.Error, Tuple<string, Token.Builder.Check>> res =
                    Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return new Left(new Errors.Parser(res.Left));
            }

            Tuple<string, Token.Builder.Check> t = res.Right;

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
                base_symbols.Add(this.symbols.symbols[i]);
            }

            for (int i = this.symbol_start; i < this.symbols.symbols.Count; i++)
            {
                symbols.Add(this.symbols.symbols[i]);
            }

            Token.Block authority_block = new Token.Block(0, symbols, context, this.facts, this.rules, this.checks);
            return Biscuit.make(this.rng, this.root, base_symbols, authority_block);
        }

        public void add_right(string resource, string right)
        {
            this.add_authority_fact(Utils.fact("right", Arrays.asList(Utils.s("authority"), Utils.strings(resource), Utils.s(right))));
        }
    }

}
