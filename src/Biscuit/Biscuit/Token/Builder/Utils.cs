using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class Utils
    {
        public static Term Set(HashSet<Term> s)
        {
            return new Term.Set(s);
        }

        public static FactBuilder Fact(string name, List<Term> ids)
        {
            return new FactBuilder(name, ids);
        }

        public static PredicateBuilder Pred(string name, List<Term> ids)
        {
            return new PredicateBuilder(name, ids);
        }

        public static RuleBuilder Rule(string head_name, List<Term> headIds,
                                                                      List<PredicateBuilder> predicates)
        {
            return new RuleBuilder(Pred(head_name, headIds), predicates, new List<ExpressionBuilder>());
        }

        public static RuleBuilder ConstrainedRule(string head_name, List<Term> head_ids,
                                                    List<PredicateBuilder> predicates,
                                                    List<ExpressionBuilder> expressions)
        {
            return new RuleBuilder(Pred(head_name, head_ids), predicates, expressions);
        }

        public static CheckBuilder Check(RuleBuilder rule)
        {
            return new CheckBuilder(rule);
        }

        public static Term Integer(long i)
        {
            return new Term.Integer(i);
        }

        public static Term Strings(string s)
        {
            return new Term.Str(s);
        }

        public static Term Symbol(string str)
        {
            return new Term.Symbol(str);
        }

        public static Term Date(DateTime d)
        {
            return new Term.Date((ulong)((DateTimeOffset)d).ToUnixTimeSeconds());
        }

        public static Term Var(string name)
        {
            return new Term.Variable(name);
        }

        //private static readonly char[] HEX_ARRAY = "0123456789ABCDEF".ToCharArray();

        //public static string byteArrayToHexString(byte[] bytes)
        //{
        //    char[] hexChars = new char[bytes.Length * 2];
        //    for (int j = 0; j < bytes.Length; j++)
        //    {
        //        int v = bytes[j] & 0xFF;
        //        hexChars[j * 2] = HEX_ARRAY[v >> 4];
        //        hexChars[j * 2 + 1] = HEX_ARRAY[v & 0x0F];
        //    }
        //    return new string(hexChars);
        //}

        //public static byte[] hexStringToByteArray(string hex)
        //{
        //    int l = hex.Length;
        //    byte[] data = new byte[l / 2];
        //    for (int i = 0; i < l; i += 2)
        //    {
        //        data[i / 2] = (byte)((char.digit(hex[i], 16) << 4)
        //                + char.digit(hex[i + 1], 16));
        //    }
        //    return data;
        //}
    }
}
