using Biscuit.Crypto;
using Biscuit.Datalog;
using Biscuit.Errors;
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
        public Block authority { get; }
        public List<Block> blocks { get; }
        public SymbolTable symbols { get; }
        Option<SerializedBiscuit> container;
        List<byte[]> revocation_ids;


        /**
         * Creates a token builder
         *
         * this function uses the default symbol table
         *
         * @param rng random number generator
         * @param root root private key
         * @return
         */
        public static Token.Builder.BiscuitBuilder builder(RNGCryptoServiceProvider rng, KeyPair root)
        {
            return new Token.Builder.BiscuitBuilder(rng, root, default_symbol_table());
        }

        /**
         * Creates a token builder
         *
         * @param rng random number generator
         * @param root root private key
         * @param symbols symbol table
         * @return
         */
        public static Builder.BiscuitBuilder builder(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable symbols)
        {
            return new Builder.BiscuitBuilder(rng, root, symbols);
        }

        /**
         * Creates a token
         * @param rng random number generator
         * @param root root private key
         * @param authority authority block
         * @return
         */
        static public Either<Error, Biscuit> make(RNGCryptoServiceProvider rng, KeyPair root, SymbolTable symbols, Block authority)
        {
            if (!Collections.disjoint(symbols.symbols, authority.symbols.symbols))
            {
                return new Left(new SymbolTableOverlap());
            }

            if (authority.index != 0)
            {
                return new Left(new InvalidAuthorityIndex(authority.index));
            }

            symbols.symbols.AddRange(authority.symbols.symbols);
            List<Block> blocks = new List<Block>();

            Either<FormatError, SerializedBiscuit> container = SerializedBiscuit.make(rng, root, authority);
            if (container.IsLeft)
            {
                FormatError e = container.Left;
                return new Left(e);
            }
            else
            {
                SerializedBiscuit s = container.Right;
                List<byte[]> revocation_ids = s.revocation_identifiers();

                Option<SerializedBiscuit> c = Option.some(s);
                return new Right(new Biscuit(authority, blocks, symbols, c, revocation_ids));
            }
        }

        Biscuit(Block authority, List<Block> blocks, SymbolTable symbols, Option<SerializedBiscuit> container, List<byte[]> revocation_ids)
        {
            this.authority = authority;
            this.blocks = blocks;
            this.symbols = symbols;
            this.container = container;
            this.revocation_ids = revocation_ids;
        }

        /**
         * Deserializes a Biscuit token from a hex string
         *
         * This checks the signature, but does not verify that the first key is the root key,
         * to allow appending blocks without knowing about the root key.
         *
         * The root key check is performed in the verify method
         *
         * This method uses the default symbol table
         * @param data
         * @return
         */
        public static Either<Error, Biscuit> from_b64(string data)
        {
            return Biscuit.from_bytes(Convert.FromBase64String(data));
        }

        /**
         * Deserializes a Biscuit token from a byte array
         *
         * This checks the signature, but does not verify that the first key is the root key,
         * to allow appending blocks without knowing about the root key.
         *
         * The root key check is performed in the verify method
         *
         * This method uses the default symbol table
         * @param data
         * @return
         */
        static public Either<Error, Biscuit> from_bytes(byte[] data)
        {
            return Biscuit.from_bytes_with_symbols(data, default_symbol_table());
        }

        /**
         * Deserializes a Biscuit token from a byte array
         *
         * This checks the signature, but does not verify that the first key is the root key,
         * to allow appending blocks without knowing about the root key.
         *
         * The root key check is performed in the verify method
         * @param data
         * @return
         */
        static public Either<Error, Biscuit> from_bytes_with_symbols(byte[] data, SymbolTable symbols)
        {
            Either<Error, SerializedBiscuit> res = SerializedBiscuit.from_bytes(data);
            if (res.IsLeft)
            {
                Error e = res.Left;
                return new Left(e);
            }

            SerializedBiscuit ser = res.Right;
            Either<FormatError, Block> authRes = Block.from_bytes(ser.authority);
            if (authRes.IsLeft)
            {
                Error e = authRes.Left;
                return new Left(e);
            }
            Block authority = authRes.Right;

            List<Block> blocks = new List<Block>();
            foreach (byte[] bdata in ser.blocks)
            {
                Either<FormatError, Block> blockRes = Block.from_bytes(bdata);
                if (blockRes.IsLeft)
                {
                    Error e = blockRes.Left;
                    return new Left(e);
                }
                blocks.Add(blockRes.Right);
            }

            foreach (string s in authority.symbols.symbols)
            {
                symbols.Add(s);
            }

            foreach (Block b in blocks)
            {
                foreach (String s in b.symbols.symbols)
                {
                    symbols.Add(s);
                }
            }

            List<byte[]> revocation_ids = ser.revocation_identifiers();

            return new Right(new Biscuit(authority, blocks, symbols, Option.some(ser), revocation_ids));
        }

        /**
         * Creates a verifier for this token
         *
         * This function checks that the root key is the one we expect
         * @param root root public key
         * @return
         */
        public Either<Error, Verifier> verify(PublicKey root)
        {
            return Verifier.make(this, Option.some(root));
        }

        public Either<Error, Verifier> verify_sealed()
        {
            return Verifier.make(this, Option<PublicKey>.none());
        }

        /**
         * Serializes a token to a byte array
         * @return
         */
        public Either<Error, byte[]> serialize()
        {
            if (this.container.isEmpty())
            {
                return new Left(new SerializationError("no internal container"));
            }
            return this.container.get().serialize();
        }

        /**
         * Serializes a token to a base 64 String
         * @return
         */
        public Either<Error, string> serialize_b64()
        {
            var serialized = serialize();
            if (serialized.IsLeft)
            {
                return new Left(serialized.Left);
            }
            
            return new Right(Convert.ToBase64String(serialized.Right));
        }

        public static Either<Error, Biscuit> from_sealed(byte[] data, byte[] secret)
        {
            //FIXME: needs a version of from_sealed with custom symbol table support
            SymbolTable symbols = default_symbol_table();

            Either<Error, SealedBiscuit> res = SealedBiscuit.from_bytes(data, secret);
            if (res.IsLeft)
            {
                Error e = res.Left;
                return new Left(e);
            }

            SealedBiscuit ser = res.Right;
            Either<FormatError, Block> authRes = Block.from_bytes(ser.authority);
            if (authRes.IsLeft)
            {
                Error e = authRes.Left;
                return new Left(e);
            }
            Block authority = authRes.Right;

            List<Block> blocks = new List<Block>();
            foreach (byte[] bdata in ser.blocks)
            {
                Either<FormatError, Block> blockRes = Block.from_bytes(bdata);
                if (blockRes.IsLeft)
                {
                    Error e = blockRes.Left;
                    return new Left(e);
                }
                blocks.Add(blockRes.Right);
            }

            foreach (string s in authority.symbols.symbols)
            {
                symbols.Add(s);
            }

            foreach (Block b in blocks)
            {
                foreach (string s in b.symbols.symbols)
                {
                    symbols.Add(s);
                }
            }

            List<byte[]> revocation_ids = ser.revocation_identifiers();

            return new Right(new Biscuit(authority, blocks, symbols, Option<SerializedBiscuit>.none(), revocation_ids));
        }

        public Either<FormatError, byte[]> seal(byte[] secret)
        {
            Either<FormatError, SealedBiscuit> res = SealedBiscuit.make(authority, blocks, secret);
            if (res.IsLeft)
            {
                FormatError e = res.Left;
                return new Left(e);
            }

            SealedBiscuit b = res.Right;
            return b.serialize();
        }

        public bool is_sealed()
        {
            return this.container.isEmpty();
        }

        /**
         * Verifies that a token is valid for a root public key
         * @param public_key
         * @return
         */
        public Either<Error, Void> check_root_key(PublicKey public_key)
        {
            if (this.container.isEmpty())
            {
                return new Left(new Sealed());
            }
            else
            {
                return this.container.get().check_root_key(public_key);
            }
        }

        public Either<Error, Datalog.World> generate_world()
        {
            Datalog.World world = new Datalog.World();
            ulong authority_index = symbols.get("authority").get();
            ulong ambient_index = symbols.get("ambient").get();

            foreach (Fact fact in this.authority.facts)
            {
                world.add_fact(fact);
            }

            foreach (Rule rule in this.authority.rules)
            {
                world.add_privileged_rule(rule);
            }

            for (int i = 0; i < this.blocks.Count; i++)
            {
                Block b = this.blocks[i];
                if (b.index != i + 1)
                {
                    return new Left(new InvalidBlockIndex(1 + this.blocks.Count, this.blocks[i].index));
                }

                foreach (Fact fact in b.facts)
                {
                    if (fact.predicate.ids[0].Equals(new ID.Symbol(authority_index)) ||
                            fact.predicate.ids[0].Equals(new ID.Symbol(ambient_index)))
                    {
                        return new Left(new FailedLogic(new LogicError.InvalidBlockFact(i, symbols.print_fact(fact))));
                    }

                    world.add_fact(fact);
                }

                foreach (Rule rule in b.rules)
                {
                    world.add_rule(rule);
                }
            }

            List<byte[]> revocation_ids = this.revocation_identifiers();
            ulong rev = symbols.get("revocation_id").get();
            for (int i = 0; i < revocation_ids.Count; i++)
            {
                byte[] id = revocation_ids[i];
                world.add_fact(new Fact(new Predicate(rev, Arrays.asList<ID>(new ID.Integer(i), new ID.Bytes(id)))));
            }

            return new Right(world);
        }

        public Either<Error, Dictionary<string, HashSet<Fact>>> check(SymbolTable symbols, List<Fact> ambient_facts, List<Rule> ambient_rules,
                                                        List<Check> verifier_checks, Dictionary<string, Rule> queries)
        {
            Either<Error, World> wres = this.generate_world();

            if (wres.IsLeft)
            {
                Error e = wres.Left;
                return new Left(e);
            }

            World world = wres.Right;

            foreach (Fact fact in ambient_facts)
            {
                world.add_fact(fact);
            }

            foreach (Rule rule in ambient_rules)
            {
                world.add_privileged_rule(rule);
            }

            HashSet<ulong> restricted_symbols = new HashSet<ulong>();
            restricted_symbols.Add(symbols.get("authority").get());
            restricted_symbols.Add(symbols.get("ambient").get());
            //System.out.println("world after adding ambient rules:\n"+symbols.print_world(world));
            world.run(restricted_symbols);
            //System.out.println("world after running rules:\n"+symbols.print_world(world));

            List<FailedCheck> errors = new List<FailedCheck>();
            for (int j = 0; j < this.authority.checks.Count; j++)
            {
                bool successful = false;
                Check c = this.authority.checks[j];

                for (int k = 0; k < c.queries.Count; k++)
                {
                    HashSet<Fact> res = world.query_rule(c.queries[k]);
                    if (res.Any())
                    {
                        successful = true;
                        break;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedBlock(0, j, symbols.print_check(this.authority.checks[j])));
                }
            }

            for (int j = 0; j < verifier_checks.Count; j++)
            {
                bool successful = false;
                Check c = verifier_checks[j];

                for (int k = 0; k < c.queries.Count; k++)
                {
                    HashSet<Fact> res = world.query_rule(c.queries[k]);
                    if (res.Any())
                    {
                        successful = true;
                        break;
                    }
                }

                if (!successful)
                {
                    errors.Add(new FailedCheck.FailedVerifier(j, symbols.print_check(verifier_checks[j])));
                }
            }

            for (int i = 0; i < this.blocks.Count; i++)
            {
                Block b = this.blocks[i];

                for (int j = 0; j < b.checks.Count; j++)
                {
                    bool successful = false;
                    Check c = b.checks[j];

                    for (int k = 0; k < c.queries.Count; k++)
                    {
                        HashSet<Fact> res = world.query_rule(c.queries[k]);
                        if (res.Any())
                        {
                            successful = true;
                            break;
                        }
                    }

                    if (!successful)
                    {
                        errors.Add(new FailedCheck.FailedBlock(b.index, j, symbols.print_check(b.checks[j])));
                    }
                }
            }

            Dictionary<string, HashSet<Fact>> query_results = new Dictionary<string, HashSet<Fact>>();
            foreach (string name in queries.Keys)
            {
                HashSet<Fact> res = world.query_rule(queries[name]);
                query_results.Add(name, res);
            }

            if (errors.Count == 0)
            {
                return new Right(query_results);
            }
            else
            {
                return new Left(new FailedLogic(new LogicError.FailedChecks(errors)));
            }
        }

        /**
         * Creates a Block builder
         * @return
         */
        public Builder.Block create_block()
        {
            return new Builder.Block(1 + this.blocks.Count, new SymbolTable(this.symbols));
        }

        /**
         * Generates a new token from an existing one and a new block
         * @param rng random number generator
         * @param keypair ephemeral key pair
         * @param block new block (should be generated from a Block builder)
         * @return
         */
        public Either<Error, Biscuit> attenuate(RNGCryptoServiceProvider rng, KeyPair keypair, Block block)
        {
            Either<Error, Biscuit> e = this.copy();

            if (e.IsLeft)
            {
                return new Left(e.Left);
            }

            Biscuit copiedBiscuit = e.Right;

            if (!Collections.disjoint(copiedBiscuit.symbols.symbols, block.symbols.symbols))
            {
                return new Left(new SymbolTableOverlap());
            }

            if (block.index != 1 + this.blocks.Count)
            {
                return new Left(new InvalidBlockIndex(1 + copiedBiscuit.blocks.Count, block.index));
            }

            Either<FormatError, SerializedBiscuit> containerRes = copiedBiscuit.container.get().append(rng, keypair, block);
            if (containerRes.IsLeft)
            {
                FormatError error = containerRes.Left;
                return new Left(error);
            }
            SerializedBiscuit container = containerRes.Right;

            SymbolTable symbols = new SymbolTable(copiedBiscuit.symbols);
            foreach (string s in block.symbols.symbols)
            {
                symbols.Add(s);
            }

            List<Block> blocks = new List<Block>();
            foreach (Block b in copiedBiscuit.blocks)
            {
                blocks.Add(b);
            }
            blocks.Add(block);

            List<byte[]> revocation_ids = container.revocation_identifiers();

            return new Right(new Biscuit(copiedBiscuit.authority, blocks, symbols, Option.some(container), revocation_ids));
        }

        public List<List<Datalog.Check>> checks()
        {
            List<List<Datalog.Check>> l = new List<List<Datalog.Check>>();
            l.Add(new List<Datalog.Check>(this.authority.checks));

            foreach (Block b in this.blocks)
            {
                l.Add(new List<Datalog.Check>(b.checks));
            }

            return l;
        }

        public List<byte[]> revocation_identifiers()
        {
            return this.revocation_ids;
        }

        public List<Option<string>> context()
        {
            List<Option<string>> res = new List<Option<string>>();
            if (this.authority.context.Length == 0)
            {
                res.Add(Option<string>.none());
            }
            else
            {
                res.Add(Option.some(this.authority.context));
            }

            foreach (Block b in this.blocks)
            {
                if (b.context.Length == 0)
                {
                    res.Add(Option<string>.none());
                }
                else
                {
                    res.Add(Option.some(b.context));
                }
            }

            return res;
        }

        /**
         * Prints a token's content
         */
        public string print()
        {
            StringBuilder s = new StringBuilder();
            s.Append("Biscuit {\n\tsymbols: ");
            s.Append(this.symbols.symbols);
            s.Append("\n\tauthority: ");
            s.Append(this.authority.print(this.symbols));
            s.Append("\n\tblocks: [\n");
            foreach (Block b in this.blocks)
            {
                s.Append("\t\t");
                s.Append(b.print(this.symbols));
                s.Append("\n");
            }
            s.Append("\t]\n}");

            return s.ToString();
        }

        /**
         * Default symbols list
         */
        static public SymbolTable default_symbol_table()
        {
            SymbolTable syms = new SymbolTable();
            syms.insert("authority");
            syms.insert("ambient");
            syms.insert("resource");
            syms.insert("operation");
            syms.insert("right");
            syms.insert("current_time");
            syms.insert("revocation_id");

            return syms;
        }

        public Either<Error, Biscuit> copy()
        {
            var serialized = this.serialize();
            if(serialized.IsLeft)
            {
                return new Left(serialized.Left);
            }

            return Biscuit.from_bytes(serialized.Right);
        }
    }
}
