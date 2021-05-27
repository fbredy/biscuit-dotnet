using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token.Formatter;
using Google.Protobuf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Biscuit.Token
{
    /// <summary>
    /// Represents a token's block with its checks
    /// </summary>
    public class Block
    {
        public long index { get; }
        public SymbolTable symbols { get; }
        public string context { get; }

        public List<Fact> facts { get; }
        public List<Rule> rules { get; }
        public List<Check> checks { get; }
        readonly long version;

        /**
         * creates a new block
         * @param index
         * @param base_symbols
         */
        public Block(long index, SymbolTable base_symbols)
        {
            this.index = index;
            this.symbols = base_symbols;
            this.context = "";
            this.facts = new List<Fact>();
            this.rules = new List<Rule>();
            this.checks = new List<Check>();
            this.version = SerializedBiscuit.MAX_SCHEMA_VERSION;
        }

        /**
         * creates a new block
         * @param index
         * @param base_symbols
         * @param facts
         * @param checks
         */
        public Block(long index, SymbolTable base_symbols, string context, List<Fact> facts, List<Rule> rules, List<Check> checks)
        {
            this.index = index;
            this.symbols = base_symbols;
            this.context = context;
            this.facts = facts;
            this.rules = rules;
            this.checks = checks;
            this.version = SerializedBiscuit.MAX_SCHEMA_VERSION;
        }

        //Either<LogicError, Void> check(long i, World w, SymbolTable symbols, List<Check> verifier_checks,
        //                               Dictionary<string, Rule> queries, Dictionary<string, Dictionary<long, HashSet<Fact>>> query_results)
        //{
        //    World world = new World(w);
        //    ulong authority_index = (ulong)symbols.get("authority").get();
        //    ulong ambient_index = (ulong)symbols.get("ambient").get();

        //    foreach (Fact fact in this.facts)
        //    {
        //        if (fact.predicate.ids[0].Equals(new ID.Symbol(authority_index)) ||
        //                fact.predicate.ids[0].Equals(new ID.Symbol(ambient_index)))
        //        {
        //            return new Left(new LogicError.InvalidBlockFact(i, symbols.print_fact(fact)));
        //        }

        //        world.add_fact(fact);
        //    }

        //    foreach (Rule rule in this.rules)
        //    {
        //        world.add_rule(rule);
        //    }

        //    world.run();

        //    List<FailedCheck> errors = new List<FailedCheck>();

        //    for (int j = 0; j < this.checks.Count; j++)
        //    {
        //        bool successful = false;
        //        Check c = this.checks[j] ;

        //        for (int k = 0; k < c.queries.Count; k++)
        //        {
        //            HashSet<Fact> res = world.query_rule(c.queries[k]);
        //            if (res.Any())
        //            {
        //                successful = true;
        //                break;
        //            }
        //        }

        //        if (!successful)
        //        {
        //            errors.Add(new FailedCheck.FailedBlock(i, j, symbols.print_check(this.checks[j])));
        //        }
        //    }

        //    for (int j = 0; j < verifier_checks.Count; j++)
        //    {
        //        bool successful = false;
        //        Check c = verifier_checks[j];

        //        for (int k = 0; k < c.queries.Count; k++)
        //        {
        //            HashSet<Fact> res = world.query_rule(c.queries[k]);
        //            if (res.Any())
        //            {
        //                successful = true;
        //                break;
        //            }
        //        }

        //        if (!successful)
        //        {
        //            errors.Add(new FailedCheck.FailedVerifier(j, symbols.print_check(verifier_checks[j])));
        //        }
        //    }

        //    foreach (string name in queries.Keys)
        //    {
        //        HashSet<Fact> res = world.query_rule(queries[name]);
        //        query_results[name].Add(this.index, res);
        //    }

        //    if (errors.Count == 0)
        //    {
        //        return new Right(null);
        //    }
        //    else
        //    {
        //        return new Left(new LogicError.FailedChecks(errors));
        //    }
        //}

        /**
         * pretty printing for a block
         * @param symbol_table
         * @return
         */
        public string print(SymbolTable symbol_table)
        {
            StringBuilder s = new StringBuilder();

            s.Append("Block[");
            s.Append(this.index);
            s.Append("] {\n\t\tsymbols: ");
            s.Append(this.symbols.symbols);
            s.Append("\n\t\tcontext: ");
            s.Append(this.context);
            s.Append("\n\t\tfacts: [");
            foreach (Fact f in this.facts)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.print_fact(f));
            }
            s.Append("\n\t\t]\n\t\trules: [");
            foreach (Rule r in this.rules)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.print_rule(r));
            }
            s.Append("\n\t\t]\n\t\tchecks: [");
            foreach (Check c in this.checks)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.print_check(c));
            }
            s.Append("\n\t\t]\n\t}");

            return s.ToString();
        }

        /**
         * Serializes a Block to its Protobuf representation
         * @return
         */
        public Format.Schema.Block serialize()
        {
            Format.Schema.Block b = new Format.Schema.Block()
            {
                Index = (uint)this.index,
            };

            b.Symbols.AddRange(this.symbols.symbols);
            
            if (this.context.Any())
            {
                b.Context = this.context;
            }
            
            foreach(var fact in facts)
            {
                b.FactsV1.Add(fact.serialize());
            }

            
            foreach (Rule rule in this.rules)
            {
                b.RulesV1.Add(rule.serialize());
            }

            foreach (Check check in this.checks)
            {
                b.ChecksV1.Add(check.serialize());
            }

            b.Version = SerializedBiscuit.MAX_SCHEMA_VERSION;
            return b;
        }

        /**
         * Deserializes a block from its Protobuf representation
         * @param b
         * @return
         */
        static public Either<FormatError, Block> deserialize(Format.Schema.Block b)
        {
            uint version = b.Version;
            if (version > SerializedBiscuit.MAX_SCHEMA_VERSION)
            {
                return new Left(new VersionError(SerializedBiscuit.MAX_SCHEMA_VERSION, version));
            }

            SymbolTable symbols = new SymbolTable();
            foreach (string s in b.Symbols)
            {
                symbols.Add(s);
            }

            List<Fact> facts = new List<Fact>();
            List<Rule> rules = new List<Rule>();
            List<Check> checks = new List<Check>();

            if (version == 0)
            {
                foreach (Format.Schema.FactV0 fact in b.FactsV0)
                {
                    Either<FormatError, Fact> res = Fact.deserializeV0(fact);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        facts.Add(res.Right);
                    }
                }


                foreach (Format.Schema.RuleV0 rule in b.RulesV0)
                {
                    Either<FormatError, Rule> res = Rule.deserializeV0(rule);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        rules.Add(res.Right);
                    }
                }


                foreach (Format.Schema.CaveatV0 caveat in b.CaveatsV0)
                {
                    Either<FormatError, Check> res = Check.deserializeV0(caveat);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        checks.Add(res.Right);
                    }
                }
            }
            else
            {
                foreach (Format.Schema.FactV1 fact in b.FactsV1)
                {
                    Either<FormatError, Fact> res = Fact.deserializeV1(fact);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        facts.Add(res.Right);
                    }
                }


                foreach (Format.Schema.RuleV1 rule in b.RulesV1)
                {
                    Either<FormatError, Rule> res = Rule.deserializeV1(rule);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        rules.Add(res.Right);
                    }
                }


                foreach (Format.Schema.CheckV1 check in b.ChecksV1)
                {
                    Either<FormatError, Check> res = Check.deserializeV1(check);
                    if (res.IsLeft)
                    {
                        FormatError e = res.Left;
                        return new Left(e);
                    }
                    else
                    {
                        checks.Add(res.Right);
                    }
                }
            }

            return new Right(new Block(b.Index, symbols, b.Context, facts, rules, checks));
        }

        /**
         * Deserializes a Block from a byte array
         * @param slice
         * @return
         */
        static public Either<FormatError, Block> from_bytes(byte[] slice)
        {
            try
            {
                Format.Schema.Block data = Format.Schema.Block.Parser.ParseFrom(slice);
                return Block.deserialize(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                return new Left(new DeserializationError(e.ToString()));
            }
        }

        public Either<FormatError, byte[]> to_bytes()
        {
            Format.Schema.Block b = this.serialize();
            try
            {
                using (MemoryStream stream = new MemoryStream())
                    using(CodedOutputStream codedStream = new CodedOutputStream(stream))
                {
                    b.WriteTo(codedStream);
                    codedStream.Flush();
                    byte[] data = stream.ToArray();
                    return new Right(data);
                }                
            }
            catch (IOException e)
            {
                return new Left(new SerializationError(e.ToString()));
            }
        }
    }
}
