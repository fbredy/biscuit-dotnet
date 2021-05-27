using System;
using System.Collections.Generic;

namespace Biscuit.Token.Builder
{
    public class Utils
    {

        public static Term set(HashSet<Term> s)
        {
            return new Term.Set(s);
        }

        public static Builder.Fact fact(string name, List<Term> ids)
        {
            return new Builder.Fact(name, ids);
        }

        public static Builder.Predicate pred(string name, List<Term> ids)
        {
            return new Builder.Predicate(name, ids);
        }

        public static Builder.Rule rule(string head_name, List<Term> head_ids,
                                                                      List<Builder.Predicate> predicates)
        {
            return new Builder.Rule(pred(head_name, head_ids), predicates, new List<Builder.Expression>());
        }

        public static Builder.Rule constrained_rule(String head_name, List<Term> head_ids,
                                                                                  List<Builder.Predicate> predicates,
                                                                                  List<Builder.Expression> expressions)
        {
            return new Builder.Rule(pred(head_name, head_ids), predicates, expressions);
        }

        public static Check check(Builder.Rule rule)
        {
            return new Check(rule);
        }

        public static Term integer(long i)
        {
            return new Term.Integer(i);
        }

        public static Term strings(string s)
        {
            return new Term.Str(s);
        }

        public static Term s(string str)
        {
            return new Term.Symbol(str);
        }

        public static Term date(DateTime d)
        {
            return new Term.Date((ulong)((DateTimeOffset)d).ToUnixTimeSeconds());
        }

        public static Term var(string name)
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
