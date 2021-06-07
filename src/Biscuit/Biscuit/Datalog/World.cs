using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class World
    {
        public void AddFact(Fact fact)
        {
            this.Facts.Add(fact);
        }

        public void AddRule(Rule rule)
        {
            this.Rules.Add(rule);
        }

        public void AddPrivilegedRule(Rule rule)
        {
            this.PrivilegedRules.Add(rule);
        }

        public void AddCheck(Check check) { this.Checks.Add(check); }

        public void ClearRules()
        {
            this.Rules.Clear();
        }

        public Either<Errors.Error, Void> Run(HashSet<ulong> restrictedSymbols)
        {
            return this.Run(new RunLimits(), restrictedSymbols);
        }

        public Either<Errors.Error, Void> Run(RunLimits limits, HashSet<ulong> restrictedSymbols)
        {

            DateTime limit = DateTime.Now.Add(limits.MaxTime);
            int iterations;
            for (iterations = 0; iterations < limits.MaxIterations; iterations++)
            {
                HashSet<Fact> newFacts = new HashSet<Fact>();

                foreach (Rule rule in this.PrivilegedRules)
                {
                    rule.Apply(this.Facts, newFacts, new HashSet<ulong>());

                    if (DateTime.Now.CompareTo(limit) >= 0)
                    {
                        return new Errors.TimeoutError();
                    }
                }

                foreach (Rule rule in this.Rules)
                {
                    rule.Apply(this.Facts, newFacts, restrictedSymbols);

                    if (DateTime.Now.CompareTo(limit) >= 0)
                    {
                        return new Errors.TimeoutError();
                    }
                }

                int len = this.Facts.Count;
                this.Facts.AddAll(newFacts);
                if (this.Facts.Count == len)
                {
                    return new Right(null);
                }

                if (this.Facts.Count >= limits.MaxFacts)
                {
                    return new Errors.TooManyFacts();
                }
            }

            return new Errors.TooManyIterationsError();
        }

        public HashSet<Fact> Facts { get; }

        public List<Rule> Rules { get; }

        public List<Check> Checks { get; }

        public List<Rule> PrivilegedRules { get; }


        public HashSet<Fact> Query(Predicate predicate)
        {
            var result = this.Facts.Where(f =>
            {
                if (f.Predicate.Name != predicate.Name)
                {
                    return false;
                }
                int minSize = Math.Min(f.Predicate.Ids.Count, predicate.Ids.Count);
                for (int i = 0; i < minSize; ++i)
                {
                    ID fid = f.Predicate.Ids[i];
                    ID pid = predicate.Ids[i];
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

        public HashSet<Fact> QueryRule(Rule rule)
        {
            HashSet<Fact> newFacts = new HashSet<Fact>();
            rule.Apply(this.Facts, newFacts, new HashSet<ulong>());
            return newFacts;
        }

        public bool TestRule(Rule rule)
        {
            return rule.Test(this.Facts);
        }

        public World()
        {
            this.Facts = new HashSet<Fact>();
            this.Rules = new List<Rule>();
            this.Checks = new List<Check>();
            this.PrivilegedRules = new List<Rule>();
        }

        public World(HashSet<Fact> facts, List<Rule> privilegedRules, List<Rule> rules)
        {
            this.Facts = facts;
            this.Rules = rules;
            this.Checks = new List<Check>();
            this.PrivilegedRules = privilegedRules;
        }

        public World(HashSet<Fact> facts, List<Rule> privilegedRules, List<Rule> rules, List<Check> checks)
        {
            this.Facts = facts;
            this.Rules = rules;
            this.Checks = checks;
            this.PrivilegedRules = privilegedRules;
        }

        public World(World world)
        {
            this.Facts = new HashSet<Fact>();
            foreach (Fact fact in world.Facts)
            {
                this.Facts.Add(fact);
            }

            this.Rules = new List<Rule>();
            this.Rules.AddRange(world.Rules);

            this.PrivilegedRules = new List<Rule>();
            this.PrivilegedRules.AddRange(world.PrivilegedRules);

            this.Checks = new List<Check>();
            this.Checks.AddRange(world.Checks);
        }

        public string Print(SymbolTable symbolTable)
        {
            StringBuilder s = new StringBuilder();

            s.Append("World {\n\t\tfacts: [");
            foreach (Fact f in this.Facts)
            {
                s.Append("\n\t\t\t");
                s.Append(symbolTable.PrintFact(f));
            }
            s.Append("\n\t\t]\n\t\tprivileged rules: [");
            foreach (Rule r in this.PrivilegedRules)
            {
                s.Append("\n\t\t\t");
                s.Append(symbolTable.PrintRule(r));
            }
            s.Append("\n\t\t]\n\t\trules: [");
            foreach (Rule r in this.Rules)
            {
                s.Append("\n\t\t\t");
                s.Append(symbolTable.PrintRule(r));
            }
            s.Append("\n\t\t]\n\t\tchecks: [");
            foreach (Check c in this.Checks)
            {
                s.Append("\n\t\t\t");
                s.Append(symbolTable.PrintCheck(c));
            }
            s.Append("\n\t\t]\n\t}");

            return s.ToString();
        }
    }
}
