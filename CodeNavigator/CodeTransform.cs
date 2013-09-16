using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class CodeTransform
    {
        internal static String ReplaceKeyword(String code, String keyword, String newKeyword)
        {
            if (code.Length == 0 || keyword.Length == 0)
                return code;

            //find keyword and check if char before and after are non-keyword chars
            int p = 0;
            int pCopyPos = 0;
            StringBuilder res = new StringBuilder();
            while ((p = code.IndexOf(keyword, p)) != -1)
            {
                int pEnd = p + keyword.Length;
                if ((p == 0 || !CodeAnalyzer.IsIdentifierChar(code[p - 1]))
                    && (pEnd == code.Length - 1 || !CodeAnalyzer.IsIdentifierChar(code[pEnd])))
                {
                    res.Append(code.Substring(pCopyPos, p - pCopyPos));
                    res.Append(newKeyword);
                    pCopyPos = pEnd;
                }
                p = pEnd;
            }

            if (pCopyPos == 0)
                return code;
            else
            {
                res.Append(code.Substring(pCopyPos, code.Length - pCopyPos));
                return res.ToString();
            }
        }
    }
}
