using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;

namespace CodeNavigator
{
    /// <summary>
    /// 
    /// </summary>
    internal class AutoComplete
    {
        internal AutoComplete(DocumentHandler documentHandler)
        {
            _documentHandler = documentHandler;
            _applicationObject = documentHandler.GetApplicationObject();
            _shortcuts = new Shortcuts(documentHandler.GetLanguage());

            InitShortcuts();
        }

        internal void OnShortcutsChanged()
        {
            InitShortcuts();
        }

        void InitShortcuts()
        {
            try
            {
                AutoCompletionFile file = new AutoCompletionFile(this);
                file.Read();
            }
            catch (Exception )
            {

            }
        }

        internal String GetShortcutPath() { return _shortcuts.Path;  }

        /// <summary>
        /// get the shurtcuts defined
        /// </summary>
        /// <returns></returns>
        internal Shortcuts GetShortcuts() { return _shortcuts; }

        internal void SetShortcuts(Shortcuts shortcuts) { _shortcuts = shortcuts; }

        
        /// <summary>
        /// TODO: move to separate class
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="Keypress"></param>
        /// <returns></returns>
        internal virtual bool DoAutoComplete(EnvDTE.TextSelection selection, string Keypress)
        {
            if (!_shortcuts.HasSubstring(Keypress))
                return false;

            EditPoint ep = selection.ActivePoint.CreateEditPoint();
            EditPoint sp = ep.CreateEditPoint();

            //the shortcut we lookup
            Shortcut shortcut = null;
            String matchWord = Keypress;
            bool bStartOfDocument = false;

            //move to left until whitespace
            while (true)
            {
                //move one char left and get current text for lookup
                sp.CharLeft(1);
                string txt = sp.GetText(ep) + Keypress;

                //check for begin of word
                bool beginOfWord = !CodeAnalyzer.IsIdentifierChar(txt[0])
                                || txt.Length>1 && !CodeAnalyzer.IsIdentifierChar(txt[1])
                                || bStartOfDocument;

                //check begin of word
                if (beginOfWord && (shortcut = _shortcuts.Get(matchWord)) != null)
                {
                    //if cursor was at start of document before the last call to CharLeft, we must not move right
                    if (!bStartOfDocument)
                        sp.CharRight();

                    break;
                }

                //check if we can stop here by moving the key cursor in the
                //list of shortcuts
                if (!_shortcuts.HasSubstring(txt))
                    break;

                //begin of doc and no shortcut found
                if (bStartOfDocument)
                    break;

                //save the current txt for lookup in next round
                matchWord = txt;
                bStartOfDocument = sp.AtStartOfDocument;
            }

            //lookup shortcut
            if (shortcut == null)
                return false;

            //insert the new char to support undo
            ep.Insert(Keypress);

            bool bUndoOpen = _applicationObject.UndoContext.IsOpen;
            if (!bUndoOpen)
                _applicationObject.UndoContext.Open("AutoComplete");

            //- insert text 
            //- set cursor to correct position within text
            //- format new text

            //save start point
            EditPoint start = sp.CreateEditPoint();

            //replace selection with new text
            sp.Delete(matchWord.Length /*- 1*/);
            sp.Insert(shortcut.text);

            //save end point
            EditPoint end = sp.CreateEditPoint();

            //move cursor to position indicated in shortcut
            sp.CharLeft(shortcut.text.Length - shortcut.cursor);
            selection.MoveToPoint(sp);

            //format
            start.SmartFormat(end);

            //delete the placeholder needed for text formating to work
            //this is required because VS would not correctly indent the
            //current cursor position; so we have added a placeholder at
            //cursor position that will be formated correctly and we now 
            //delete it
            sp.Delete(1);

            if (!bUndoOpen)
                _applicationObject.UndoContext.Close();

            return true;
        }

        /// <summary>
        /// Add a file extension that is used with this autocomplete object
        /// </summary>
        /// <param name="ext">file extension without "."</param>
        internal void AddFileExt(String ext)
        {
            _documentHandler.AddFileExt(ext);
        }

        private DTE2 _applicationObject;

        private Shortcuts _shortcuts;

        private DocumentHandler _documentHandler;
    }
}
