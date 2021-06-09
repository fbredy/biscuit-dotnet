using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
using Biscuit.Token.Builder;
using Biscuit.Token.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Biscuit.Token
{
    public class Biscuit
    {
        public Block Authority { get; }
        public List<Block> Blocks { get; }
        public SymbolTable Symbols { get; }
        public List<RevocationIdentifier> RevocationIdentifiers { get; }

        private readonly Option<SerializedBiscuit> container;

        /// <summary>
        /// Creates a token builder
        /// this function uses the default symbol table
        /// </summary>
        /// <param name="rng">rng random number generator</param>
        /// <param name="root">root private key</param>
        /// <returns></returns>
        public static BiscuitBuilder Builder(RNGCryptoServiceProvider rng, KeyPair root)
        {
            return new BiscuitBuilder(rng, root, DefaultSymbolTable());
        }

        /// <summary>
        /// Creates a token builder
        /// </summary>
        /// <param name="rng">random number generator</param>
        /// <param name="root">root private key</param>
        /// <param name="symbols">symbol table</param>
        /// <returns></returns>
        public static BiscuitBuilder Builder(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable symbols)
        {
            return new BiscuitBuilder(rng, root, symbols);
        }

        /// <summary>
        /// Creates a token
        /// </summary>
        /// <param name="rng">random number generator</param>
        /// <param name="root">root private key</param>
        /// <param name="symbols">symbol table</param>
        /// <param name="authority">authority authority block</param>
        /// <returns></returns>
        static public Either<Error, Biscuit> Make(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable symbols, Block authority)
        {
            if (!Collections.Disjoint(symbols.Symbols, authority.Symbols.Symbols))
            {
                return new Left(new SymbolTableOverlap());
            }

            if (authority.Index != 0)
            {
                return new Left(new InvalidAuthorityIndex(authority.Index));
            }

            symbols.Symbols.AddRange(authority.Symbols.Symbols);
            List<Block> blocks = new List<Block>();

            Either<FormatError, SerializedBiscuit> container = SerializedBiscuit.Make(rng, root, authority);
            if (container.IsLeft)
            {
                return container.Left;
            }
            else
            {
                SerializedBiscuit s = container.Right;
                List<RevocationIdentifier> revocation_ids = s.RevocationIdentifiers();

                Option<SerializedBiscuit> c = Option.Some(s);
                return new Right(new Biscuit(authority, blocks, symbols, c, revocation_ids));
            }
        }

        Biscuit(Block authority, List<Block> blocks, SymbolTable symbols, Option<SerializedBiscuit> container, List<RevocationIdentifier> revocation_ids)
        {
            this.Authority = authority;
            this.Blocks = blocks;
            this.Symbols = symbols;
            this.container = container;
            this.RevocationIdentifiers = revocation_ids;
        }

        /// <summary>
        /// Deserializes a Biscuit token from a hex string
        /// This checks the signature, but does not verify that the first key is the root key,
        /// to allow appending blocks without knowing about the root key.
        /// The root key check is performed in the verify method
        /// This method uses the default symbol table
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Either<Error, Biscuit> FromBase64(string data)
        {
            return Biscuit.FromBytes(Convert.FromBase64String(data));
        }

        /// <summary>
        /// Deserializes a Biscuit token from a byte array
        /// This checks the signature, but does not verify that the first key is the root key,
        /// to allow appending blocks without knowing about the root key.
        /// The root key check is performed in the verify method
        /// This method uses the default symbol table
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public Either<Error, Biscuit> FromBytes(byte[] data)
        {
            return Biscuit.FromBytesWithSymbols(data, DefaultSymbolTable());
        }

        /// <summary>
        /// Deserializes a Biscuit token from a byte array
        /// This checks the signature, but does not verify that the first key is the root key,
        /// to allow appending blocks without knowing about the root key.
        /// The root key check is performed in the verify method
        /// </summary>
        /// <param name="data"></param>
        /// <param name="symbols"></param>
        /// <returns></returns>
        static public Either<Error, Biscuit> FromBytesWithSymbols(byte[] data, SymbolTable symbols)
        {
            Either<Error, SerializedBiscuit> res = SerializedBiscuit.FromBytes(data);
            if (res.IsLeft)
            {
                Error e = res.Left;
                return new Left(e);
            }

            SerializedBiscuit ser = res.Right;
            Either<FormatError, Block> authRes = Block.FromBytes(ser.authority);
            if (authRes.IsLeft)
            {
                Error e = authRes.Left;
                return new Left(e);
            }
            Block authority = authRes.Right;

            List<Block> blocks = new List<Block>();
            foreach (byte[] bdata in ser.blocks)
            {
                Either<FormatError, Block> blockRes = Block.FromBytes(bdata);
                if (blockRes.IsLeft)
                {
                    return blockRes.Left;
                }
                blocks.Add(blockRes.Right);
            }

            foreach (string s in authority.Symbols.Symbols)
            {
                symbols.Add(s);
            }

            foreach (Block b in blocks)
            {
                foreach (string s in b.Symbols.Symbols)
                {
                    symbols.Add(s);
                }
            }

            List<RevocationIdentifier> revocationIds = ser.RevocationIdentifiers();

            return new Right(new Biscuit(authority, blocks, symbols, Option.Some(ser), revocationIds));
        }

        /// <summary>
        /// Creates a verifier for this token
        /// This function checks that the root key is the one we expect
        /// </summary>
        /// <param name="root">root public key</param>
        /// <returns></returns>
        public Either<Error, Verifier> Verify(PublicKey root)
        {
            return Verifier.Make(this, Option.Some(root));
        }

        public Either<Error, Verifier> VerifySealed()
        {
            return Verifier.Make(this, Option<PublicKey>.None());
        }

        /// <summary>
        /// Serializes a token to a byte array
        /// </summary>
        /// <returns></returns>
        public Either<Error, byte[]> Serialize()
        {
            if (this.container.IsEmpty())
            {
                return new Left(new SerializationError("no internal container"));
            }
            return this.container.Get().Serialize();
        }

        /// <summary>
        /// Serializes a token to a base 64 String
        /// </summary>
        /// <returns></returns>
        public Either<Error, string> SerializeBase64()
        {
            var serialized = Serialize();
            if (serialized.IsLeft)
            {
                return new Left(serialized.Left);
            }

            return new Right(Convert.ToBase64String(serialized.Right));
        }

        public static Either<Error, Biscuit> FromSealed(byte[] data, byte[] secret)
        {
            //FIXME: needs a version of from_sealed with custom symbol table support
            SymbolTable symbols = DefaultSymbolTable();

            Either<Error, SealedBiscuit> res = SealedBiscuit.FromBytes(data, secret);
            if (res.IsLeft)
            {
                Error e = res.Left;
                return new Left(e);
            }

            SealedBiscuit ser = res.Right;
            Either<FormatError, Block> authRes = Block.FromBytes(ser.Authority);
            if (authRes.IsLeft)
            {
                Error e = authRes.Left;
                return new Left(e);
            }
            Block authority = authRes.Right;

            List<Block> blocks = new List<Block>();
            foreach (byte[] bdata in ser.Blocks)
            {
                Either<FormatError, Block> blockRes = Block.FromBytes(bdata);
                if (blockRes.IsLeft)
                {
                    return blockRes.Left;
                }
                blocks.Add(blockRes.Right);
            }

            foreach (string s in authority.Symbols.Symbols)
            {
                symbols.Add(s);
            }

            foreach (Block b in blocks)
            {
                foreach (string s in b.Symbols.Symbols)
                {
                    symbols.Add(s);
                }
            }

            List<RevocationIdentifier> revocation_ids = ser.RevocationIdentifiers();

            return new Biscuit(authority, blocks, symbols, Option<SerializedBiscuit>.None(), revocation_ids);
        }

        public Either<FormatError, byte[]> Seal(byte[] secret)
        {
            Either<FormatError, SealedBiscuit> res = SealedBiscuit.Make(Authority, Blocks, secret);
            if (res.IsLeft)
            {
                return res.Left;
            }

            SealedBiscuit b = res.Right;
            return b.Serialize();
        }

        public bool IsSealed()
        {
            return this.container.IsEmpty();
        }

        /// <summary>
        /// Verifies that a token is valid for a root public key
        /// </summary>
        /// <param name="public_key"></param>
        /// <returns></returns>
        public Either<Error, Void> CheckRootKey(PublicKey public_key)
        {
            if (this.container.IsEmpty())
            {
                return new Left(new Sealed());
            }
            else
            {
                return this.container.Get().CheckRootKey(public_key);
            }
        }

        public Either<Error, Datalog.World> GenerateWorld()
        {
            Datalog.World world = new Datalog.World();
            ulong authority_index = Symbols.Get("authority").Get();
            ulong ambient_index = Symbols.Get("ambient").Get();

            foreach (Fact fact in this.Authority.Facts)
            {
                world.AddFact(fact);
            }

            foreach (Rule rule in this.Authority.Rules)
            {
                world.AddPrivilegedRule(rule);
            }

            for (int i = 0; i < this.Blocks.Count; i++)
            {
                Block b = this.Blocks[i];
                if (b.Index != i + 1)
                {
                    return new Left(new InvalidBlockIndex(1 + this.Blocks.Count, this.Blocks[i].Index));
                }

                foreach (Fact fact in b.Facts)
                {
                    if (fact.Predicate.Ids[0].Equals(new ID.Symbol(authority_index)) ||
                            fact.Predicate.Ids[0].Equals(new ID.Symbol(ambient_index)))
                    {
                        return new Left(new FailedLogic(new LogicError.InvalidBlockFact(i, Symbols.PrintFact(fact))));
                    }

                    world.AddFact(fact);
                }

                foreach (Rule rule in b.Rules)
                {
                    world.AddRule(rule);
                }
            }

            List<RevocationIdentifier> revocation_ids = this.RevocationIdentifiers;
            ulong rev = Symbols.Get("revocation_id").Get();
            for (int i = 0; i < revocation_ids.Count; i++)
            {
                byte[] id = revocation_ids[i].Bytes ;
                world.AddFact(new Fact(new Predicate(rev, Arrays.AsList<ID>(new ID.Integer(i), new ID.Bytes(id)))));
            }

            return world;
        }

        public Either<Error, Dictionary<string, HashSet<Fact>>> Check(SymbolTable symbols, List<Fact> ambient_facts, List<Rule> ambient_rules,
                                                        List<Check> verifier_checks, Dictionary<string, Rule> queries)
        {
            Either<Error, World> wres = this.GenerateWorld();

            if (wres.IsLeft)
            {
                return wres.Left;
            }

            World world = wres.Right;

            foreach (Fact fact in ambient_facts)
            {
                world.AddFact(fact);
            }

            foreach (Rule rule in ambient_rules)
            {
                world.AddPrivilegedRule(rule);
            }

            HashSet<ulong> restricted_symbols = new HashSet<ulong>
            {
                symbols.Get("authority").Get(),
                symbols.Get("ambient").Get()
            };
            world.Run(restricted_symbols);

            List<FailedCheck> errors = new List<FailedCheck>();
            for (int j = 0; j < this.Authority.Checks.Count; j++)
            {
                bool successful = false;
                Check c = this.Authority.Checks[j];

                for (int k = 0; k < c.Queries.Count && !successful; k++)
                {
                    HashSet<Fact> res = world.QueryRule(c.Queries[k]);
                    if (res.Any())
                    {
                        successful = true;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedBlock(0, j, symbols.PrintCheck(this.Authority.Checks[j])));
                }
            }

            for (int j = 0; j < verifier_checks.Count; j++)
            {
                bool successful = false;
                Check c = verifier_checks[j];

                for (int k = 0; k < c.Queries.Count && !successful; k++)
                {
                    HashSet<Fact> res = world.QueryRule(c.Queries[k]);
                    if (res.Any())
                    {
                        successful = true;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedVerifier(j, symbols.PrintCheck(verifier_checks[j])));
                }
            }

            for (int i = 0; i < this.Blocks.Count; i++)
            {
                Block b = this.Blocks[i];

                for (int j = 0; j < b.Checks.Count; j++)
                {
                    bool successful = false;
                    Check c = b.Checks[j];

                    for (int k = 0; k < c.Queries.Count && !successful; k++)
                    {
                        HashSet<Fact> res = world.QueryRule(c.Queries[k]);
                        if (res.Any())
                        {
                            successful = true;
                        }
                    }

                    if (!successful)
                    {
                        errors.Add(new FailedCheck.FailedBlock(b.Index, j, symbols.PrintCheck(b.Checks[j])));
                    }
                }
            }

            Dictionary<string, HashSet<Fact>> queryResults = new Dictionary<string, HashSet<Fact>>();
            foreach (string name in queries.Keys)
            {
                HashSet<Fact> res = world.QueryRule(queries[name]);
                queryResults.Add(name, res);
            }

            if (errors.Count == 0)
            {
                return queryResults;
            }
            else
            {
                return new FailedLogic(new LogicError.FailedChecks(errors));
            }
        }

        /// <summary>
        /// Create a block builder
        /// </summary>
        /// <returns></returns>
        public BlockBuilder CreateBlock()
        {
            return new BlockBuilder(1 + this.Blocks.Count, new SymbolTable(this.Symbols));
        }

        /// <summary>
        /// Generates a new token from an existing one and a new block
        /// </summary>
        /// <param name="rng">random number generator</param>
        /// <param name="keypair">ephemeral key pair</param>
        /// <param name="block">new block(should be generated from a Block builder)</param>
        /// <returns></returns>
        public Either<Error, Biscuit> Attenuate(RNGCryptoServiceProvider rng, KeyPair keypair, Block block)
        {
            Either<Error, Biscuit> e = this.Copy();

            if (e.IsLeft)
            {
                return e.Left;
            }

            Biscuit copiedBiscuit = e.Right;

            if (!Collections.Disjoint(copiedBiscuit.Symbols.Symbols, block.Symbols.Symbols))
            {
                return new SymbolTableOverlap();
            }

            if (block.Index != 1 + this.Blocks.Count)
            {
                return new InvalidBlockIndex(1 + copiedBiscuit.Blocks.Count, block.Index);
            }

            Either<FormatError, SerializedBiscuit> containerRes = copiedBiscuit.container.Get().Append(rng, keypair, block);
            if (containerRes.IsLeft)
            {
                FormatError error = containerRes.Left;
                return new Left(error);
            }
            SerializedBiscuit container = containerRes.Right;

            SymbolTable symbols = new SymbolTable(copiedBiscuit.Symbols);
            foreach (string s in block.Symbols.Symbols)
            {
                symbols.Add(s);
            }

            List<Block> blocks = new List<Block>();
            foreach (Block b in copiedBiscuit.Blocks)
            {
                blocks.Add(b);
            }
            blocks.Add(block);

            List<RevocationIdentifier> revocation_ids = container.RevocationIdentifiers();

            return new Biscuit(copiedBiscuit.Authority, blocks, symbols, Option.Some(container), revocation_ids);
        }

        public List<List<Check>> Checks()
        {
            List<List<Check>> checks = new List<List<Check>>
            {
                new List<Check>(this.Authority.Checks)
            };
            checks.AddRange(this.Blocks.Select(b => new List<Check>(b.Checks)));
            return checks;
        }

        public List<Option<string>> Context()
        {
            List<Option<string>> res = new List<Option<string>>();
            if (this.Authority.Context.Length == 0)
            {
                res.Add(Option<string>.None());
            }
            else
            {
                res.Add(Option.Some(this.Authority.Context));
            }

            foreach (Block b in this.Blocks)
            {
                if (b.Context.Length == 0)
                {
                    res.Add(Option<string>.None());
                }
                else
                {
                    res.Add(Option.Some(b.Context));
                }
            }

            return res;
        }

        /// <summary>
        /// Prints a token's content
        /// </summary>
        /// <returns></returns>
        public string Print()
        {
            StringBuilder s = new StringBuilder();
            s.Append("Biscuit {\n\tsymbols: ");
            s.Append(this.Symbols.Symbols);
            s.Append("\n\tauthority: ");
            s.Append(this.Authority.Print(this.Symbols));
            s.Append("\n\tblocks: [\n");
            foreach (Block b in this.Blocks)
            {
                s.Append("\t\t");
                s.Append(b.Print(this.Symbols));
                s.Append("\n");
            }
            s.Append("\t]\n}");

            return s.ToString();
        }

        /// <summary>
        /// Default symbols list
        /// </summary>
        /// <returns></returns>
        static public SymbolTable DefaultSymbolTable()
        {
            SymbolTable syms = new SymbolTable();
            syms.Insert("authority");
            syms.Insert("ambient");
            syms.Insert("resource");
            syms.Insert("operation");
            syms.Insert("right");
            syms.Insert("current_time");
            syms.Insert("revocation_id");

            return syms;
        }

        public Either<Error, Biscuit> Copy()
        {
            var serialized = this.Serialize();
            if (serialized.IsLeft)
            {
                return serialized.Left;
            }

            return Biscuit.FromBytes(serialized.Right);
        }
    }
}
