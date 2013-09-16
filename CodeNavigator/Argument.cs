using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeNavigator
{
    class Argument
    {
        internal String Type { get; set; }
        internal String Name { get; set; }
        internal String Default { get; set; }

        internal Argument(String type, String name, String _default)
        {
            this.Type = type;
            this.Name = name;
            this.Default = _default;
        }

        private static Argument ArgumentFromString(String arg)
        {
            String nameAndType;
            String defaultValue;
            int p = arg.IndexOf('=');
            if (p == -1)
            {
                nameAndType = arg;
                defaultValue = "";
            }
            else
            {
                nameAndType = arg.Substring(0, p).Trim();
                defaultValue = arg.Substring(p + 1, arg.Length - p - 1).Trim();
            }

            int start = nameAndType.Length;
            String name = CodeAnalyzerCpp.ExtractIdentifierReverse(nameAndType, ref start);
            String type = nameAndType.Substring(0, start).Trim();

            return new Argument(type, name, defaultValue);
        }

        internal static List<Argument> Parse(String args)
        {
            List<Argument> res = new List<Argument>();

            char[] delim = { '(', ',' };
            int lastPos = 0;
            int pos = 0;
            while ((pos = args.IndexOfAny(delim, pos)) != -1)
            {
                if (args[pos] == '(')
                {
                    int pEndBrace = CodeAnalyzer.SkipBlock(args, ref pos, '(', ')');
                    if (pEndBrace != -1)
                        pos = pEndBrace + 1;
                }
                else
                {
                    res.Add(ArgumentFromString(args.Substring(lastPos, pos - lastPos)));
                    pos++;
                    lastPos = pos;
                }
            }

            if (args.Length > lastPos)
                res.Add(ArgumentFromString(args.Substring(lastPos, args.Length - lastPos).Trim()));

            return res;
        }

        internal void Append(StringBuilder res, bool bAppendDefaultAsComment = false)
        {
            res.Append(Type).Append(' ').Append(Name);

            if (Default.Length!=0)
            {
                if (bAppendDefaultAsComment)
                    res.Append(" /*= ").Append(Default).Append("*/");
                else
                    res.Append(" = ").Append(Default);
            }
                
        }
    }
}
