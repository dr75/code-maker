using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator.Commands
{
    class CmdEditShortcuts : VsCommand
    {
        public CmdEditShortcuts(FastCode fastCode)
            : base(fastCode, "EditShortcuts", "Edit shortcuts", 551)
        {
            
        }

        public override bool Exec()
        {
            AutoComplete handler = GetDocumentHandler().GetAutoComplete();
            if (handler == null)
                return false;

            String path = handler.GetShortcuts().Path;
            DTE2 dte = _fastCode.GetApplicationObject();
            EnvDTE.Window wnd = dte.ItemOperations.OpenFile(path /*, Constants.vsViewKindTextView*/);


            return true;
        }
    }
}
