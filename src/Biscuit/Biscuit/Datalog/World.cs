using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class World
    {
        public void add_fact(Fact fact)
        {
            this.facts.Add(fact);
        }

        public void add_rule(Rule rule)
        {
            this.rules.Add(rule);
        }

        public void add_privileged_rule(Rule rule)
        {
            this.privileged_rules.Add(rule);
        }

        public void add_check(Check check) { this.checks.Add(check); }

        public void clearRules()
        {
            this.rules.Clear();
        }

        public Either<Errors.Error, Void> run(HashSet<ulong> restricted_symbols)
        {
            return this.run(new RunLimits(), restricted_symbols);
        }

        public Either<Errors.Error, Void> run(RunLimits limits, HashSet<ulong> restricted_symbols)
        {
            int iterations = 0;
            DateTime limit = DateTime.Now.Add(limits.MaxTime);

            while (true)
            {
                HashSet<Fact> new_facts = new HashSet<Fact>();

                foreach (Rule rule in this.privileged_rules)
                {
                    rule.apply(this.facts, new_facts, new HashSet<ulong>());

                    if (DateTime.Now.CompareTo(limit) >= 0)
                    {
                        return new Errors.Timeout();
                    }
                }

                foreach (Rule rule in this.rules)
                {
                    rule.apply(this.facts, new_facts, restricted_symbols);

                    if (DateTime.Now.CompareTo(limit) >= 0)
                    {
                        return new Errors.Timeout();
                    }
                }

                int len = this.facts.Count;
                this.facts.addAll(new_facts);
                if (this.facts.Count == len)
                {
                    return new Right(null);
                }

                if (this.facts.Count >= limits.MaxFacts)
                {
                    return new Left(new Errors.TooManyFacts());
                }

                iterations += 1;
                if (iterations >= limits.MaxIterations)
                {
                    return new Left(new Errors.TooManyIterations());
                }
            }
        }

        public HashSet<Fact> facts { get; }

        public List<Rule> rules { get; }

        public List<Check> checks { get; }

        public List<Rule> privileged_rules { get; }


        public HashSet<Fact> query(Predicate pred)
        {
            var result = this.facts.Where(f =>
            {
                if (f.predicate.name != pred.name)
                {
                    return false;
                }
                int min_size = Math.Min(f.predicate.ids.Count, pred.ids.Count);
                for (int i = 0; i < min_size; ++i)
                {
                    ID fid = f.predicate.ids[i];
                    ID pid = pred.ids[i];
                    if ((fid is ID.Symbol || fid is ID.Integer || fid is ID.Str || fid is ID.Date)
                    && fid.GetType() == pid.GetType())
                    {
                        if (!fid.Equals(pid))
                        {
                            return false;
                        }
                    }
                    else if (!(fid is ID.Symbol && pid is ID.Variable))
                    {
                        return false;
                    }
                }
                return true;
            }).Distinct().ToArray();

            return new HashSet<Fact>(result);
        }

        public HashSet<Fact> query_rule(Rule rule)
        {
            HashSet<Fact> new_facts = new HashSet<Fact>();
            rule.apply(this.facts, new_facts, new HashSet<ulong>());
            return new_facts;
        }

        public bool test_rule(Rule rule)
        {
            return rule.test(this.facts);
        }

        public World()
        {
            this.facts = new HashSet<Fact>();
            this.rules = new List<Rule>();
            this.checks = new List<Check>();
            this.privileged_rules = new List<Rule>();
        }

        public World(HashSet<Fact> facts, List<Rule> privileged_rules, List<Rule> rules)
        {
            this.facts = facts;
            this.rules = rules;
            this.checks = new List<Check>();
            this.privileged_rules = privileged_rules;
        }

        public World(HashSet<Fact> facts, List<Rule> privileged_rules, List<Rule> rules, List<Check> checks)
        {
            this.facts = facts;
            this.rules = rules;
            this.checks = checks;
            this.privileged_rules = privileged_rules;
        }

        public World(World w)
        {
            this.facts = new HashSet<Fact>();
            foreach (Fact fact in w.facts)
            {
                this.facts.Add(fact);
            }
            
            this.rules = new List<Rule>();
            this.rules.AddRange(w.rules);

            this.privileged_rules = new List<Rule>();
            this.privileged_rules.AddRange(w.privileged_rules);

            this.checks = new List<Check>();
            this.checks.AddRange(w.checks);
        }

        public string print(SymbolTable symbol_table)
        {
            StringBuilder s = new StringBuilder();

            s.Append("World {\n\t\tfacts: [");
            foreach (Fact f in this.facts)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.print_fact(f));
            }
            s.Append("\n\t\t]\n\t\tprivileged rules: [");
            foreach (Rule r in this.privileged_rules)
            {
                s.Append("\n\t\t\t");
                s.Append(symbol_table.print_rule(r));
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
    }
}
