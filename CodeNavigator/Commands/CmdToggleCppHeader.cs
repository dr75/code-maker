using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator.Commands
{
    class CmdToggleCppHeader : VsCommand
    {
        public CmdToggleCppHeader(FastCode fastCode)
            :base(fastCode, "ToggleCppHeader", "Toggle between cpp and header", 216)
        {
            
        }

        public override bool Exec()
        {
            //get the current project item and create a new file at the 
            //same position in the project tree & disk folder

            DocumentHandler handler = GetDocumentHandler();
            if (handler == null)
                return false;

            DocumentHandlerCpp cpp = handler.GetCppHandler();
            if (cpp == null)
                return false;

            cpp.ToggleCppHeader();

            return true;
        }
    }
}
