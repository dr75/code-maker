using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;

namespace CodeNavigator
{
    class CodeItem
    {
        internal static ProjectItem GetProjectItem(ProjectItems items, String name, String[] extensions)
        {
            foreach (ProjectItem item in items)
            {
                String itemName = item.Name;

                foreach (String ext in extensions)
                {
                    if (   itemName.Length == name.Length + ext.Length
                        && itemName.StartsWith(name)
                        && itemName.EndsWith(ext))
                        return item;
                }

                //check subitem recursively
                ProjectItem subItem = GetProjectItem(item.ProjectItems, name, extensions);
                if (subItem != null)
                    return subItem;
            }

            //not found
            return null;
        }

        internal static ProjectItem GetProjectItem(ProjectItems items, String name)
        {
            foreach (ProjectItem item in items)
            {
                if (item.Name.Equals(name))
                    return item;

                //check subitem recursively
                ProjectItem subItem = GetProjectItem(item.ProjectItems, name);
                if (subItem != null)
                    return subItem;
            }

            //not found
            return null;
        }

        /// <summary>
        /// Lookup name in items and ignore the file extension
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static ProjectItem GetProjectItemIgnoreExt(ProjectItems items, String name)
        {
            foreach (ProjectItem item in items)
            {
                if (    item.Name.StartsWith(name) 
                    && (item.Name.Length == name.Length || item.Name[name.Length]=='.'))
                    return item;

                //check subitem recursively
                ProjectItem subItem = GetProjectItemIgnoreExt(item.ProjectItems, name);
                if (subItem != null)
                    return subItem;
            }

            //not found
            return null;
        }


        internal CodeItem(ProjectItem item)
        {
            _projectItem = item;
        }

        internal ProjectItem GetProjectItem() { return _projectItem;  }

        internal ProjectItem GetProjectItem(String name)
        {
            return CodeItem.GetProjectItem(_projectItem.ContainingProject.ProjectItems, name);
        }

        internal virtual bool IsCppHeader() { return false;  }
        internal virtual bool IsCppImpl() { return false; }

        internal bool ActivateWindow(int selStart = -1, int selEnd = -1)
        {
            if (!EnsureIsOpen())
                return false;

            Document doc = _projectItem.Document;
            doc.Activate();
            if (selStart != -1)
            {
                //move cursor
                TextSelection sel = (TextSelection)doc.Selection;

                //move to start pos
                sel.MoveToAbsoluteOffset(selStart);

                //select to end
                if (selEnd != -1)
                    sel.MoveToAbsoluteOffset(selEnd, true);
            }
            return true;
        }

        protected bool EnsureIsOpen()
        {
            //if the item is not open, open it
            if (_projectItem.Document == null)
                _projectItem.Open();

            return _projectItem.Document != null;
        }

        /// <summary>
        /// Get the selected text from the beginning of the document until the 
        /// given position.
        /// </summary>
        /// <param name="until">position in document or null to return the entire doc</param>
        /// <returns></returns>
        private String GetText(object until = null)
        {
            if (!EnsureIsOpen())
                return null;

            return GetTextFromOpenDoc(_projectItem.Document, until).Replace("\r","");
        }

        internal static String GetTextFromOpenDoc(Document doc, object until = null)
        {
            //now get the text from the header up to the class def
            TextDocument textDoc = (TextDocument)doc.Object(String.Empty);
            EditPoint ep = textDoc.CreateEditPoint();

            return ep.GetText(until == null ? textDoc.EndPoint : until);
        }

        private void SetText(String newText, object until = null)
        {
            if (!EnsureIsOpen())
                return;

            SaveTextToOpenDoc(_projectItem.Document, newText, until);
        }

        internal static void SaveTextToOpenDoc(Document doc, String newText, object until = null)
        {
            //now get the text from the header up to the class def
            TextDocument textDoc = (TextDocument)doc.Object(String.Empty);
            EditPoint ep = textDoc.CreateEditPoint();

            ep.ReplaceText(until == null ? textDoc.EndPoint : until, newText, 
                (int)EnvDTE.vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
        }

        internal void SetContent(String newContent)
        {
            SetText(newContent);
        }

        /// <summary>
        /// Use this only if set content is the only operation! If using 
        /// multiple of such operations, the content will be read and written
        /// multiple times.
        /// </summary>
        /// <param name="oldWord"></param>
        /// <param name="newWord"></param>
        internal void ReplaceWord(String oldWord, String newWord)
        {
            String content = GetContent(false);

            if (content != null)
            {
                content = CodeTransform.ReplaceKeyword(content, oldWord, newWord);
                SetContent(content);
            }
        }

        internal virtual void ReplaceWordInClass(String oldWord, String newWord)
        {
            ReplaceWord(oldWord, newWord);
        }

        internal String GetContent(bool bRemoveComments = true)
        {
            //without comments requested and already there?
            if (bRemoveComments && _contentWithoutComments!=null)
                return _contentWithoutComments;

            //get content
            if (_contentWithComments==null)
                _contentWithComments = GetText(null);

            //remove comments if requested
            if (bRemoveComments)
            {
                _sourceCharMapping = new List<CodeLocation>();
                _contentWithoutComments = CodeAnalyzer.RemoveComments(
                                _contentWithComments, _sourceCharMapping);
                return _contentWithoutComments;
            }
            else
                return _contentWithComments;
        }
        
        internal String GetClassName()
        {
            if (_className == null)
            {
                String fileName = _projectItem.Name;
                int dot = fileName.LastIndexOf('.');
                _className = (dot == -1 ? fileName : fileName.Substring(0, dot));
            }

            return _className;
        }

        internal string GetNamespaceAsString(char delim)
        {
            List<String> namespaces = GetNamespace();

            if (namespaces == null)
                return "";

            StringBuilder res = new StringBuilder();
            foreach (String ns in namespaces)
            {
                if (res.Length > 0)
                    res.Append(delim);

                res.Append(ns);
            }

            return res.ToString();
        }

        //extract namespace from given document
        //document must be the active document
        internal List<String> GetNamespace()
        {
            if (_namespace==null)
                _namespace = GetNamespaceInt();

            return _namespace;
        }

        protected virtual List<String> GetNamespaceInt()
        {
            String text = GetContent();
            if (text == null)
                return null;

            //get all code until current cursor position
            int endOfText = 0;
            /*if (_projectItem.Document != null)
            {
                TextSelection sel = (TextSelection)_projectItem.Document.Selection;
                endOfText = sel.ActivePoint.AbsoluteCharOffset;
            }*/

            //if no selection, use the the first class def
            if (endOfText == 0)
                endOfText = CodeAnalyzer.FindFirstClassDef(text);

            if (endOfText > 0)
                text = text.Substring(0, endOfText);

            //remove comments and get namespaces
            return CodeAnalyzer.GetNamespaceFromCode(text);
        }

        internal ProjectItems GetCollectionForItem(String fileExt)
        {
            //lookup the an item with the given extension and put the new item 
            //in the same collection
            String fileName = _projectItem.Name;
            int dot = fileName.LastIndexOf('.');
            if (dot == -1)
                return _projectItem.Collection;

            String name = fileName.Substring(0, dot);
            String ext = fileName.Substring(dot);

            //same extension?
            if (fileExt.Equals(ext.ToLower()))
                return _projectItem.Collection;

            //other extension -> get corresponding class (e.g., get ".h" from ".cpp")            
            ProjectItem correspondingItem = CodeItem.GetProjectItem(_projectItem.ContainingProject.ProjectItems, name + fileExt);
            if (correspondingItem != null)
                return correspondingItem.Collection;
            else
                return _projectItem.Collection;
        }

        protected virtual void OpenAllFiles() { EnsureIsOpen();  }

        internal void RenameClass(String newName)
        {
            //open all corresponding files to make sure we can switch between them after renaming the first one
            OpenAllFiles();

            //get the class name before renaming
            String className = GetClassName();

            //rename files first; this will throw an exception if the file already exists
            RenameFiles(newName);

            //simply replace all words in all documents
            ReplaceWordInClass(className, newName);
            _className = newName;

            String[] nameSpace = null;
            List<ClassRename> renames = new List<ClassRename>();

            RenameClassInDependentFiles(renames, _projectItem.ContainingProject.ProjectItems, 
                nameSpace, className);

            if (renames.Count == 0)
                return;

            String text = "Also replace all occurrences of '" + className + "' in ";
            if (renames.Count == 1)
                text = text + "file '" + renames[0].GetItemPath() + "'?";
            else if (renames.Count < 20)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(text);
                builder.Append(" the following ");
                builder.Append(renames.Count);
                builder.Append(" files?\n");

                foreach (ClassRename rename in renames)
                    builder.Append('\n').Append(rename.GetItemPath());

                text = builder.ToString();
            }
            else
                text = text + renames.Count + " files?";
                

            MessageBoxResult dialogResult = MessageBox.Show(text, "Rename in Files", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                foreach (ClassRename rename in renames)
                    rename.Rename(nameSpace, className, newName);
            }
        }

        internal virtual bool IsThisOrRelatedItem(ProjectItem item)
        {
            return item == this._projectItem;
        }

        internal void RenameClassInDependentFiles(List<ClassRename> renames, ProjectItems items, 
            String[] nameSpace, String oldID)
        {            
            foreach (ProjectItem item in items)
            {
                if (item.Document != null && !IsThisOrRelatedItem(item))
                {
                    String code = ContainsWord(item, oldID);
                    if (code != null)
                    {
                        ClassRename rename = new ClassRename(item, code);
                        renames.Add(rename);
                    }
                }

                //check subitem recursively
                RenameClassInDependentFiles(renames, item.ProjectItems, nameSpace, oldID);
            }
        }

        /// <summary>
        /// Returns the code of the item if it contains the requested identifier
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal String ContainsWord(ProjectItem item, String name)
        {
            String code;

            //if the doc is open, load it from the window to also get unsaved text
            if (item.Document != null)
            {
                //load from open doc
                code = GetTextFromOpenDoc(item.Document);
            }
            else
            {
                //load from file
                String path = item.Document.FullName;
                code = System.IO.File.ReadAllText(path);
            }

            if (CodeAnalyzer.NextId(code, null, name, 0) != -1)
                return code;
            else
                return null;
        }

        /// <summary>
        /// Rename all related files according to the new item name (e.g., the new class name)
        /// </summary>
        /// <param name="newItemName"></param>
        internal virtual bool RenameFiles(String newItemName)
        {
            return RenameFile(newItemName);
        }

        protected bool RenameFile(String newItemName)
        {
            /*if (!EnsureIsOpen())
                return false;

            String fileName = _projectItem.Document.FullName;*/
            String fileName = _projectItem.Name;
            int dot = fileName.LastIndexOf('.');
            //int slash = fileName.LastIndexOf('\\');

            //String dir = fileName.Substring(0, slash + 1);
            //String name = (dot == -1 ? fileName : fileName.Substring(slash+1, dot - slash - 1));
            String ext = (dot == -1 ? "" : fileName.Substring(dot, fileName.Length - dot));

            //String newName = dir + newItemName + ext;
            String newName = newItemName + ext;
            _projectItem.Name = newName;
            //_projectItem.Save(newName);
            return true; //_projectItem.Document.Save(newName) == vsSaveStatus.vsSaveSucceeded;
        }

        internal void AppendString(String s, bool bEnsureLineEndBeforeAppend = true)
        {
            if (!EnsureIsOpen())
                return;

            //create an edit point at the end of the doc
            TextDocument textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");
            EditPoint start  = textDoc.EndPoint.CreateEditPoint();
            EditPoint insert = textDoc.EndPoint.CreateEditPoint();

            //if requested, add a linefeed if the last char is not a line feed
            if (bEnsureLineEndBeforeAppend && !insert.AtStartOfDocument)
            {
                EditPoint end = textDoc.EndPoint.CreateEditPoint();
                end.CharLeft(10);
                String text = end.GetText(insert).TrimEnd('\t', ' ');
                if (text.Length == 0 || text[text.Length - 1] != '\n')
                    insert.Insert("\n");
            }

            insert.Insert(s);
            start.SmartFormat(textDoc.EndPoint);
        }

        internal void InsertString(int charOffset, string s, bool bEnsureLineEndBeforeAppend)
        {
            if (!EnsureIsOpen())
                return;

            //create an edit point at the end of the doc
            TextDocument textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");

            EditPoint start = textDoc.CreateEditPoint();
            start.MoveToAbsoluteOffset(charOffset);

            EditPoint insert = textDoc.CreateEditPoint();
            insert.MoveToAbsoluteOffset(charOffset);

            //if requested, add a linefeed if the last char is not a line feed
            if (bEnsureLineEndBeforeAppend && !insert.AtStartOfDocument)
            {
                EditPoint tmp = textDoc.CreateEditPoint();
                int offset = (charOffset > 10 ? charOffset - 10 : 0);
                tmp.MoveToAbsoluteOffset(offset);
                String text = tmp.GetText(insert).TrimEnd('\t', ' ');
                if (text.Length == 0 || text[text.Length - 1] != '\n')
                    insert.Insert("\n");
            }

            insert.Insert(s);
            start.SmartFormat(insert);
        }

        internal void DeleteRange(int startOffset, int stopOffset)
        {
            if (!EnsureIsOpen())
                return;

            //create an edit point at the end of the doc
            TextDocument textDoc = (TextDocument)_projectItem.Document.Object("TextDocument");

            EditPoint start = textDoc.CreateEditPoint();

            //TODO: why do we have to add +3 here?
            start.MoveToAbsoluteOffset(startOffset + 3);

            EditPoint stop = textDoc.CreateEditPoint();

            //TODO: why do we have to add +2 here?
            stop.MoveToAbsoluteOffset(stopOffset + 2);

            start.Delete(stop);
        }

        internal List<CodeLocation> GetSourceCharMapping() { return _sourceCharMapping; }

        //the corresponding project item
        protected ProjectItem _projectItem;

        //content including comments (used for caching)
        private String _contentWithComments = null;

        //content excluding comments (used for caching)
        private String _contentWithoutComments = null;

        private String _className;

        List<String> _namespace = null;

        /*
         * Mapping from char offsets in code without comments
         * to char offsets in code with commen
         * - One entry for each char after end of a comment
         * - If there are no comment, the list is empty
         * - The object is only valid after calling GetContent(true);
         * 
         * Example: comment from char 28 until char 34
         * -------------------------------------------------------
         * without		with		offset	CodeLocation
         * comment		comment
         * -------------------------------------------------------
         * 27	-> 	    27		    0	    NA
         * 28	-> 	    35		    7	    (28,35)
         */
        List<CodeLocation> _sourceCharMapping = null;
    }
}
