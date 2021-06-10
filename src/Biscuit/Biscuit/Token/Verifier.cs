using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Token
{
    /// <summary>
    /// Token verification class
    /// </summary>
    public class Verifier
    {
        readonly Biscuit token;
        readonly List<CheckBuilder> checks;
        readonly List<List<Check>> token_checks;
        readonly List<Policy> policies;
        readonly World world;
        readonly SymbolTable symbols;

        private Verifier(Biscuit token, World w)
        {
            this.token = token;
            this.world = w;
            this.symbols = new SymbolTable(this.token.Symbols);
            this.checks = new List<CheckBuilder>();
            this.policies = new List<Policy>();
            this.token_checks = this.token.Checks();
        }

        /// <summary>
        /// Creates an empty verifier
        /// used to apply policies when unauthenticated(no token)
        /// and to preload a verifier that is cloned for each new request
        /// </summary>
        public Verifier()
        {
            this.world = new World();
            this.symbols = Biscuit.DefaultSymbolTable();
            this.checks = new List<CheckBuilder>();
            this.policies = new List<Policy>();
            this.token_checks = new List<List<Datalog.Check>>();
        }

        Verifier(Biscuit token, List<CheckBuilder> checks, List<Policy> policies,
                 List<List<Datalog.Check>> token_checks, World world, SymbolTable symbols)
        {
            this.token = token;
            this.checks = checks;
            this.policies = policies;
            this.token_checks = token_checks;
            this.world = world;
            this.symbols = symbols;
        }


        static public Either<Error, Verifier> Make(Biscuit token, Option<PublicKey> root)
        {
            if (!token.IsSealed())
            {
                Either<Error, Void> checkRootKey = token.CheckRootKey(root.Get());
                if (checkRootKey.IsLeft)
                {
                    return checkRootKey.Left;
                }
            }

            Either<Error, World> world = token.GenerateWorld();
            if (world.IsLeft)
            {
                return world.Left;
            }

            return new Right(new Verifier(token, world.Right));
        }

        public Verifier Clone()
        {
            return new Verifier(this.token, new List<CheckBuilder>(this.checks), new List<Policy>(this.policies),
                    new List<List<Datalog.Check>>(this.token_checks), new World(this.world), new SymbolTable(this.symbols));
        }

        public Either<Error, Void> AddToken(Biscuit token, Option<PublicKey> root)
        {
            if (!token.IsSealed())
            {
                Either<Error, Void> res = token.CheckRootKey(root.Get());
                if (res.IsLeft)
                {
                    return res.Left;
                }
            }

            if (this.token != null)
            {
                return new FailedLogic(new LogicError.VerifierNotEmpty());
            }

            ulong authority_index = symbols.Get("authority").Get();
            ulong ambient_index = symbols.Get("ambient").Get();

            foreach (Fact fact in token.Authority.Facts)
            {
                if (fact.Predicate.Ids[0].Equals(new ID.Symbol(ambient_index)))
                {
                    return new FailedLogic(new LogicError.InvalidAuthorityFact(symbols.PrintFact(fact)));
                }

                Fact converted_fact = FactBuilder.ConvertFrom(fact, token.Symbols).Convert(this.symbols);
                world.AddFact(converted_fact);
            }

            foreach (Rule rule in token.Authority.Rules)
            {
                Rule converted_rule = RuleBuilder.ConvertFrom(rule, token.Symbols).Convert(this.symbols);
                world.AddPrivilegedRule(converted_rule);
            }

            List<Check> authority_checks = new List<Check>();
            foreach (Check check in token.Authority.Checks)
            {
                Datalog.Check converted_check = CheckBuilder.ConvertFrom(check, token.Symbols).Convert(this.symbols);
                authority_checks.Add(converted_check);
            }
            token_checks.Add(authority_checks);

            for (int i = 0; i < token.Blocks.Count; i++)
            {
                Block b = token.Blocks[i];
                if (b.Index != i + 1)
                {
                    return new InvalidBlockIndex(1 + token.Blocks.Count, token.Blocks[i].Index);
                }

                foreach (Fact fact in b.Facts)
                {
                    if (fact.Predicate.Ids[0].Equals(new ID.Symbol(authority_index)) ||
                            fact.Predicate.Ids[0].Equals(new ID.Symbol(ambient_index)))
                    {
                        return new FailedLogic(new LogicError.InvalidBlockFact(i, symbols.PrintFact(fact)));
                    }

                    Fact converted_fact = FactBuilder.ConvertFrom(fact, token.Symbols).Convert(this.symbols);
                    world.AddFact(converted_fact);
                }

                foreach (Rule rule in b.Rules)
                {
                    Rule converted_rule = RuleBuilder.ConvertFrom(rule, token.Symbols).Convert(this.symbols);
                    world.AddRule(converted_rule);
                }

                List<Check> block_checks = new List<Check>();
                foreach (Check check in b.Checks)
                {
                    Check converted_check = CheckBuilder.ConvertFrom(check, token.Symbols).Convert(this.symbols);
                    block_checks.Add(converted_check);
                }
                token_checks.Add(block_checks);
            }

            List<RevocationIdentifier> revocation_ids = token.RevocationIdentifiers;
            ulong rev = symbols.Get("revocation_id").Get();
            for (int i = 0; i < revocation_ids.Count; i++)
            {
                byte[] id = revocation_ids[i].Bytes;
                world.AddFact(new Fact(new Predicate(rev, Arrays.AsList<ID>(new ID.Integer(i), new ID.Bytes(id)))));
            }

            return new Right(null);
        }

        public void AddFact(FactBuilder fact)
        {
            world.AddFact(fact.Convert(symbols));
        }

        public Either<Error, Void> AddFact(string s)
        {
            Either<Builder.Parser.Error, Tuple<string, FactBuilder>> res =
                    Builder.Parser.Parser.Fact(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, FactBuilder> t = res.Right;

            AddFact(t.Item2);

            return new Right(null);
        }

        public void AddRule(RuleBuilder rule)
        {
            world.AddPrivilegedRule(rule.Convert(symbols));
        }

        public Either<Error, Void> AddRule(string s)
        {
            Either<Builder.Parser.Error, Tuple<string, RuleBuilder>> res =
                    Builder.Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, RuleBuilder> t = res.Right;

            AddRule(t.Item2);

            return new Right(null);
        }

        public void AddCheck(CheckBuilder check)
        {
            this.checks.Add(check);
            world.AddCheck(check.Convert(symbols));
        }

        public Either<Error, Void> AddCheck(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, CheckBuilder>> res =
                    Token.Builder.Parser.Parser.Check(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, CheckBuilder> t = res.Right;

            AddCheck(t.Item2);

            return new Right(null);
        }

        public void AddResource(string resource)
        {
            world.AddFact(Utils.Fact("resource", Arrays.AsList(Utils.Symbol("ambient"), Utils.Strings(resource))).Convert(symbols));
        }

        public void AddOperation(string operation)
        {
            world.AddFact(Utils.Fact("operation", Arrays.AsList(Utils.Symbol("ambient"), Utils.Symbol(operation))).Convert(symbols));
        }

        public void SetTime()
        {

            world.AddFact(Utils.Fact("time", Arrays.AsList(Utils.Symbol("ambient"), Utils.Date(DateTime.Now))).Convert(symbols));
        }

        public void RevocationCheck(List<long> ids)
        {
            List<RuleBuilder> q = new List<RuleBuilder>();

            var termIds = ids.Select(id => new Term.Integer(id)).ToHashSet<Term>();

            q.Add(Utils.ConstrainedRule(
                    "revocation_check",
                    Arrays.AsList(Utils.Var("id")),
                    Arrays.AsList(Utils.Pred("revocation_id", Arrays.AsList(Utils.Var("id")))),
                    Arrays.AsList<ExpressionBuilder>(
                            new ExpressionBuilder.Unary(
                                    ExpressionBuilder.Op.Negate,
                                    new ExpressionBuilder.Binary(
                                            ExpressionBuilder.Op.Contains,
                                            new ExpressionBuilder.Value(Utils.Var("id")),
                                            new ExpressionBuilder.Value(new Term.Set(termIds))
                                    )
                            )
                    )
            ));

            this.checks.Add(new CheckBuilder(q));
        }

        public Either<Error, List<string>> GetRevocationIdentifiers()
        {
            List<string> ids = new List<string>();

            Builder.RuleBuilder getRevocationIds = Utils.Rule(
                    "revocation_id",
                    Arrays.AsList(Utils.Var("id")),
                    Arrays.AsList(Utils.Pred("revocation_id", Arrays.AsList(Utils.Var("id"))))
            );

            Either<Error, HashSet<FactBuilder>> queryRes = this.Query(getRevocationIds);
            if (queryRes.IsLeft)
            {
                return queryRes.Left;
            }

            foreach (var fact in queryRes.Right)
            {
                foreach (var id in fact.Ids.Where(id => id is Term.Str))
                {
                    ids.Add(((Term.Str)id).Value);
                }
            }

            return ids;
        }

        public void Allow()
        {
            List<RuleBuilder> q = new List<RuleBuilder>
            {
                Utils.ConstrainedRule(
                    "allow",
                    new List<Term>(),
                    new List<PredicateBuilder>(),
                    Arrays.AsList<ExpressionBuilder>(new ExpressionBuilder.Value(new Term.Bool(true)))
            )
            };

            this.policies.Add(new Policy(q, Policy.Kind.Allow));
        }

        public void Deny()
        {
            List<RuleBuilder> q = new List<RuleBuilder>
            {
                Utils.ConstrainedRule(
                    "deny",
                    new List<Term>(),
                    new List<PredicateBuilder>(),
                    Arrays.AsList<ExpressionBuilder>(new ExpressionBuilder.Value(new Term.Bool(true)))
            )
            };

            this.policies.Add(new Policy(q, Policy.Kind.Deny));
        }

        public Either<Error, Void> AddPolicy(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, Token.Policy>> res =
                    Token.Builder.Parser.Parser.Policy(s);

            if (res.IsLeft)
            {
                return new Left(new ParserError(res.Left));
            }

            Tuple<string, Token.Policy> t = res.Right;

            this.policies.Add(t.Item2);
            return new Right(null);
        }

        public Either<Error, HashSet<FactBuilder>> Query(RuleBuilder query)
        {
            return this.Query(query, new RunLimits());
        }

        public Either<Error, HashSet<FactBuilder>> Query(string s)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, RuleBuilder>> res =
                    Token.Builder.Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, RuleBuilder> t = res.Right;

            return Query(t.Item2);
        }

        public Either<Error, HashSet<FactBuilder>> Query(RuleBuilder query, RunLimits limits)
        {
            Either<Error, Void> runRes = world.Run(limits, new HashSet<ulong>());
            if (runRes.IsLeft)
            {
                Error e = runRes.Left;
                return new Left(e);
            }

            HashSet<Fact> facts = world.QueryRule(query.Convert(symbols));
            HashSet<FactBuilder> s = new HashSet<FactBuilder>();

            foreach (Fact f in facts)
            {
                s.Add(FactBuilder.ConvertFrom(f, symbols));
            }

            return new Right(s);
        }

        public Either<Error, HashSet<FactBuilder>> Query(string s, RunLimits limits)
        {
            Either<Token.Builder.Parser.Error, Tuple<string, RuleBuilder>> res =
                    Token.Builder.Parser.Parser.Rule(s);

            if (res.IsLeft)
            {
                return new ParserError(res.Left);
            }

            Tuple<string, RuleBuilder> t = res.Right;

            return Query(t.Item2, limits);
        }

        public Either<Error, long> Verify()
        {
            return this.Verify(new RunLimits());
        }

        public Either<Error, long> Verify(RunLimits limits)
        {
            DateTime timeLimit = DateTime.Now.Add(limits.MaxTime);

            if (this.symbols.Get("authority").IsEmpty() || this.symbols.Get("ambient").IsEmpty())
            {
                return new MissingSymbols();
            }

            HashSet<ulong> restricted_symbols = new HashSet<ulong>
            {
                this.symbols.Get("authority").Get(),
                this.symbols.Get("ambient").Get()
            };

            Either<Error, Void> runRes = world.Run(limits, restricted_symbols);
            if (runRes.IsLeft)
            {
                return runRes.Left;
            }

            SymbolTable symbols = new SymbolTable(this.symbols);

            List<FailedCheck> errors = new List<FailedCheck>();

            for (int j = 0; j < this.checks.Count; j++)
            {
                Datalog.Check c = this.checks[j].Convert(symbols);
                bool successful = false;

                for (int k = 0; k < c.Queries.Count && !successful; k++)
                {
                    bool res = world.TestRule(c.Queries[k]);

                    if (DateTime.Now.CompareTo(timeLimit) >= 0)
                    {
                        return new TimeoutError();
                    }

                    if (res)
                    {
                        successful = true;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedVerifier(j, symbols.PrintCheck(c)));
                }
            }


            for (int i = 0; i < this.token_checks.Count; i++)
            {
                List<Check> checks = this.token_checks[i];

                for (int j = 0; j < checks.Count ; j++)
                {
                    bool successful = false;
                    Check c = checks[j];

                    for (int k = 0; k < c.Queries.Count && !successful; k++)
                    {
                        bool res = world.TestRule(c.Queries[k]);

                        if (DateTime.Now.CompareTo(timeLimit) >= 0)
                        {
                            return new TimeoutError();
                        }

                        if (res)
                        {
                            successful = true;
                        }
                    }

                    if (!successful)
                    {
                        errors.Add(new FailedCheck.FailedBlock(i, j, symbols.PrintCheck(checks[j])));
                    }
                }
            }

            if (errors.IsEmpty())
            {
                for (int i = 0; i < this.policies.Count; i++)
                {
                    Check c = this.policies[i].Convert(symbols);
                    for (int k = 0; k < c.Queries.Count; k++)
                    {
                        bool res = world.TestRule(c.Queries[k]);

                        if (DateTime.Now.CompareTo(timeLimit) >= 0)
                        {
                            return new TimeoutError();
                        }

                        if (res)
                        {
                            if (this.policies[i].kind == Policy.Kind.Deny)
                            {
                                return new FailedLogic(new LogicError.Denied(i));
                            }
                            else
                            {
                                return new Right((long)i);
                            }
                        }
                    }
                }

                return new FailedLogic(new LogicError.NoMatchingPolicy());
            }
            else
            {
                return new FailedLogic(new LogicError.FailedChecks(errors));
            }
        }

        public string PrintWorld()
        {
            List<string> facts = this.world.Facts.Select(fact => this.symbols.PrintFact(fact)).ToList();
            List<string> rules = this.world.Rules.Select(rule => this.symbols.PrintRule(rule)).ToList();
            List<String> privileged_rules = this.world.PrivilegedRules.Select((r) => this.symbols.PrintRule(r)).ToList();

            List<string> checks = new List<string>();

            for (int j = 0; j < this.checks.Count; j++)
            {
                checks.Add("Verifier[" + j + "]: " + this.checks[j].ToString());
            }

            if (this.token != null)
            {
                for (int j = 0; j < this.token.Authority.Checks.Count; j++)
                {
                    checks.Add("Block[0][" + j + "]: " + this.symbols.PrintCheck(this.token.Authority.Checks[j]));
                }

                for (int i = 0; i < this.token.Blocks.Count; i++)
                {
                    Block block = this.token.Blocks[i];

                    for (int j = 0; j < block.Checks.Count; j++)
                    {
                        checks.Add("Block[" + i + "][" + j + "]: " + this.symbols.PrintCheck(block.Checks[j]));
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
