using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace CodeNavigator
{
    abstract class CodeItemCpp : CodeItem
    {
        internal CodeItemCpp(ProjectItem item)
            :base(item)
        {}

        internal static CodeItemCppImpl GetCppItem(ProjectItem srcItem)
        {
            //is it the header?
            if (IsCpp(srcItem))
                return new CodeItemCppImpl(srcItem);            

            //try to find the corresponding item
            ProjectItem cpp = FindCorrespondingCpp(srcItem);

            return new CodeItemCppImpl(cpp == null ? srcItem : cpp);
        }

        static Dictionary<ProjectItem, ProjectItem> correspondingItems = new Dictionary<ProjectItem, ProjectItem>();

        static String[] cppExtensions = { ".cpp", ".cxx", ".cc" };
        private static ProjectItem FindCorrespondingCpp(ProjectItem srcItem)
        {
            ProjectItem res = null;
            if (correspondingItems.TryGetValue(srcItem, out res))
                return res;

            String name = srcItem.Name;

            //no file extension?
            int pDot = name.LastIndexOf('.');
            if (pDot == -1)
                return null;

            name = name.Substring(0, pDot);
            ProjectItems items = srcItem.ContainingProject.ProjectItems;

            //lookup .cpp            
            return GetProjectItem(items, name, cppExtensions);
        }

        internal static bool IsCpp(String name)
        {
            //is it the cpp file?
            return (name.EndsWith(".cpp")
                 || name.EndsWith(".cxx")
                 || name.EndsWith(".cc"));
        }

        internal static bool IsCpp(ProjectItem item)
        {
            return IsCpp(item.Name);
        }

        internal static bool IsHeader(String name)
        {
            //is it the header?
            return (name.EndsWith(".h") || name.EndsWith(".hpp"));
        }

        internal static bool IsHeader(ProjectItem item)
        {
            //is it the header?
            return IsHeader(item.Name);
        }

        internal override bool IsThisOrRelatedItem(ProjectItem item)
        {
            if (item == this._projectItem)
                return true;

            CodeItemCpp other = GetCorrespondingItem();
            return other != null && item == other._projectItem;
        }

        internal static CodeItemCpp Create(ProjectItem srcItem)
        {
            if (IsHeader(srcItem))
                return new CodeItemCppHeader(srcItem);
            else
                return new CodeItemCppImpl(srcItem);
        }

        internal abstract CodeItemCppHeader GetCppHeader();
        internal abstract CodeItemCppImpl GetCppImpl();

        //extract namespace from given document
        //document must be the active document
        protected override List<String> GetNamespaceInt()
        {
            //lookup header for the cpp
            CodeItemCppHeader header = GetCppHeader();

            //no header found or we are in the header -> use the default approach
            if (header == null || header.GetProjectItem() == _projectItem)
                return base.GetNamespaceInt();

            //get content
            String text = header.GetContent();

            //could not get content?
            if (text == null)
                return base.GetNamespace();

            //lookup def of class
            int p = CodeAnalyzer.FindFirstClassDef(text);

            //no class in file -> use the default approach
            if (p == -1)
                return base.GetNamespace();

            return CodeAnalyzer.GetNamespaceFromCode(text.Substring(0, p));
        }

        /// <summary>
        /// replace in both, cpp and header
        /// </summary>
        /// <param name="oldWord"></param>
        /// <param name="newWord"></param>
        internal override void ReplaceWordInClass(String oldWord, String newWord)
        {
            //replace in this (either cpp or header)
            ReplaceWord(oldWord, newWord);

            //replace in corresponding item
            CodeItemCpp other = GetCorrespondingItem();
            if (other != null)
                other.ReplaceWord(oldWord, newWord);
        }

        internal override bool RenameFiles(String newItemName)
        {
            //rename this
            bool res = RenameFile(newItemName);

            //rename corresponding item
            CodeItemCpp other = GetCorrespondingItem();
            if (other != null)
                res &= other.RenameFile(newItemName);

            return res;
        }

        protected override void OpenAllFiles() 
        {
            EnsureIsOpen();

            CodeItemCpp other = GetCorrespondingItem();
            if (other != null)
                other.EnsureIsOpen();
        }

        internal abstract CodeItemCpp GetCorrespondingItem();

        //get all implementations for the given decls
        internal List<MethodImpl> GetMethodImplementations(List<MethodDecl> decls, String className = null)
        {
            if (className == null)
                className = GetClassName();

            List<MethodImpl> impls = new List<MethodImpl>();
            foreach (MethodDecl decl in decls)
            {
                MethodImpl impl = GetMethodImpl(decl, className);
                if (impl != null)
                {
                    impl.SetClassName(className);
                    impls.Add(impl);
                }
            }

            return impls;
        }

        protected String GetAsString<T>(List<T> decls, bool bExtract) where T : MethodDecl
        {
            if (decls.Count == 0)
                return "";

            StringBuilder res = new StringBuilder();
            CodeManipulate manip = null;
            if (bExtract)
                manip = new CodeManipulate(this);

            foreach (MethodDecl decl in decls)
            {
                decl.UpdateOffsetFromMapping(this);
                res.AppendLine(GetCode(decl));

                if (bExtract)
                    manip.Delete(decl);
            }

            if (bExtract)
                manip.Commit();

            return res.ToString();
        }

        private string GetCode(CodeFragment fragment, bool bIncludeComments = true)
        {
            int start = fragment.Start.CharOffsetWithComments;
            int end = fragment.End.CharOffsetWithComments;
            return GetContent(!bIncludeComments).Substring(start + 1, end - start);
        }

        internal string GetMethodImplementationsAsString(List<MethodDecl> decls, string className, bool bExtract)
        {
            List<MethodImpl> impls = GetMethodImplementations(decls, className);
            return GetAsString(impls, bExtract);
        }

        //find the impl. for the given decl
        internal MethodImpl GetMethodImpl(MethodDecl decl, String className = null)
        {
            Dictionary<String, MethodImpl> impls = GetMethodImplementations(className);

            MethodImpl impl = null;
            impls.TryGetValue(decl.GetSignature(), out impl);
            return impl;
        }

        private Dictionary<String, MethodImpl> GetMethodImplementations(String className = null)
        {
            if (_impls == null)
            {
                if (className == null)
                    className = GetClassName();

                _impls = new Dictionary<String, MethodImpl>();
                List<MethodImpl> implList = CodeAnalyzerCpp.GetMethodImplementations(className, GetContent());
                foreach (MethodImpl impl in implList)
                {
                    try
                    {
                        _impls.Add(impl.GetSignature(), impl);
                    }
                    catch (ArgumentException)
                    {
                        //signature already exists
                        //-> impl. invalid

                    }
                }
            }

            return _impls;
        }

        Dictionary<String, MethodImpl> _impls = null;

        //extract the declarations defined in a class def.
        internal string ExtractMethodDecls(string content, string className, bool bExtract)
        {
            //get header
            CodeItemCppHeader header = GetCppHeader();

            //src is cpp -> get declarations for methods from header
            if (IsCppImpl() && header != null)
            {
                //lookup method implementations in cpp content
                List<MethodImpl> methods = CodeAnalyzerCpp.GetMethodImplementations(className,
                                                                    CodeAnalyzer.RemoveComments(content));

                //get corresponding decls. from header
                content = header.GetMethodDeclarationsAsString(methods, bExtract);
            }

            return content;
        }

        //extract the implementations for methods of a class
        internal string ExtractMethodImpls(string content, string className, bool bExtract)
        {
            //get header
            CodeItemCppImpl impl = GetCppImpl();

            //src is header -> get implementation from src-cpp
            if (IsCppHeader() && impl != null)
            {
                //lookup method implementations in header content
                List<MethodDecl> decls = CodeAnalyzerCpp.GetMethodDeclarations(
                                                        className, CodeAnalyzer.RemoveComments(content));

                //get corresponding impl. from cpp
                content = impl.GetMethodImplementationsAsString(decls, className, bExtract);
            }

            return content;
        }
    }
}
