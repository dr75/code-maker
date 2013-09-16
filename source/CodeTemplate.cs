using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CodeNavigator
{
    class CodeTemplate
    {
        public CodeTemplate(String name)
        {
            _name = name;

            int pos = _name.LastIndexOf('.');
            if (pos != -1)
                _fileExt = _name.Substring(pos).ToLower();

            LoadFromFile();
        }

        public String GetTemplate() { return _template; }

        private void LoadFromFile()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            int endOfDir = assemblyPath.LastIndexOf("\\");
            String assemblyDir = (endOfDir != -1 ? assemblyPath.Substring(0, endOfDir) : "");
            String templPath = assemblyDir + "\\templates\\" + _name;

            try
            {
                _template = System.IO.File.ReadAllText(templPath);
                if (_template.StartsWith("//@FastCode"))
                {
                    int p = _template.IndexOf("\n");
                    if (p != -1)
                        _template = _template.Substring(p + 1);
                }

            }
            catch {
                Debug.Assert(false);
            }
        }

        public CodeTemplateInstance NewInstance(String name)
        {
            return new CodeTemplateInstance(this, name);
        }

        public String GetFileExt()
        {
            return _fileExt;
        }

        internal CodeTemplate AddCodeTemplate(String name)
        {
            if (_childTemplates == null)
                _childTemplates = new Dictionary<String, CodeTemplate>();

            CodeTemplate templ = new CodeTemplate(name);
            _childTemplates.Add(name, templ);
            return templ;
        }

        internal String GetName() { return _name; }

        String _name;
        String _fileExt;
        String _template;

        private Dictionary<String, CodeTemplate> _childTemplates = null;

        internal Dictionary<String, CodeTemplate> GetChilds()
        {
            return _childTemplates;
        }
    }
}
