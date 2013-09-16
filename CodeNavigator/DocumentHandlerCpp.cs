using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class DocumentHandlerCpp : DocumentHandler
    {
        internal DocumentHandlerCpp(DTE2 applicationObject)
            :base(applicationObject, "cpp")
        {

        }

        internal override DocumentHandlerCpp GetCppHandler() { return this; }

        protected override AutoComplete InitAutoComplete()
        {
            _autoComplete = new AutoCompleteCpp(this);
            return _autoComplete;
        }

        internal bool SwitchToMethodImpl(int offset)
        {
            CodeItemCpp implItem = GetImplItem();
            if (implItem == null)
                return false;

            MethodDecl decl = GetMethodDeclReverse(implItem.GetClassName(), offset);

            if (decl == null || decl.IsAbstract())  //only create impl. if not abstract
                return false;

            //check if we already have the corresponding impl.
            MethodImpl impl = implItem.GetMethodImpl(decl, decl.GetClassName());
            if (impl == null)
                return false;

            impl.Body.UpdateOffsetFromMapping(implItem);
            String code = implItem.GetContent(false);

            //activate window and select method body
            int selStart = CodeAnalyzer.SkipWhitespace(code, impl.Body.Start.CharOffsetWithComments + 1);
            int selEnd = CodeAnalyzer.SkipWhitespaceReverse(code, impl.Body.End.CharOffsetWithComments - 1);

            implItem.ActivateWindow(selStart + 1, selEnd + 2);  //sel index seems to be based on 1

            return true;
        }

        //returns the item that is used for impl. methods
        private CodeItemCpp GetImplItem()
        {
            CodeItemCpp currentItem = CodeItemCpp.GetCppItem(this.GetDocument().ProjectItem);
            if (currentItem == null)
                return null;

            //get cpp (if not existing we do not create an impl. file)
            CodeItemCppImpl cpp = currentItem.GetCppImpl();
            return (cpp != null ? cpp : currentItem);
        }
        
        internal bool CreateMethodImpl(int offset)
        {
            CodeItemCpp implItem = GetImplItem();
            if (implItem == null)
                return false;

            MethodDecl decl = GetMethodDeclReverse(implItem.GetClassName(), offset, ";");

            if (decl == null || decl.IsAbstract())  //only create impl. if not abstract
                return false;

            //check if we already have the corresponding impl.
            MethodImpl impl = implItem.GetMethodImpl(decl);

            //already exists
            if (impl != null)
                return false;

            //ADD new impl
            impl = new MethodImpl(decl);
            impl.Body = new CodeFragment("\nthrow \"not implemented\";\n");

            //insert after prev. method 
            MethodDecl prevDecl = GetMethodDeclReverse(implItem.GetClassName(), offset);
            if (prevDecl != null)
            {
                //insert after impl. of prevDecl
                MethodImpl prevImpl = implItem.GetMethodImpl(prevDecl);
                if (prevImpl != null)
                {
                    prevImpl.UpdateOffsetFromMapping(implItem);

                    //+1 to move to curly brace
                    //+1 to move after curly brace
                    implItem.InsertString(prevImpl.End.CharOffsetWithComments + 2, 
                        "\n" + impl.GetImplAsString(implItem.GetClassName()) + '\n', 
                        true);
                    return true;
                }
            }

            //insert before next method 
            MethodDecl nextDecl = GetMethodDeclFwd(implItem.GetClassName(), decl.End.CharOffsetWithoutComments + 1);
            if (nextDecl != null)
            {
                //insert before impl. of nextDecl
                MethodImpl nextImpl = implItem.GetMethodImpl(nextDecl);
                if (nextImpl != null)
                {
                    nextImpl.UpdateOffsetFromMapping(implItem);

                    implItem.InsertString(nextImpl.Start.CharOffsetWithComments + 1,
                        "\n" + impl.GetImplAsString(implItem.GetClassName()) + "\n\n",
                        true);
                    return true;
                }
            }

            //could not insert -> append
            implItem.AppendString("\n" + impl.GetImplAsString(implItem.GetClassName()) + '\n', true);

            return true;
        }


        private MethodDecl GetMethodDeclReverse(String className, int offset, String append = null)
        {
            //get content
            String content = GetDocContent(offset);
            /*if (!content.StartsWith("//@FastCode"))
                return false;*/

            content = CodeAnalyzer.RemoveComments(content);

            //check if we are just finishing a method decl
            content = (append == null ? content : content + append);
            char[] beginChars = { ';', '{', '}' };

            //look for class body to make sure we are not within a method def
            int pClassBody = content.Length - 1;
            char[] curlyBraces = { '{', '}' };
            while (pClassBody != -1)
            {
                int p = content.LastIndexOfAny(curlyBraces, pClassBody);
                if (p == -1)
                    return null;

                if (content[p] == '}')
                {
                    p = CodeAnalyzerCpp.SkipBlockReverse(content, ref p, '{', '}');

                    //no opening brace
                    if (p == -1)
                        return null;

                    p--;
                }
                else
                {
                    //this must be the class body -> look for class keyword
                    int pClass = content.LastIndexOf("class", p);
                    if (pClass == -1)
                        return null;

                    //check if there is a { or } between "class" and '{'
                    //if yes, then we are not in a class body (e.g., in a method definition within a class)
                    int pOther = content.LastIndexOfAny(beginChars, p - 1);
                    if (pOther > pClass)
                        return null;  //no valid class def

                    //valid def
                    break;
                }

                pClassBody = p-1;
            }

            if (pClassBody == -1)
                return null;

            //lookup end of the decl we look for; we must skip blocks during skip
            int pBegin = content.Length - 1;
            while (pBegin != -1)
            {
                int prev = content.LastIndexOfAny(beginChars, pBegin);
                if (prev == -1 || content[prev] == '{')
                    return null;

                if (content[prev] == '}')
                {
                    prev = CodeAnalyzerCpp.SkipBlockReverse(content, ref prev, '{', '}');

                    //no opening brace
                    if (prev == -1)
                        return null;

                    prev--;
                }
                else if (content[prev] == ';')
                {
                    pBegin = prev;
                    break;
                }

                pBegin = prev;
            }

            return CodeAnalyzerCpp.ExtractMethodDeclReverse(className, content, pBegin);
        }

        private MethodDecl GetMethodDeclFwd(String className, int offset, String append = null)
        {
            //get content
            String content = GetDocContent();
            /*if (!content.StartsWith("//@FastCode"))
                return false;*/

            content = CodeAnalyzer.RemoveComments(content);

            //check if we are just finishing a method decl
            content = (append == null ? content : content + append);
            int stop = content.IndexOf(';', offset);
            if (stop == -1)
                return null;

            return CodeAnalyzerCpp.ExtractMethodDecl(className, content, offset, stop);
        }

        internal void ToggleCppHeader()
        {
            CodeItemCpp other = CodeItemCpp.Create(this.GetDocument().ProjectItem).GetCorrespondingItem();
            if (other != null)
                other.ActivateWindow();
        }

        internal void ToggleDeclImpl()
        {
            ProjectItem item = GetCurrentProjectItem();
            CodeItemCpp cppItem = CodeItemCpp.Create(item);
            if (cppItem.IsCppHeader())
            {
                //lookup ; in current line and get decl
                TextSelection selection = (TextSelection)item.Document.Selection;
                
                EditPoint ep = selection.ActivePoint.CreateEditPoint();
                EditPoint spStart = ep.CreateEditPoint();
                spStart.EndOfLine();

                SwitchToMethodImpl(spStart.AbsoluteCharOffset - 1);
            }
            else
            {
                //TODO
            }
        }

        private AutoCompleteCpp _autoComplete = null;
    }
}
