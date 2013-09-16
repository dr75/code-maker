using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class DocumentHandler
    {
        internal DocumentHandler(DTE2 applicationObject, String language)
        {
            _applicationObject = applicationObject;
            _language = language;
            _codeGenerator = CodeGenerator.Create(language);
            _autoComplete = InitAutoComplete();
        }

        protected virtual AutoComplete InitAutoComplete()
        {
            return new AutoComplete(this);
        }

        internal virtual DocumentHandlerCpp GetCppHandler() { return null; }

        internal Document GetDocument() { return _handledDocument; }
        internal void SetDocument(Document doc, String fileExt)
        {
            _handledDocument = doc;
            _fileExt = fileExt;
        }

        internal String GetDocContent(object until = null)
        {
            return CodeItem.GetTextFromOpenDoc(_handledDocument, until);
        }

        /// <summary>
        /// get the active project
        /// </summary>
        /// <returns></returns>
        internal Project GetCurrentProject()
        {
            //get the location of the active document in the project tree 
            //and the document folder to create a new document at same position
            Array projects = (Array)_applicationObject.ActiveSolutionProjects;
            if (projects == null || projects.Length == 0)
                return null;

            return (Project)projects.GetValue(0);
        }

        private bool IsIdentifier(String text)
        {
            return CodeAnalyzer.IsIdentifier(text);
        }

        /// <summary>
        /// extract class refactoring for the current selection
        /// </summary>
        /// <returns></returns>
        internal ProjectItem ExtractClass()
        {
            //active doc
            Document doc = _applicationObject.ActiveDocument;
            if (doc == null)
                return null;

            //get the current selection as new content
            String content = ((TextSelection)doc.Selection).Text;

            //if it is an ID, use it as name for the new class and set content to ""
            if (IsIdentifier(content))
                content = "";

            //use "newClass"
            ProjectItem newClass = NewClass(content, true);

            //delete the selection
            if (content.Length > 0)
                ((TextSelection)doc.Selection).Delete();

            return newClass;
        }

        internal ProjectItem GetCurrentProjectItem()
        {
            //active doc
            Document doc = _applicationObject.ActiveDocument;

            if (doc == null)
                throw new Exception("You must open a document to execute this command!");

            //get project item            
            return doc.ProjectItem;
        }

        /// <summary>
        /// Create new class according to namespace at current cursor location.
        /// If a single word is selected, this will be used as the name of the 
        /// class.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="bExtract"></param>
        /// <returns></returns>
        internal ProjectItem NewClass(String content, bool bExtract)
        {
            //get project item
            ProjectItem item = GetCurrentProjectItem();

            //we use a default name
            String name = "";
            String folder = item.Document.Path;

            //if a single word is selected, we use it as the class name
            TextSelection selection = (TextSelection)item.Document.Selection;
            bool bContentIsClassDef = false;
            if (selection.Text.Length > 0)
            {
                String strSel = selection.Text;
                if (IsIdentifier(strSel))              //check that there are only valid chars
                    name = strSel;
                else
                {
                    int pStart = 0;
                    String className = CodeAnalyzer.FindFirstClassDef(CodeAnalyzer.RemoveComments(strSel), ref pStart);
                    if (className.Length != 0)
                    {
                        bContentIsClassDef = true;
                        name = className;
                    }
                }
            }
            else
            {
                //no text selected -> get ID at current cursor pos
                String ID = GetIdAtSelection(selection);
                if (ID.Length > 0
                    && Char.ToLower(ID[0]) != ID[0]             //should start uppercase
                    && CodeItem.GetProjectItemIgnoreExt(item.ContainingProject.ProjectItems, ID) == null)         //should not be a valid item
                    name = ID;
            }

            //create a new file
            ClassRefactor refactor = new ClassRefactor(_codeGenerator, item, folder, name);
            refactor.SetContent(content, bContentIsClassDef);

            return (bExtract ? refactor.ExtractClass() : refactor.NewClass());
        }

        private String GetIdAtSelection(TextSelection selection)
        {
            EditPoint ep = selection.ActivePoint.CreateEditPoint();
            EditPoint spStart = ep.CreateEditPoint();

            String ID = "";

            //move to left until whitespace
            while (!spStart.AtStartOfDocument)
            {
                //move one char left and get current text for lookup
                spStart.CharLeft(1);
                ID = spStart.GetText(ep);

                //check for begin of ID
                if (!CodeAnalyzer.IsIdentifierChar(ID[0]))
                {
                    spStart.CharRight(1);
                    break;
                }
            }

            //move to right until whitespace
            EditPoint spEnd = ep.CreateEditPoint();
            while (!spEnd.AtEndOfDocument)
            {
                //move one char left and get current text for lookup
                spEnd.CharRight(1);
                ID = spStart.GetText(spEnd);

                //check for begin of ID
                if (!CodeAnalyzer.IsIdentifierChar(ID[ID.Length - 1]))
                {
                    spEnd.CharLeft(1);
                    break;
                }
            }

            return spStart.GetText(spEnd);
        }

        internal bool IsDocumentHeader()
        {
            return _fileExt.Equals("h") || _fileExt.Equals("hh");
        }

        internal DTE2 GetApplicationObject()
        {
            return _applicationObject;
        }

        internal string GetLanguage()
        {
            return _language;
        }

        /// <summary>
        /// Add a file extension that is used with this autocomplete object
        /// </summary>
        /// <param name="ext">file extension without "."</param>
        internal void AddFileExt(String ext)
        {
            _fileExtensions.Add(ext.ToLower());
        }

        /// <summary>
        /// returns whether this object handles the given document based
        /// on the documents file extension
        /// </summary>
        /// <param name="ext">file extension in lower case without "."</param>
        /// <returns></returns>
        internal bool HandlesFileExt(String ext)
        {
            return _fileExtensions.Contains(ext);
        }

        protected String _fileExt;

        internal AutoComplete GetAutoComplete() { return _autoComplete; }

        protected DTE2 _applicationObject;

        internal CodeGenerator Generator { get { return _codeGenerator; } }
        private CodeGenerator _codeGenerator;

        private Document _handledDocument = null;
        protected String _language;

        private AutoComplete _autoComplete = null;

        //cache for handled files
        private HashSet<String> _fileExtensions = new HashSet<String>();
    }
}
