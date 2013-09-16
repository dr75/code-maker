using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class CodeTemplateInstance
    {
        public CodeTemplateInstance(CodeTemplate templ, String name)
        {
            _data = new StringBuilder(templ.GetTemplate());
            _templ = templ;
            _name = name;
#if DEBUG
            SetValue("name", name);
#endif
        }

        public void SetValue(String key, String value)
        {
            _data.Replace("<t:" + key + " />", value);
        }

        public String GetName()
        {
            return _name;
        }

        public String GetOutputFileName()
        {
            return _name + _templ.GetFileExt();
        }

        public override String ToString()
        {
            PrettyPrint();
            SetValue("name", _name);
            return _data.ToString();
        }

        public void SetCtorImpl(String ctor)
        {
            SetValue("ctor_impl", ctor);
        }

        public void SetDtorImpl(String dtor)
        {
            SetValue("dtor_impl", dtor);
        }

        public void SetCtorDecl(String ctor)
        {
            SetValue("ctor_decl", ctor);
        }

        public void SetDtorDecl(String dtor)
        {
            SetValue("dtor_decl", dtor);
        }

        public CodeTemplateInstance GetChildInstance(String templName, String instName)
        {
            if (_childInstances==null)
                _childInstances = new Dictionary<String, CodeTemplateInstance>();

            CodeTemplateInstance res = null;
            if (!_childInstances.TryGetValue(instName, out res))
            {
                CodeTemplate templ = null;
                if (!_templ.GetChilds().TryGetValue(templName, out templ))
                    return null;

                res = new CodeTemplateInstance(templ, instName);
                _childInstances.Add(instName, res);
            }

            return res;
        }

        private void PrettyPrint()
        {
            /*StringBuilder res = new StringBuilder();

            int nCurlyBraces = 0;
            for (int i = 0; i < res.Length; i++)
            {
                char c = res[i];
                if (c == '{')
                    nCurlyBraces++;
                else if (c == '}')
                    nCurlyBraces++;
            }*/
        }

        internal String GetFileExt()
        {
            return _templ.GetFileExt();
        }

        internal void SetNamespace(string nameSpace)
        {
            StringBuilder namespaceOpen = new StringBuilder();
            StringBuilder namespaceClose = new StringBuilder();
            StringBuilder namespaceUsing = new StringBuilder();

            string[] namespaces = nameSpace.Split('.');
            if (nameSpace.Length > 0 && namespaces != null)
            {
                foreach (String ns in namespaces)
                {
                    namespaceOpen.Append("namespace ").Append(ns).Append(" {\n");
                    namespaceClose.Append("} //namespace ").Append(ns).Append("\n");
                    namespaceUsing.Append("using namespace ").Append(ns).Append(";\n");
                }
            }

            SetValue("namespaceStart", namespaceOpen.ToString());
            SetValue("namespaceEnd", namespaceClose.ToString());
            SetValue("namespaceUsing", namespaceUsing.ToString());
        }

        internal CodeTemplate GetTemplate() { return _templ; }

        private StringBuilder _data;
        private String _name;
        private CodeTemplate _templ;
        private Dictionary<String, CodeTemplateInstance> _childInstances = null;
    }
}
