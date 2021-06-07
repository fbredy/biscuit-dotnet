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
        public long Index { get; }
        public SymbolTable Symbols { get; }
        public string Context { get; }

        public List<Fact> Facts { get; }
        public List<Rule> Rules { get; }
        public List<Check> Checks { get; }
        public uint Version { get; }

        /// <summary>
        /// creates a new block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="base_symbols"></param>
        public Block(long index, SymbolTable base_symbols)
        {
            this.Index = index;
            this.Symbols = base_symbols;
            this.Context = string.Empty;
            this.Facts = new List<Fact>();
            this.Rules = new List<Rule>();
            this.Checks = new List<Check>();
            this.Version = SerializedBiscuit.MAX_SCHEMA_VERSION;
        }

        /// <summary>
        /// creates a new block
        /// </summary>
        /// <param name="index"></param>
        /// <param name="base_symbols"></param>
        /// <param name="context"></param>
        /// <param name="facts"></param>
        /// <param name="rules"></param>
        /// <param name="checks"></param>
        public Block(long index, SymbolTable base_symbols, string context, List<Fact> facts, List<Rule> rules, List<Check> checks)
        {
            this.Index = index;
            this.Symbols = base_symbols;
            this.Context = context;
            this.Facts = facts;
            this.Rules = rules;
            this.Checks = checks;
            this.Version = SerializedBiscuit.MAX_SCHEMA_VERSION;
        }

        /// <summary>
        /// pretty printing for a block
        /// </summary>
        /// <param name="symbol_table"></param>
        /// <returns></returns>
        public string Print(SymbolTable symbol_table)
        {
            StringBuilder s = new StringBuilder();

            s.Append("Block[");
            s.Append(this.Index);
            s.Append("] {\n\t\tsymbols: ");
            s.Append(this.Symbols.Symbols);
            s.Append("\n\t\tcontext: ");
            s.Append(this.Context);
            s.Append("\n\t\tfacts: [");
            foreach (Fact f in this.Facts)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.PrintFact(f));
            }
            s.Append("\n\t\t]\n\t\trules: [");
            foreach (Rule r in this.Rules)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.PrintRule(r));
            }
            s.Append("\n\t\t]\n\t\tchecks: [");
            foreach (Check c in this.Checks)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.PrintCheck(c));
            }
            s.Append("\n\t\t]\n\t}");

            return s.ToString();
        }

        /// <summary>
        /// Serializes a Block to its Protobuf representation
        /// </summary>
        /// <returns></returns>
        public Format.Schema.Block Serialize()
        {
            Format.Schema.Block block = new Format.Schema.Block()
            {
                Index = (uint)this.Index,
            };

            block.Symbols.AddRange(this.Symbols.Symbols);

            if (this.Context.Any())
            {
                block.Context = this.Context;
            }

            foreach (var fact in Facts)
            {
                block.FactsV1.Add(fact.Serialize());
            }


            foreach (Rule rule in this.Rules)
            {
                block.RulesV1.Add(rule.Serialize());
            }

            foreach (Check check in this.Checks)
            {
                block.ChecksV1.Add(check.Serialize());
            }

            block.Version = SerializedBiscuit.MAX_SCHEMA_VERSION;
            return block;
        }

        /// <summary>
        /// Deserializes a block from its Protobuf representation
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        static public Either<FormatError, Block> Deserialize(Format.Schema.Block b)
        {
            uint version = b.Version;
            if (version > SerializedBiscuit.MAX_SCHEMA_VERSION)
            {
                return new VersionError(SerializedBiscuit.MAX_SCHEMA_VERSION, version);
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
                    Either<FormatError, Fact> res = Fact.DeserializeV0(fact);
                    if (res.IsLeft)
                    {
                        return res.Left;
                    }
                    else
                    {
                        facts.Add(res.Right);
                    }
                }


                foreach (Format.Schema.RuleV0 rule in b.RulesV0)
                {
                    Either<FormatError, Rule> res = Rule.DeserializeV0(rule);
                    if (res.IsLeft)
                    {
                        return res.Left;
                    }
                    else
                    {
                        rules.Add(res.Right);
                    }
                }


                foreach (Format.Schema.CaveatV0 caveat in b.CaveatsV0)
                {
                    Either<FormatError, Check> res = Check.DeserializeV0(caveat);
                    if (res.IsLeft)
                    {
                        return res.Left;
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
                    Either<FormatError, Fact> res = Fact.DeserializeV1(fact);
                    if (res.IsLeft)
                    {
                        return res.Left;
                    }
                    else
                    {
                        facts.Add(res.Right);
                    }
                }


                foreach (Format.Schema.RuleV1 rule in b.RulesV1)
                {
                    Either<FormatError, Rule> res = Rule.DeserializeV1(rule);
                    if (res.IsLeft)
                    {
                        return res.Left;
                    }
                    else
                    {
                        rules.Add(res.Right);
                    }
                }


                foreach (Format.Schema.CheckV1 check in b.ChecksV1)
                {
                    Either<FormatError, Check> res = Check.DeserializeV1(check);
                    if (res.IsLeft)
                    {
                        return res.Left;
                    }
                    else
                    {
                        checks.Add(res.Right);
                    }
                }
            }

            return new Right(new Block(b.Index, symbols, b.Context, facts, rules, checks));
        }

        /// <summary>
        /// Deserializes a Block from a byte array
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        static public Either<FormatError, Block> FromBytes(byte[] slice)
        {
            try
            {
                Format.Schema.Block data = Format.Schema.Block.Parser.ParseFrom(slice);
                return Block.Deserialize(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                return new DeserializationError(e.ToString());
            }
        }

        public Either<FormatError, byte[]> ToBytes()
        {
            Format.Schema.Block b = this.Serialize();
            try
            {
                using MemoryStream stream = new MemoryStream();
                using CodedOutputStream codedStream = new CodedOutputStream(stream);
                b.WriteTo(codedStream);
                codedStream.Flush();
                return stream.ToArray();
            }
            catch (IOException e)
            {
                return new SerializationError(e.ToString());
            }
        }
    }
}
