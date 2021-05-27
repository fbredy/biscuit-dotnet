using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Check = Biscuit.Token.Builder.Check;

namespace Biscuit.Token
{
    /**
 * Token verification class
 */
    public class Verifier
    {
        Biscuit token;
        List<Check> checks;
        List<List<Datalog.Check>> token_checks;
        List<Policy> policies;
        World world;
        SymbolTable symbols;

        private Verifier(Biscuit token, World w)
        {
            this.token = token;
            this.world = w;
            this.symbols = new Datalog.SymbolTable(this.token.symbols);
            this.checks = new List<Check>();
            this.policies = new List<Policy>();
            this.token_checks = this.token.checks();
        }

        /**
         * Creates an empty verifier
         *
         * used to apply policies when unauthenticated (no token)
         * and to preload a verifier that is cloned for each new request
         */
        public Verifier()
        {
            this.world = new World();
            this.symbols = Biscuit.default_symbol_table();
            this.checks = new List<Check>();
            this.policies = new List<Policy>();
            this.token_checks = new List<List<Datalog.Check>>();
        }

        Verifier(Biscuit token, List<Check> checks, List<Policy> policies,
                 List<List<Datalog.Check>> token_checks, World world, SymbolTable symbols)
        {
            this.token = token;
            this.checks = checks;
            this.policies = policies;
            this.token_checks = token_checks;
            this.world = world;
            this.symbols = symbols;
        }

        /**
         * Creates a verifier for a token
         *
         * also checks that the token is valid for this root public key
         * @param token
         * @param root
         * @return
         */
        static public Either<Error, Verifier> make(Biscuit token, Option<PublicKey> root)
        {
            if (!token.is_sealed())
            {
                Either<Error, Void> checkRootKey = token.check_root_key(root.get());
                if (checkRootKey.IsLeft)
                {
                    Error e = checkRootKey.Left;
                    return new Left(e);
                }
            }

            Either<Error, World> world = token.generate_world();
            if (world.IsLeft)
            {
                Error e = world.Left;
                return new Left(e);
            }

            return new Right(new Verifier(token, world.Right));
        }

        public Verifier clone()
        {
            return new Verifier(this.token, new List<Check>(this.checks), new List<Policy>(this.policies),
                    new List<List<Datalog.Check>>(this.token_checks), new World(this.world), new SymbolTable(this.symbols));
        }

        public Either<Error, Void> add_token(Biscuit token, Option<PublicKey> root)
        {
            if (!token.is_sealed())
            {
                Either<Error, Void> res = token.check_root_key(root.get());
                if (res.IsLeft)
                {
                    Error e = res.Left;
                    return new Left(e);
                }
            }

            if (this.token != null)
            {
                return new Left(new FailedLogic(new LogicError.VerifierNotEmpty()));
            }

            ulong authority_index = symbols.get("authority").get();
            ulong ambient_index = symbols.get("ambient").get();

            foreach (Datalog.Fact fact in token.authority.facts)
            {
                if (fact.predicate.ids[0].Equals(new ID.Symbol(ambient_index)))
                {
                    return new Left(new FailedLogic(new LogicError.InvalidAuthorityFact(symbols.print_fact(fact))));
                }

                Datalog.Fact converted_fact = Builder.Fact.convert_from(fact, token.symbols).convert(this.symbols);
                world.add_fact(converted_fact);
            }

            foreach (Datalog.Rule rule in token.authority.rules)
            {
                Datalog.Rule converted_rule = Builder.Rule.convert_from(rule, token.symbols).convert(this.symbols);
                world.add_privileged_rule(converted_rule);
            }

            List<Datalog.Check> authority_checks = new List<Datalog.Check>();
            foreach (Datalog.Check check in token.authority.checks)
            {
                Datalog.Check converted_check = Check.convert_from(check, token.symbols).convert(this.symbols);
                authority_checks.Add(converted_check);
            }
            token_checks.Add(authority_checks);

            for (int i = 0; i < token.blocks.Count; i++)
            {
                Block b = token.blocks[i];
                if (b.index != i + 1)
                {
                    return new Left(new InvalidBlockIndex(1 + token.blocks.Count, token.blocks[i].index));
                }

                foreach (Datalog.Fact fact in b.facts)
                {
                    if (fact.predicate.ids[0].Equals(new ID.Symbol(authority_index)) ||
                            fact.predicate.ids[0].Equals(new ID.Symbol(ambient_index)))
                    {
                        return new Left(new FailedLogic(new LogicError.InvalidBlockFact(i, symbols.print_fact(fact))));
                    }

                    Datalog.Fact converted_fact = Builder.Fact.convert_from(fact, token.symbols).convert(this.symbols);
                    world.add_fact(converted_fact);
                }

                foreach (Datalog.Rule rule in b.rules)
                {
                    Datalog.Rule converted_rule = Builder.Rule.convert_from(rule, token.symbols).convert(this.symbols);
                    world.add_rule(converted_rule);
                }

                List<Datalog.Check> block_checks = new List<Datalog.Check>();
                foreach (Datalog.Check check in b.checks)
                {
                    Datalog.Check converted_check = Check.convert_from(check, token.symbols).convert(this.symbols);
                    block_checks.Add(converted_check);
                }
                token_checks.Add(block_checks);
            }

            List<byte[]> revocation_ids = token.revocation_identifiers();
            ulong rev = symbols.get("revocation_id").get();
            for (int i = 0; i < revocation_ids.Count; i++)
            {
                byte[] id = revocation_ids[i];
                world.add_fact(new Datalog.Fact(new Datalog.Predicate(rev, Arrays.asList<ID>(new ID.Integer(i), new ID.Bytes(id)))));
            }

            return new Right(null);
        }

        public void add_fact(Builder.Fact fact)
        {
            world.add_fact(fact.convert(symbols));
        }

        public Either<Error, Void> add_fact(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Builder.Fact>> res =
                    Token.Builder.Parser.Parser.fact(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Builder.Fact> t = res.Right;

            add_fact(t.Item2);

            return new Right(null);
        }

        public void add_rule(Builder.Rule rule)
        {
            world.add_privileged_rule(rule.convert(symbols));
        }

        public Either<Error, Void> add_rule(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Builder.Rule>> res =
                    Token.Builder.Parser.Parser.rule(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Builder.Rule> t = res.Right;

            add_rule(t.Item2);

            return new Right(null);
        }

        public void add_check(Check check)
        {
            this.checks.Add(check);
            world.add_check(check.convert(symbols));
        }

        public Either<Error, Void> add_check(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Builder.Check>> res =
                    Token.Builder.Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Builder.Check> t = res.Right;

            add_check(t.Item2);

            return new Right(null);
        }

        public void add_resource(string resource)
        {
            world.add_fact(Utils.fact("resource", Arrays.asList(Utils.s("ambient"), Utils.strings(resource))).convert(symbols));
        }

        public void add_operation(string operation)
        {
            world.add_fact(Utils.fact("operation", Arrays.asList(Utils.s("ambient"), Utils.s(operation))).convert(symbols));
        }

        public void set_time()
        {

            world.add_fact(Utils.fact("time", Arrays.asList(Utils.s("ambient"), Utils.date(DateTime.Now))).convert(symbols));
        }

        public void revocation_check(List<long> ids)
        {
            List<Builder.Rule> q = new List<Builder.Rule>();
            
            var termIds = ids.Select(id => new Term.Integer(id)).ToHashSet<Term>();

            q.Add(Utils.constrained_rule(
                    "revocation_check",
                    Arrays.asList(Utils.var("id")),
                    Arrays.asList(Utils.pred("revocation_id", Arrays.asList(Utils.var("id")))),
                    Arrays.asList<Expression>(
                            new Expression.Unary(
                                    Expression.Op.Negate,
                                    new Expression.Binary(
                                            Expression.Op.Contains,
                                            new Expression.Value(Utils.var("id")),
                                            new Expression.Value(new Term.Set(termIds))
                                    )
                            )
                    )
            ));

            this.checks.Add(new Check(q));
        }

        public Either<Error, List<string>> get_revocation_ids()
        {
            List<string> ids = new List<string>();

            Builder.Rule getRevocationIds = Utils.rule(
                    "revocation_id",
                    Arrays.asList(Utils.var("id")),
                    Arrays.asList(Utils.pred("revocation_id", Arrays.asList(Utils.var("id"))))
            );

            Either<Error, HashSet<Builder.Fact>> queryRes = this.query(getRevocationIds);
            if (queryRes.IsLeft)
            {
                Error e = queryRes.Left;
                return new Left(e);
            }

            foreach (var fact in queryRes.Right)
            {
                foreach (var id in fact.ids.Where(id => id is Term.Str))
                {
                    ids.Add(((Term.Str)id).value);
                }
            }

            return new Right(ids);
        }

        public void allow()
        {
            List<Builder.Rule> q = new List<Builder.Rule>();

            q.Add(Utils.constrained_rule(
                    "allow",
                    new List<Term>(),
                    new List<Builder.Predicate>(),
                    Arrays.asList<Expression>(new Expression.Value(new Term.Bool(true)))
            ));

            this.policies.Add(new Policy(q, Policy.Kind.Allow));
        }

        public void deny()
        {
            List<Builder.Rule> q = new List<Builder.Rule>();

            q.Add(Utils.constrained_rule(
                    "deny",
                    new List<Term>(),
                    new List<Builder.Predicate>(),
                    Arrays.asList<Expression>(new Expression.Value(new Term.Bool(true)))
            ));

            this.policies.Add(new Policy(q, Policy.Kind.Deny));
        }

        public Either<Error, Void> add_policy(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Policy>> res =
                    Token.Builder.Parser.Parser.policy(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Policy> t = res.Right;

            this.policies.Add(t.Item2);
            return new Right(null);
        }

        public Either<Error, HashSet<Builder.Fact>> query(Builder.Rule query)
        {
            return this.query(query, new RunLimits());
        }

        public Either<Error, HashSet<Builder.Fact>> query(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Builder.Rule>> res =
                    Token.Builder.Parser.Parser.rule(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Builder.Rule> t = res.Right;

            return query(t.Item2);
        }

        public Either<Error, HashSet<Builder.Fact>> query(Builder.Rule query, RunLimits limits)
        {
            Either<Error, Void> runRes = world.run(limits, new HashSet<ulong>());
            if (runRes.IsLeft)
            {
                Error e = runRes.Left;
                return new Left(e);
            }

            HashSet<Datalog.Fact> facts = world.query_rule(query.convert(symbols));
            HashSet<Builder.Fact> s = new HashSet<Builder.Fact>();

            foreach (Datalog.Fact f in facts)
            {
                s.Add(Builder.Fact.convert_from(f, symbols));
            }

            return new Right(s);
        }

        public Either<Error, HashSet<Builder.Fact>> query(string s, RunLimits limits)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Builder.Rule>> res =
                    Token.Builder.Parser.Parser.rule(s);

            if (res.IsLeft)
            {
                return new Left(new Parser(res.Left));
            }

            Tuple<string, Token.Builder.Rule> t = res.Right;

            return query(t.Item2, limits);
        }

        public Either<Error, long> verify()
        {
            return this.verify(new RunLimits());
        }

        public Either<Error, long> verify(RunLimits limits)
        {
            DateTime timeLimit = DateTime.Now.Add(limits.MaxTime);

            if (this.symbols.get("authority").isEmpty() || this.symbols.get("ambient").isEmpty())
            {
                return new Left(new MissingSymbols());
            }

            HashSet<ulong> restricted_symbols = new HashSet<ulong>();
            restricted_symbols.Add(this.symbols.get("authority").get());
            restricted_symbols.Add(this.symbols.get("ambient").get());

            Either<Error, Void> runRes = world.run(limits, restricted_symbols);
            if (runRes.IsLeft)
            {
                Error e = runRes.Left;
                return new Left(e);
            }

            SymbolTable symbols = new SymbolTable(this.symbols);

            List<FailedCheck> errors = new List<FailedCheck>();

            for (int j = 0; j < this.checks.Count; j++)
            {
                Datalog.Check c = this.checks[j].convert(symbols);
                bool successful = false;

                for (int k = 0; k < c.queries.Count; k++)
                {
                    bool res = world.test_rule(c.queries[k]);

                    if (DateTime.Now.CompareTo(timeLimit) >= 0)
                    {
                        return new Left(new Timeout());
                    }

                    if (res)
                    {
                        successful = true;
                        break;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedVerifier(j, symbols.print_check(c)));
                }
            }


            for (int i = 0; i < this.token_checks.Count; i++)
            {
                List<Datalog.Check> checks = this.token_checks[i];

                for (int j = 0; j < checks.Count; j++)
                {
                    bool successful = false;
                    Datalog.Check c = checks[j];

                    for (int k = 0; k < c.queries.Count; k++)
                    {
                        bool res = world.test_rule(c.queries[k]);

                        if (DateTime.Now.CompareTo(timeLimit) >= 0)
                        {
                            return new Left(new Timeout());
                        }

                        if (res)
                        {
                            successful = true;
                            break;
                        }
                    }

                    if (!successful)
                    {
                        errors.Add(new FailedCheck.FailedBlock(i, j, symbols.print_check(checks[j])));
                    }
                }
            }

            if (errors.isEmpty())
            {
                for (int i = 0; i < this.policies.Count; i++)
                {
                    Datalog.Check c = this.policies[i].convert(symbols);
                    bool successful = false;

                    for (int k = 0; k < c.queries.Count; k++)
                    {
                        bool res = world.test_rule(c.queries[k]);

                        if (DateTime.Now.CompareTo(timeLimit) >= 0)
                        {
                            return new Left(new Timeout());
                        }

                        if (res)
                        {
                            if (this.policies[i].kind == Policy.Kind.Deny)
                            {
                                return new Left(new FailedLogic(new LogicError.Denied(i)));
                            }
                            else
                            {
                                return new Right((long)i);
                            }
                        }
                    }
                }

                return new Left(new FailedLogic(new LogicError.NoMatchingPolicy()));
            }
            else
            {
                return new Left(new FailedLogic(new LogicError.FailedChecks(errors)));
            }
        }

        public string print_world()
        {
            List<string> facts = this.world.facts.Select(fact => this.symbols.print_fact(fact)).ToList();
            List<string> rules = this.world.rules.Select(rule => this.symbols.print_rule(rule)).ToList();
            List<String> privileged_rules = this.world.privileged_rules.Select((r) => this.symbols.print_rule(r)).ToList();

            List<string> checks = new List<string>();

            for (int j = 0; j < this.checks.Count; j++)
            {
                checks.Add("Verifier[" + j + "]: " + this.checks[j].ToString());
            }

            if (this.token != null)
            {
                for (int j = 0; j < this.token.authority.checks.Count; j++)
                {
                    checks.Add("Block[0][" + j + "]: " + this.symbols.print_check(this.token.authority.checks[j]));
                }

                for (int i = 0; i < this.token.blocks.Count; i++)
                {
                    Block block = this.token.blocks[i];

                    for (int j = 0; j < block.checks.Count; j++)
                    {
                        checks.Add("Block[" + i + "][" + j + "]: " + this.symbols.print_check(block.checks[j]));
                    }
                }
            }

            StringBuilder b = new StringBuilder();
            b.Append("World {\n\tfacts: [\n\t\t");
            b.Append(string.Join(",\n\t\t", facts));
            b.Append("\n\t],\n\tprivileged rules: [\n\t\t");
            b.Append(string.Join(",\n\t\t", privileged_rules));
            b.Append("\n\t],\n\trules: [\n\t\t");
            b.Append(string.Join(",\n\t\t", rules));
            b.Append("\n\t],\n\tchecks: [\n\t\t");
            b.Append(string.Join(",\n\t\t", checks));
            b.Append("\n\t]\n}");

            return b.ToString();
        }
    }
}
