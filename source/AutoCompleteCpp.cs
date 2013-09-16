using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;

namespace CodeNavigator
{
    class AutoCompleteCpp : AutoComplete
    {
        internal AutoCompleteCpp(DocumentHandlerCpp handler)
            :base(handler)
        {
            _documentHandler = handler;
        }

        internal override bool DoAutoComplete(EnvDTE.TextSelection selection, string Keypress)
        {
            if (base.DoAutoComplete(selection, Keypress))
                return true;
            //check if the user finishs creating a new method declaration
            else if (Keypress.Equals(";") && IsDocumentHeader())
                return OnSemicolonInHeader(selection);
            else
                return false;
        }

        private bool IsDocumentHeader()
        {
            return _documentHandler.IsDocumentHeader();
        }

        private bool OnSemicolonInHeader(EnvDTE.TextSelection selection)
        {            
            //check prev. char
            EditPoint ep = selection.ActivePoint.CreateEditPoint();
            EditPoint sp = ep.CreateEditPoint();

            //move one char left and get current text for lookup
            //TODO: check what happens at begin of doc.
            sp.CharLeft(1);
            string prevChar = sp.GetText(ep);
            int offset = selection.ActivePoint.AbsoluteCharOffset - 1;

            //if prev char is a semikolon switch to impl
            if (prevChar.Equals(";"))
            {
                //return true to cancel the keypress if successful
                return _documentHandler.SwitchToMethodImpl(offset);
            }
            else
            {
                _documentHandler.CreateMethodImpl(offset);
                //return false to NOT CANCEL the keypress
                return false;
            }            
        }

        private DocumentHandlerCpp _documentHandler;
    }
}
