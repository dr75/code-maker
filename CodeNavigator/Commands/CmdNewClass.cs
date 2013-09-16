using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator.Commands
{
    class CmdNewClass : VsCommand
    {
        public CmdNewClass(FastCode fastCode)
            :base(fastCode, "NewClass", "Create a new class", 530)
        {
            
        }

        public override bool Exec()
        {
            //get the current project item and create a new file at the 
            //same position in the project tree & disk folder

            DocumentHandler handler = GetDocumentHandler();
            if (handler != null)
            {
                ProjectItem newClass = handler.NewClass("", false);
                if (newClass != null && newClass.Open() != null)
                    newClass.Document.Activate();
            }

            return true;
        }
    }
}
