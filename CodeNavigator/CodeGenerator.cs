using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    abstract class CodeGenerator
    {
        //instance must be create via method create
        protected CodeGenerator() { }

        //--- public ---

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="language">the language ("cpp", "cs", TBD)</param>
        /// <returns></returns>
        internal static CodeGenerator Create(String language)
        {
            if (language.Equals("cpp"))
                return new CodeGeneratorCpp();
            else if (language.Equals("cs"))
                return new CodeGeneratorCs();
            else
                return null;
        }
        
        //--- protected ---

        protected CodeTemplate AddCodeTemplate(String name)
        {
            CodeTemplate templ = new CodeTemplate(name);
            _codeTemplatesNewClass.Add(templ);

            return templ;
        }

        //--- private ---

        internal ProjectItem AddProjectItem(ProjectItems items, String path, String content)
        {
            try
            {
                //TODO: check if item already exists
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine(content);
                tw.Close();
            }
            catch (Exception /*ex*/)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }

            //try to add to project
            try
            {
                return items.AddFromFile(path);
            }
            catch { }

            //return existing
            try
            {
                return items.Item(path);
            }
            catch { }

            return null;
        }

        internal abstract CodeItem CreateCodeItem(ProjectItem srcItem);

        internal void FormatDocument(ProjectItem item)
        {
            Window w = item.Open();
            w.Activate();

            try
            {
                //does not work with VS 2010
                item.DTE.ExecuteCommand("Edit.FormatDocument", "");
                w.Document.Save();

                //do not close -> we leave it open since they were just created
                //w.Close();
                return;
            }
            catch (Exception e)
            {
                String err = e.ToString();
            }

            //format failed -> try work-around
            try
            {
                TextDocument doc = (TextDocument)item.Document.Object(String.Empty);

                EditPoint start = doc.StartPoint.CreateEditPoint();
                EditPoint end = doc.EndPoint.CreateEditPoint();

                start.SmartFormat(end);
                w.Document.Save();
            }
            catch (Exception e)
            {
                String err = e.ToString();
            }
            
        }

        internal List<CodeTemplate> GetNewClassTemplates() { return _codeTemplatesNewClass; }
        internal CodeTemplate GetClassDefTemplate() { return _codeTemplateClassDef; }

        private List<CodeTemplate> _codeTemplatesNewClass = new List<CodeTemplate>();
        protected CodeTemplate _codeTemplateClassDef = null;
    }
}
