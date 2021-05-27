using Biscuit.Datalog.Expressions;
using Ristretto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biscuit.Datalog
{
    [Serializable]
    public sealed class SymbolTable
    {
        public List<string> symbols { get; }
        public ulong insert(string symbol)
        {
            int index = this.symbols.IndexOf(symbol);
            if (index == -1)
            {
                this.symbols.Add(symbol);
                return (ulong)(this.symbols.Count - 1);
            }
            else
            {
                return (ulong)index;
            }
        }

        public ID Add(string symbol)
        {
            return new ID.Symbol(this.insert(symbol));
        }

        public Option<ulong> get(string symbol)
        {
            long index = this.symbols.IndexOf(symbol);
            if (index == -1)
            {
                return Option<ulong>.none();
            }
            else
            {
                return Option<ulong>.some((ulong)index);
            }
        }

        public string print_id(ID value)
        {
            string _s = string.Empty;
            if (value is ID.Bool) {
                _s = ((ID.Bool)value).value.ToString();
            } else if (value is ID.Bytes) {
                _s = StrUtils.bytesToHex(((ID.Bytes)value).value);
            } else if (value is ID.Date) {
                DateTime d = DateTime.UnixEpoch.AddSeconds(((ID.Date)value).value);
                _s = d.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssZ");
            } else if (value is ID.Integer) {
                _s = ((ID.Integer)value).value.ToString();
            } else if (value is ID.Set) {
                ID.Set idset = (ID.Set)value;
                if (idset.value.Count > 0)
                {
                    _s = "[ ";
                    _s += string.Join(", ", idset.value.Select((id)=>print_id(id)).ToList());
                    _s += " ]";
                }
            } else if (value is ID.Str) {
                _s = "\"" + ((ID.Str)value).value + "\"";
            } else if (value is ID.Symbol) {
                _s = "#" + print_symbol((int)((ID.Symbol)value).value);
            } else if (value is ID.Variable) {
                _s = "$" + print_symbol((int)((ID.Variable)value).value);
            }
            return _s;
        }

        public string print_rule(Rule r)
        {
            string res = this.print_predicate(r.head);
            res += " <- " + this.print_rule_body(r);

            return res;
        }

        public string print_rule_body(Rule r)
        {
            List<string> preds = r.body.Select(p=> this.print_predicate(p)).ToList();
            List<string> expressions = r.expressions.Select(c => this.print_expression(c)).ToList();

            string res = string.Join(", ", preds);
            if (expressions.Any())
            {
                if (preds.Any())
                {
                    res += ", ";
                }
                res += string.Join(", ", expressions);
            }
            return res;
        }

        public string print_expression(Expression e)
        {
            return e.print(this).get();
        }


        public string print_predicate(Predicate p)
        {
            List<string> ids = p.ids.Select(i => { 
                if (i is ID.Variable) {
                    return "$" + this.print_symbol((int)((ID.Variable)i).value);
                } else if (i is ID.Symbol) {
                    return "#" + this.print_symbol((int)((ID.Symbol)i).value);
                } else if (i is ID.Date) {
                    return DateTime.UnixEpoch.AddSeconds(((ID.Date)i).value).ToString();
                } else if (i is ID.Integer) {
                    return "" + ((ID.Integer)i).value;
                } else if (i is ID.Str) {
                    return "\"" + ((ID.Str)i).value + "\"";
                } else if (i is ID.Bytes) {
                    return "hex:" + StrUtils.bytesToHex(((ID.Bytes)i).value);
                } else
                {
                    return "???";
                }
            }).ToList();

            var result = this.print_symbol((int)p.name);

            return (result ?? "<?>") + "(" + string.Join(", ", ids) + ")";
        }

        public string print_fact(Fact f)
        {
            return this.print_predicate(f.predicate);
        }

        public string print_check(Check c)
        {
            string res = "check if ";
            List<string> queries = c.queries.Select((q)=> this.print_rule_body(q)).ToList();
            return res + string.Join(" or ", queries);
        }

        public string print_world(World w)
        {
            List<string> facts = w.facts.Select((f)=> this.print_fact(f)).ToList();
            List<string> rules = w.rules.Select((r)=> this.print_rule(r)).ToList();
            List<string> checksStr = w.checks.Select((c)=> this.print_check(c)).ToList();

            StringBuilder b = new StringBuilder();
            b.Append("World {\n\tfacts: [\n\t\t");
            b.Append(string.Join(",\n\t\t", facts));
            b.Append("\n\t],\n\trules: [\n\t\t");
            b.Append(string.Join(",\n\t\t", rules));
            b.Append("\n\t],\n\tchecks: [\n\t\t");
            b.Append(string.Join(",\n\t\t", checksStr));
            b.Append("\n\t]\n}");

            return b.ToString();
        }

        public string print_symbol(int i)
        {
            if (i >= 0 && i < this.symbols.Count)
            {
                return this.symbols[i];
            }
            else
            {
                return "<" + i + "?>";
            }
        }

        public SymbolTable()
        {
            this.symbols = new List<string>();
        }
        public SymbolTable(SymbolTable s)
        {
            this.symbols = new List<string>();
            this.symbols.AddRange(s.symbols);
        }
    }
}
