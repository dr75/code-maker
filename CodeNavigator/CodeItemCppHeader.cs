using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class CodeItemCppHeader : CodeItemCpp
    {
        internal CodeItemCppHeader(ProjectItem item)
            :base(item)
        {}

        static String[] headerExtensions = { ".h", ".hpp", ".hh" };

        //lookup by class name
        internal static CodeItemCppHeader Lookup(ProjectItems items, String className)
        {
            //lookup .h
            ProjectItem header = GetProjectItem(items, className, headerExtensions);

            return (header != null ? new CodeItemCppHeader(header) : null);
        }

        internal override bool IsCppHeader() { return true; }

        internal void SetCppImpl(CodeItemCppImpl cpp)
        {
            _cpp = cpp;
        }

        internal override CodeItemCppImpl GetCppImpl()
        {
            if (_cpp == null)
            {
                _cpp = CodeItemCpp.GetCppItem(_projectItem);
                _cpp.SetCppHeader(this);
            }
                
            return _cpp;
        }

        internal override CodeItemCppHeader GetCppHeader()
        {
            return this;
        }

        internal override CodeItemCpp GetCorrespondingItem()
        {
            return GetCppImpl();
        }

        //get the declarations for the given method impl
        internal List<MethodDecl> GetMethodDeclarations(List<MethodImpl> methods)
        {
            List<MethodDecl> decls = new List<MethodDecl>();
            foreach (MethodImpl impl in methods)
            {
                MethodDecl decl = GetMethodDecl(impl);
                /*if (decl == null)
                    decl = impl;*/

                decls.Add(decl);
            }

            //TODO: lookup the corresponding declarations in header
            return decls;
        }

        //get the declarations for the given method impl
        internal string GetMethodDeclarationsAsString(List<MethodImpl> methods, bool bExtract)
        {
            List<MethodDecl> decls = GetMethodDeclarations(methods);
            return GetAsString(decls, bExtract);
        }

        /// <summary>
        /// Get the decl from code, which corresponds to the given impl.
        /// </summary>
        /// <param name="impl"></param>
        /// <returns></returns>
        private MethodDecl GetMethodDecl(MethodImpl impl)
        {
            Dictionary<String, MethodDecl> decls = GetMethodDecls();

            MethodDecl decl = null;
            decls.TryGetValue(impl.GetSignature(), out decl);
            return decl;
        }

        private Dictionary<String, MethodDecl> GetMethodDecls()
        {
            if (_decls == null)
            {
                _decls = new Dictionary<String, MethodDecl>();
                List<MethodDecl> declList = CodeAnalyzerCpp.GetMethodDeclarations(GetClassName(), GetContent());
                foreach (MethodDecl decl in declList)
                    _decls.Add(decl.GetSignature(), decl);
            }

            return _decls;
        }

        CodeItemCppImpl _cpp = null;

        Dictionary<String, MethodDecl> _decls = null;
    }
}
