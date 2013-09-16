using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class CodeAnalyzer
    {
        public static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        public static bool IsIdentifierChar(char c)
        {
            return c >= '0' && c <= '9'
                || c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c == '_';
        }

        public static bool IsIdentifier(String text)
        {
            if (text.Length == 0)
                return false;

            //must not start with a number
            if (text[0] >= '0' && text[0] <= '9')
                return false;

            //check for valid chars
            foreach (char c in text)
            {
                //check if valid char
                if (!IsIdentifierChar(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// returns string starting at pStart until (excluding) lookup
        /// pStart then points to the next char after the returned string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart"></param>
        /// <param name="lookup"></param>
        /// <returns></returns>
        protected static String ExtractUntil(String text, ref int pStart, char lookup)
        {
            int p = text.IndexOf(lookup, pStart);
            if (p == -1)
                return null;

            String res = text.Substring(pStart, p - pStart);
            pStart = p;
            return res;
        }

        /// <summary>
        /// Skip a block starting with openBrace and ending with closeBrace
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart">Should point to the opening brace or before it. 
        /// After execution it points to the begin of the block (first char after '{' ).</param>
        /// <param name="openBrace"></param>
        /// <param name="closeBrace"></param>
        /// <returns>the position of the closing brace.</returns>
        internal static int SkipBlock(String text, ref int pStart, char openBrace, char closeBrace)
        {
            int p = text.IndexOf(openBrace, pStart);
            if (p == -1)
                return -1;
            p++;

            int pBlockBegin = p;

            int nesting = 1;
            char[] braces = { openBrace, closeBrace };
            while ((p = text.IndexOfAny(braces, p)) != -1)
            {
                if (text[p] == openBrace)
                    nesting++;
                else
                    nesting--;

                if (nesting == 0)
                {
                    pStart = pBlockBegin;
                    return p;
                }

                p++;
            }

            //too less closing braces
            return -1;
        }

        /// <summary>
        /// Skip a block starting with openBrace and ending with closeBrace
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart">Should point to the closing brace or after it. 
        /// After execution it points to the end of the block (last char before '}' ).</param>
        /// <param name="openBrace"></param>
        /// <param name="closeBrace"></param>
        /// <returns>the position of the opening brace.</returns>
        internal static int SkipBlockReverse(String text, ref int pStart, char openBrace, char closeBrace)
        {
            int p = text.LastIndexOf(closeBrace, pStart);
            if (p == -1)
                return -1;
            p--;

            int pBlockEnd = p;

            int nesting = 1;
            char[] braces = { openBrace, closeBrace };
            while ((p = text.LastIndexOfAny(braces, p)) != -1)
            {
                if (text[p] == closeBrace)
                    nesting++;
                else
                    nesting--;

                if (nesting == 0)
                {
                    pStart = pBlockEnd;
                    return p;
                }

                p--;
            }

            //too less closing braces
            return -1;
        }

        /// <summary>
        /// returns the string between lookup and pStart
        /// pStart then points to the begin of the string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart"></param>
        /// <param name="lookup"></param>
        /// <returns></returns>
        protected static String ExtractUntilReverse(String text, ref int pStart, char lookup)
        {
            int p = text.LastIndexOf(lookup, pStart);
            if (p == -1)
                return null;

            p++;
            String res = text.Substring(p, pStart - p);
            pStart = p;
            return res;
        }

        /// <summary>
        /// returns the string between lookup and pStart
        /// pStart then points to the begin of the string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart"></param>
        /// <param name="lookup"></param>
        /// <returns></returns>
        protected static String ExtractUntilReverse(String text, ref int pStart, char[] lookup)
        {
            int p = text.LastIndexOfAny(lookup, pStart);
            if (p == -1)
                return null;

            p++;
            String res = text.Substring(p, pStart - p);
            pStart = p;
            return res;
        }

        internal static String ExtractIdentifier(String text, ref int pStartEnd)
        {
            int pStart = pStartEnd;
            while (pStartEnd < text.Length)
            {
                if (!IsIdentifierChar(text[pStartEnd]))
                    break;

                pStartEnd++;
            }

            return text.Substring(pStart, pStartEnd - pStart);
        }

        internal static String ExtractIdentifierReverse(String text, ref int pStart)
        {
            if (pStart == text.Length)
                pStart = text.Length - 1;
            int pEnd = pStart;
            while (pStart >= 0)
            {
                if (!IsIdentifierChar(text[pStart]))
                {
                    pStart++;
                    break;
                }

                pStart--;
            }

            if (pStart == -1)
                pStart = 0;

            return text.Substring(pStart, pEnd - pStart + 1);
        }

        /// <summary>
        /// returns the string between lookup and pStart
        /// pStart then points to the begin of the string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pStart"></param>
        /// <param name="lookup"></param>
        /// <returns></returns>
        protected static String ExtractUntilReverseOrStart(String text, ref int pStart, char[] lookup)
        {
            int p = text.LastIndexOfAny(lookup, pStart);

            p++;    //either not found -> start at 0; or found but then skip that char

            String res = text.Substring(p, pStart - p);
            pStart = p;
            return res;
        }

        protected static String Keyword(String text)
        {
            if (text == null)
                return null;

            text = text.Trim();
            return IsIdentifier(text) ? text : null;
        }

        //after call, pStart points to begin if 
        protected static String ExtractBlock(String code, ref int pStart, char openBrace = '{', char closeBrace = '}')
        {
            int pEnd = SkipBlock(code, ref pStart, openBrace, closeBrace);
            if (pEnd == -1)
                return null;

            return code.Substring(pStart, pEnd - pStart);
        }

        internal static String RemoveComments(String code, List<CodeLocation> charMapping = null)
        {
            CommentRemover remover = new CommentRemover(code, charMapping);
            return remover.RemoveComments();
        }

        internal static List<String> GetNamespaceFromCode(String codeWithoutComments)
        {
            //parse namespaces
            int pos = 0;
            List<String> namespaces = new List<String>();
            GetNamespaces(codeWithoutComments, ref pos, namespaces);

            return namespaces;
        }

        private static void GetNamespaces(String text, ref int pos, List<String> namespaces)
        {
            int nBlocks = 0;
            char[] braces = { 'n', '{', '}' };
            while ((pos = text.IndexOfAny(braces, pos)) != -1)
            {
                if (text[pos] == 'n')
                {
                    //check if "namespace"
                    if (text[pos + 1] != 'a'
                     || !text.Substring(pos, 10).Equals("namespace "))
                    {
                        pos++;
                        continue;
                    }

                    pos += 10;

                    //look for start of namespace block
                    char[] chars = { '{', ';' };
                    int pStart = text.IndexOfAny(chars, pos);
                    if (pStart == -1)
                        return;

                    //namespace block
                    if (text[pStart] == '{')
                    {
                        namespaces.Add(text.Substring(pos, pStart - pos).Trim());
                        pos = pStart + 1;

                        //continue recursively
                        GetNamespaces(text, ref pos, namespaces);

                        //check if namespace is also closing
                        if (pos != -1 && text[pos] == '}')
                            namespaces.RemoveAt(namespaces.Count - 1);
                    }
                }
                //a block within the namespace
                else if (text[pos] == '{')
                    nBlocks++;
                else
                {
                    //no open blocks -> closing namespace
                    if (nBlocks == 0)
                        return;

                    nBlocks--;
                }

                if (pos == -1)
                    break;

                pos++;
            }
        }

        /// <summary>
        /// looks for the first def. of a class within code
        /// will not remove comments
        /// </summary>
        /// <param name="code"></param>
        /// <returns>index of first char of the "class" keyword</returns>
        internal static int FindFirstClassDef(String code)
        {
            char[] classHeaderEnd = { '{', ';' };
            int pos = 0;
            while ((pos = code.IndexOf("class", pos)) != -1)
            {
                //check if last char is delim
                char lastChar = (pos == 0 ? ' ' : code[pos - 1]);
                pos += 5;

                bool isLastCharDelim =
                       lastChar == ';'
                    || lastChar == '}'
                    || IsWhiteSpace(lastChar);

                if (!isLastCharDelim)
                    continue;

                //check if next char is whitespace
                char nextChar = (pos >= code.Length ? ' ' : code[pos]);
                if (!IsWhiteSpace(nextChar))
                    continue;

                //lookup end of class decl. or def.
                int pBody = code.IndexOfAny(classHeaderEnd, pos);
                if (pBody == -1)
                    continue;

                //if char is ';' this is a fwd decl
                if (code[pBody] == ';')
                    continue;

                //return position of class keyword
                return pos - 5;
            }

            //not found
            return -1;
        }

        internal static String FindFirstClassDef(String code, ref int pStart)
        {
            pStart = FindFirstClassDef(code);
            if (pStart == -1)
                return "";

            pStart += "class".Length;
            pStart = SkipWhitespace(code, pStart);
            if (pStart == -1)
                return "";

            return ExtractIdentifier(code, ref pStart);
        }

        internal static int SkipWhitespace(String code, int pos)
        {
            while (pos < code.Length)
            {
                if (!IsWhiteSpace(code[pos]))
                    return pos;

                pos++;
            }

            return code.Length;
        }

        internal static int SkipWhitespaceReverse(String code, int pos)
        {
            while (pos >= 0)
            {
                if (!IsWhiteSpace(code[pos]))
                    return pos;

                pos--;
            }

            return -1;
        }

        //skip comments reverse starting at pos
        //returns start of first comment line
        //TODO: support /* */
        //TODO: do not copy lines; use SkipWhitespace instead
        internal static int SkipCommentLinesReverse(String code, int pos)
        {
            //find prev line
            pos = code.LastIndexOf('\n', pos) + 1;
            if (pos <= 0)
                return 0;

            //check line by line
            while (pos >= 2)
            {
                int startOfLine = code.LastIndexOf('\n', pos-2) + 1;
                String line = code.Substring(startOfLine, pos - startOfLine - 1).Trim();
                if (!line.StartsWith("//"))
                    return pos;

                pos = startOfLine;
            }

            return pos;
        }

        /// <summary>
        /// Find full qualified ID. If nameSpace is given, it will check
        /// for correct namespace definitions or usings
        /// </summary>
        /// <param name="code"></param>
        /// <param name="nameSpace"></param>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        internal static int NextId(String code, String[] nameSpace, String ID, int pos)
        {
            while ((pos = code.IndexOf(ID, pos)) != -1)
            {
                //check if last char is ID char
                char prevChar = (pos == 0 ? ' ' : code[pos - 1]);
                pos += ID.Length;
                if (IsIdentifierChar(prevChar))
                    continue;

                //check if next char is still ID
                char nextChar = (pos >= code.Length ? ' ' : code[pos]);
                if (IsIdentifierChar(nextChar))
                    continue;

                //found
                return pos - ID.Length;
            }

            //not found
            return -1;
        }

    }
}
