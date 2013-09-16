using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class ClassRefactor
    {
        internal ClassRefactor(CodeGenerator generator, ProjectItem srcItem, String folder, String origName)
        {
            _srcItem = srcItem;
            _folder = folder;
            _origName = origName;
            _generator = generator;
            _bExtract = false;

            _codeTemplatesNewClass = _generator.GetNewClassTemplates();
            _codeTemplateClassDef = _generator.GetClassDefTemplate();
        }

        internal void SetContent(string content, bool bContentIsClassDef)
        {
            _content = content;
            _bContentIsClassDef = bContentIsClassDef;
        }

        internal ProjectItem ExtractClass()
        {
            _bExtract = true;
            return NewClass();
        }
        
        // Create a new class file
        internal ProjectItem NewClass()
        {
            _codeItem = _generator.CreateCodeItem(_srcItem);

            InitNameForNewClass();
            

            String folderWithName = _folder + _newName;

            ProjectItem res = null;
            int count = 0;
            foreach (CodeTemplate t in _codeTemplatesNewClass)
            {
                CodeTemplateInstance tInst = t.NewInstance(_newName);
                FillTemplate(tInst);

                count++;
            }

            return res;
        }

        private void FillTemplate(CodeTemplateInstance tInst)
        {
            if (_bContentIsClassDef)
                tInst.SetValue("classDef", _content);
            else if (_codeTemplateClassDef != null)
                tInst.SetValue("classDef", _codeTemplateClassDef.GetTemplate());

            //if (content.Length > 0)
            {
                tInst.SetCtorDecl("");
                tInst.SetCtorImpl("");
                tInst.SetDtorDecl("");
                tInst.SetDtorImpl("");
            }

            //use srcItem to fill the code template (e.g. extract the namespace)
            tInst.SetNamespace(_nameSpace);

            //set given content
            SetClassContent(tInst);

            //find collection to store the item
            ProjectItems items = _codeItem.GetCollectionForItem(tInst.GetTemplate().GetFileExt());

            //check if the item exists
            if (CodeItem.GetProjectItem(items, tInst.GetOutputFileName()) != null)
            {
                String err = "File '" + tInst.GetOutputFileName() + "' already exists!";
                //if (count == 0)
                throw new Exception(err);
            }

            //create file and add to project
            ProjectItem item = _generator.AddProjectItem(items, _folder + tInst.GetOutputFileName(), tInst.ToString());
            if (_newItem == null)
                _newItem = item;

            if (item != null)
                _generator.FormatDocument(item);
        }

        private void InitNameForNewClass()
        {
            _newName = _origName;
            while (true)
            {
                //get the new name
                String qualName = GetNameForNewClassFromUser(_newName);

                //get the namesace and class from full name
                if (!SplitReverse(qualName, '.', out _nameSpace, out _newName))
                    _newName = qualName;

                //if no name was given we abort
                if (_newName.Length == 0)
                    throw new Exception("abort");

                //check if name already exists
                ProjectItem exFile = CodeItem.GetProjectItemIgnoreExt(_srcItem.ContainingProject.ProjectItems, _newName);
                if (exFile != null)
                {
                    System.Windows.MessageBox.Show("'" + exFile.Name + "' already exists!");
                    continue;
                }

                //name is OK
                break;
            }
        }

        private String GetNameForNewClassFromUser(String def)
        {
            String ns = _codeItem.GetNamespaceAsString('.');

            String qualName;
            if (ns.Length == 0)
                qualName = def;
            else
                qualName = ns + '.' + def;

            String txt = "Provide a name for the new class including the namespace to create.\n\n"
                        + "If a file already exists, you will get an error message.";

            String newName = Microsoft.VisualBasic.Interaction.InputBox(txt, "New Class", qualName);
            if (newName == null)
                return "";
            else
                return newName.Replace("::", ".");
        }

        /// <summary>
        /// Split string at last pos of delim and return first and second part.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delim"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>if delim was found</returns>
        bool SplitReverse(String s, char delim, out String first, out String second)
        {
            int pDelim = s.LastIndexOf(delim);
            if (pDelim == -1)
            {
                first = "";
                second = "";
                return false;
            }

            first = s.Substring(0, pDelim);
            second = s.Substring(pDelim + 1, s.Length - pDelim - 1);
            return true;
        }
        
        //set the class content in a extract class refactoring
        protected void SetClassContent(CodeTemplateInstance tInst)
        {
            String content = _content;

            //extract methods from content and replace the orig name with the new name
            if (    _origName.Length > 0 
                && (_codeItem.IsCppHeader() || _codeItem.IsCppImpl()) )
            {
                CodeItemCpp cppItem = (CodeItemCpp)_codeItem;

                //extract the decl. or definition depending on whether 
                if (CodeItemCpp.IsHeader(tInst.GetFileExt()))
                    content = cppItem.ExtractMethodDecls(content, _origName, _bExtract);
                else
                    content = cppItem.ExtractMethodImpls(content, _origName, _bExtract);


                //replace class name in src
                String newName = tInst.GetName();
                content = CodeTransform.ReplaceKeyword(content, _origName, newName);
            }

            //now set the content
            tInst.SetValue("content", content + "\n");
        }

        ProjectItem _srcItem;

        //orig class name to copy / extract members from
        String _origName;

        //new name for the extracted class
        String _newName;

        //the newly created project item
        ProjectItem _newItem;

        //namespace for the new class
        String _nameSpace;

        String _folder;

        //entire class definition or class content
        String _content;

        //true if content is a class definition
        bool _bContentIsClassDef;

        //true if this is an extract class refactoring
        bool _bExtract;

        CodeGenerator _generator;
        CodeItem _codeItem;

        private List<CodeTemplate> _codeTemplatesNewClass = null;
        private CodeTemplate _codeTemplateClassDef = null;
    }
}
