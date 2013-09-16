using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator.Commands
{
    class CmdToggleImplDecl : VsCommand
    {
        public CmdToggleImplDecl(FastCode fastCode)
            //: base(fastCode, "CmdToggleImplDecl", "Toggle between method impl. and decl.", 530)
            //currently we can only switch from decl to impl.
            : base(fastCode, "SwitchToImpl", "Switch to implementation", 216)
        {
            
        }

        public override bool Exec()
        {
            //get the current project item and create a new file at the 
            //same position in the project tree & disk folder

            DocumentHandler handler = GetDocumentHandler();
            if (handler == null)
                return false;

            DocumentHandlerCpp cppHandler = handler.GetCppHandler();
            if (cppHandler != null)
                cppHandler.ToggleDeclImpl();
            return true;
        }
    }
}
