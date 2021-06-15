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
        public List<string> Symbols { get; }

        public ulong Insert(string symbol)
        {
            int index = this.Symbols.IndexOf(symbol);
            if (index == -1)
            {
                this.Symbols.Add(symbol);
                return (ulong)(this.Symbols.Count - 1);
            }
            else
            {
                return (ulong)index;
            }
        }

        public ID Add(string symbol)
        {
            return new ID.Symbol(this.Insert(symbol));
        }

        public Option<ulong> Get(string symbol)
        {
            long index = this.Symbols.IndexOf(symbol);
            if (index == -1)
            {
                return Option<ulong>.None();
            }
            else
            {
                return Option<ulong>.Some((ulong)index);
            }
        }

        public string PrintId(ID value)
        {
            string result = string.Empty;
            switch (value)
            {
                case ID.Bool _:
                    result = ((ID.Bool)value).Value.ToString();
                    break;
                case ID.Bytes _:
                    result = StrUtils.BytesToHex(((ID.Bytes)value).Value);
                    break;
                case ID.Date _:
                    {
                        DateTime d = DateTime.UnixEpoch.AddSeconds(((ID.Date)value).Value);
                        result = d.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssZ");
                        break;
                    }

                case ID.Integer _:
                    result = ((ID.Integer)value).Value.ToString();
                    break;
                case ID.Set _:
                    {
                        ID.Set idset = (ID.Set)value;
                        if (idset.Value.Count > 0)
                        {
                            result = "[ " + string.Join(", ", idset.Value.Select((id) => PrintId(id)).ToList()) + " ]";
                        }
                        break;
                    }

                case ID.Str _:
                    result = "\"" + ((ID.Str)value).Value + "\"";
                    break;
                case ID.Symbol _:
                    result = "#" + PrintSymbol((int)((ID.Symbol)value).Value);
                    break;
                case ID.Variable _:
                    result = "$" + PrintSymbol((int)((ID.Variable)value).Value);
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }

        public string PrintRule(Rule r)
        {
            string res = this.PrintPredicate(r.Head);
            res += " <- " + this.PrintRuleBody(r);

            return res;
        }

        public string PrintRuleBody(Rule r)
        {
            List<string> preds = r.Body.Select(p => this.PrintPredicate(p)).ToList();
            List<string> expressions = r.Expressions.Select(c => this.PrintExpression(c)).ToList();

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

        public string PrintExpression(Expression e)
        {
            return e.Print(this).Get();
        }


        public string PrintPredicate(Predicate p)
        {
            List<string> ids = p.Ids.Select(i =>
            {
                if (i is ID.Variable)
                {
                    return "$" + this.PrintSymbol((int)((ID.Variable)i).Value);
                }
                else if (i is ID.Symbol)
                {
                    return "#" + this.PrintSymbol((int)((ID.Symbol)i).Value);
                }
                else if (i is ID.Date)
                {
                    return DateTime.UnixEpoch.AddSeconds(((ID.Date)i).Value).ToString();
                }
                else if (i is ID.Integer)
                {
                    return string.Empty + ((ID.Integer)i).Value;
                }
                else if (i is ID.Str)
                {
                    return "\"" + ((ID.Str)i).Value + "\"";
                }
                else if (i is ID.Bytes)
                {
                    return "hex:" + StrUtils.BytesToHex(((ID.Bytes)i).Value);
                }
                else
                {
                    return "???";
                }
            }).ToList();

            var result = this.PrintSymbol((int)p.Name);

            return (result ?? "<?>") + "(" + string.Join(", ", ids) + ")";
        }

        public string PrintFact(Fact f)
        {
            return this.PrintPredicate(f.Predicate);
        }

        public string PrintCheck(Check c)
        {
            string res = "check if ";
            List<string> queries = c.Queries.Select((q) => this.PrintRuleBody(q)).ToList();
            return res + string.Join(" or ", queries);
        }

        public string PrintWorld(World w)
        {
            List<string> facts = w.Facts.Select((f) => this.PrintFact(f)).ToList();
            List<string> rules = w.Rules.Select((r) => this.PrintRule(r)).ToList();
            List<string> checksStr = w.Checks.Select((c) => this.PrintCheck(c)).ToList();

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

        public string PrintSymbol(int i)
        {
            if (i >= 0 && i < this.Symbols.Count)
            {
                return this.Symbols[i];
            }
            else
            {
                return "<" + i + "?>";
            }
        }

        public SymbolTable()
        {
            this.Symbols = new List<string>();
        }
        public SymbolTable(SymbolTable s)
        {
            this.Symbols = new List<string>();
            this.Symbols.AddRange(s.Symbols);
        }
    }
}
