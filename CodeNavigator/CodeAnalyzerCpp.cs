using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class CodeAnalyzerCpp : CodeAnalyzer
    {
        static String MethodName(String text)
        {
            if (text == null)
                return null;

            text = text.Trim();
            if (text.StartsWith("~"))
                return IsIdentifier(text.Substring(1, text.Length - 1)) ? text : null;
            else
                return IsIdentifier(text) ? text : null;
        }

        internal static List<MethodImpl> GetMethodImplementations(String className, String code)
        {
            List<MethodImpl> res = new List<MethodImpl>();

            int p = 0;
            while ((p = code.IndexOf(className + "::", p)) != -1)
            {
                bool isValid = (p != 0 && IsWhiteSpace(code[p - 1]));
                int pStart = p;
                p += className.Length + 2;

                if (!isValid)
                    continue;

                //get method name
                String name = MethodName(ExtractUntil(code, ref p, '('));
                if (name == null)
                    continue;
                

                //get arguments
                String args = ExtractBlock(code, ref p, '(', ')');
                if (args == null)
                    continue;
                p += args.Length + 1;

                //modifiers and initializer
                String modAndInit = ExtractUntil(code, ref p, '{').Trim();
                if (modAndInit == null)
                    continue;

                //ctor initializer
                String initializer = "";
                int pInit = 0;
                String modifiers = ExtractUntil(modAndInit, ref pInit, ':');
                if (modifiers == null)
                    modifiers = modAndInit;
                else
                    initializer = modAndInit.Substring(pInit, modAndInit.Length - pInit);

                //method body
                String body = ExtractBlock(code, ref p);
                if (body == null)
                    continue;
                int pBodyStart = p - 1;
                int pBodyEnd = p + body.Length;
                p++;

                //get returnType
                char[] beginOfMethod = { '}', ';', '#' };
                String returnType = ExtractUntilReverseOrStart(code, ref pStart, beginOfMethod).Trim();
                if (pStart > 0 && code[pStart-1] == '#')
                {
                    int PPSkip = SkipPreprocessor(code, pStart-1, pStart + returnType.Length);
                    if (PPSkip > pStart)
                    {
                        int nSkip = PPSkip - pStart;
                        pStart = PPSkip;
                        int newLen = returnType.Length - nSkip;
                        if (newLen <= 0)
                            returnType = "";
                        else
                            returnType = code.Substring(pStart, newLen);
                    }
                }

                //create impl
                MethodImpl impl = new MethodImpl(returnType, name, args, modifiers, initializer, body);
                impl.SetClassName(className);
                impl.Start.CharOffsetWithoutComments = pStart;
                impl.End.CharOffsetWithoutComments = pBodyEnd;

                //set body char offset
                impl.Body.Start.CharOffsetWithoutComments = pBodyStart;
                impl.Body.End.CharOffsetWithoutComments = pBodyEnd;

                res.Add(impl);
            }

            //res.Add("void " + className + "::foo() {}");
            return res;
        }

        //returns code.Length if code ends with an unclosed PP stmt
        private static int SkipPreprocessor(string code, int pStart, int pEnd = -1)
        {
            if (pEnd == -1 || pEnd > code.Length)
                pEnd = code.Length;

            int p = pStart;
            while (p < pEnd)
            {
                int next = code.IndexOf('#', p);
                if (next == -1 || next >= pEnd)
                    return p;

                //'#' found -> skip line
                next = code.IndexOf('\n', next);
                if (next == -1)
                    return code.Length;

                p = next + 1;
            }

            return p;
        }

        //start should point to ';' or to a pos. after ';'
        internal static MethodDecl ExtractMethodDeclReverse(String className, String code, int start)
        {

            while (IsWhiteSpace(code[start]) && start > 0)
                start--;
            
            if (code[start] == ';')
                start--;

            //lookup end of prev decl
            int pBegin = 0;
            
            //find end of prev decl or begin / end of block
            //look for last decl. or begin of block
            char[] beginChars = { ';', '{', '}' };
            pBegin = code.LastIndexOfAny(beginChars, start);
            if (pBegin == -1)
                pBegin = 0;

            return ExtractMethodDeclFromTo(className, code, pBegin+1, start+1);
        }

        private static int SkipAccessModifiers(String code, int start, int stop)
        {
            while (start < stop)
            {
                //skip whitespace
                while (IsWhiteSpace(code[start]) && start < stop)
                    start++;

                if (start >= stop || code[start] != 'p')
                    return start;

                //find colon
                int pColon = start;
                while (code[pColon]!=':' && pColon < stop)
                    pColon++;

                //not found
                if (code[pColon]!=':')
                    return start;

                String mod = code.Substring(start, pColon - start).Trim();

                //skip modifier (protected, private, public)
                if (mod.Equals("protected")
                    || mod.Equals("private")
                    || mod.Equals("public"))
                    start = pColon + 1;
                else
                    return start;
            }

            return start;
        }

        //extract a method decl that starts at start and ends with semikolon at stop
        internal static MethodDecl ExtractMethodDecl(String className, String code, int start, int stop)
        {
            if (stop >= code.Length)
                stop = code.Length - 1;
#if DEBUG
            String dbgText = code.Substring(start, stop - start);
#endif

            //if we do not have a ';', we cannot extract a method decl
            if (code[stop] != ';')
                return null;
            Debug.Assert(code[stop] == ';');

            return ExtractMethodDeclReverse(className, code, stop);
        }

        //extract a method decl that starts at start and ends with semikolon at stop
        internal static MethodDecl ExtractMethodDeclFromTo(String className, String code, int start, int stop)
        {
            if (stop >= code.Length)
                stop = code.Length - 1;
#if DEBUG
            String dbgText = code.Substring(start, stop - start);
#endif

            //if we do not have a ';', we cannot extract a method decl
            if (code[stop] != ';')
                return null;
            Debug.Assert(code[stop]==';');

            //skip access modifiers
            start = SkipAccessModifiers(code, start, stop);

#if DEBUG
            dbgText = code.Substring(start, stop - start);
#endif

            //get end of arg list
            int pArgEnd = stop;
            String modifiers = ExtractUntilReverse(code, ref pArgEnd, ')');
            if (modifiers == null || pArgEnd <= start)
                return null;
            pArgEnd--;

            //get start of arg list
            int pArgStart = start;
            String typeAndName = ExtractUntil(code, ref pArgStart, '(');
            if (typeAndName == null)
                return null;
            pArgStart++;

            //extract args excluding '(', ')'
            String args = code.Substring(pArgStart, pArgEnd - pArgStart);
            if (!ValidArgs(args))
                return null;

            //get method name
            typeAndName = typeAndName.Trim();
            int pRetEnd = typeAndName.Length-1;
            String name = Keyword(ExtractIdentifierReverse(typeAndName, ref pRetEnd));
            if (name == null || IsKeyword(name))
                return null;

            //return type
            String returnType = typeAndName.Substring(0, pRetEnd).Trim();
            if (returnType.EndsWith("~"))
            {
                //move ~ to name
                returnType = returnType.Substring(0, returnType.Length - 1).Trim();
                name = '~' + name;
            }

            //extract virtual
            bool bVirtual = false;
            if (returnType.StartsWith("virtual")
                && (returnType.Length == 7 || !IsIdentifierChar(returnType[7])))
            {
                returnType = returnType.Substring(7, returnType.Length - 7).Trim();
                bVirtual = true;
            }

            //extract static
            bool bStatic = false;
            if (returnType.StartsWith("static")
                && (returnType.Length == 6 || !IsIdentifierChar(returnType[6])))
            {
                returnType = returnType.Substring(6, returnType.Length - 6).Trim();
                bStatic = true;
            }

            //check if the type is valid
            if ( !(returnType.Length==0 || IsValidTypeDef(returnType)) )
                return null;

            //check if ctor or dtor
            bool bCtor = false;
            bool bDtor = false;
            if (returnType.Length == 0)
            {
                //method without return type (probably a statement)
                if (!name.EndsWith(className))
                    return null;

                //is ctor?
                if (name.Equals(className))
                    bCtor = true;
                else if (name.Equals("~" + className))
                    bDtor = true;
                else
                    return null;
            }

            //create decl
            MethodDecl res = new MethodDecl(returnType, name, args, modifiers);

            res.Start.CharOffsetWithoutComments = start;
            res.End.CharOffsetWithoutComments = stop;
            
            //set virtual, static
            res.SetIsVirtual(bVirtual);
            res.SetIsStatic(bStatic);

            //ctor / dtor
            res.SetIsConstructor(bCtor);
            res.SetIsDestructor(bDtor);

            //class name
            res.SetClassName(className);

            return res;
        }

        private static bool ValidArgs(string args)
        {
            //TODO: do a more complete analysis
            char[] chars = { '(', ')' };
            int pFirstBrace = args.IndexOfAny(chars);

            //no braces
            if (pFirstBrace == -1)
            {
            }
            //starts with closing brace?
            else if (args[pFirstBrace] == ')')
                return false;
            else
            {
                //TODO: check for ALL matching braces
                int pRoundBraceBlockStart = SkipBlock(args, ref pFirstBrace, '(', ')');
                if (pRoundBraceBlockStart == -1)
                    return false;
            }

            return true;
        }

        //checks if type is a valid type def. (e.g., const char*&)
        private static bool IsValidTypeDef(string type)
        {
            int len = type.Length;
            if (len==0)
                return false;

            //the following code allows "::" (::foo or foo::bar )but it must not be at the end of a type (foo::)
            if (type[len - 1] == ':')
                return false;

            char prevChar = '\0';
            bool bLastWhite = true;
            int wordStart = 0;
            for (int i = 0; i < len; i++)
            {
                char c = type[i];
                char nextChar = (i+1 < type.Length ? type[i+1] : '\0');
                bool bWhite = IsWhiteSpace(c);
                bool bValidChar = (IsIdentifierChar(c)
                                   || bWhite
                                   || c == '&'
                                   || c == '*'
                                   || c == ':' && (nextChar == ':' || prevChar == ':')  //::
                                   );

                if (!bValidChar)
                    return false;

                //end of string -> make it end of word
                if (i==type.Length - 1)
                {
                    i++;
                    bWhite = true;
                }
                //end of word
                if (   bWhite 
                    && !bLastWhite 
                    && IsNonTypeKeyword(type.Substring(wordStart, i - wordStart)))
                    return false;
                //begin of word
                else if (!bWhite && bLastWhite)
                    wordStart = i;

                bLastWhite = bWhite;
                prevChar = c;
            }

            return true;
        }

        private static bool IsKeyword(string word)
        {
            if (_keywords==null)
            {
                _keywords = new HashSet<string>();
                _keywords.Add("return");
                _keywords.Add("virtual");
                _keywords.Add("else");
                _keywords.Add("if");
                _keywords.Add("while");
                _keywords.Add("for");
                _keywords.Add("do");
            }
            return _keywords.Contains(word);
        }
        
        //returns true if word is a keyword that cannot be in a type def
        private static bool IsNonTypeKeyword(string word)
        {
            if (!IsKeyword(word))
                return false;

            //if it is a keyword, check if it is a keyword that may appear in a type def
            if (_typeDefKeywords == null)
            {
                _typeDefKeywords = new HashSet<string>();
                _typeDefKeywords.Add("const");
            }
            return !_typeDefKeywords.Contains(word);
        }

        internal static List<MethodDecl> GetMethodDeclarations(String className, String code)
        {
            List<MethodDecl> res = new List<MethodDecl>();

            int lastP = 0;
            int p = 0;
            while ((p = code.IndexOf(';', p)) != -1)
            {
                MethodDecl decl = ExtractMethodDecl(className, code, lastP, p);

                if (decl != null)
                    res.Add(decl);

                p++;
                lastP = p;
            }

            //res.Add("void " + className + "::foo() {}");
            return res;
        }

        //all keywords
        static HashSet<String> _keywords = null;

        //keywords that appear in type defs
        static HashSet<String> _typeDefKeywords = null;
    }
}
