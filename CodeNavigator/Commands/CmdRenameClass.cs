using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class CmdRenameClass : VsCommand
    {
        public CmdRenameClass(FastCode fastCode)
            :base(fastCode, "RenameClass", "Rename a new class", 551)
        {
            
        }

        public override bool Exec()
        {
            //get the current project item and create a new file at the 
            //same position in the project tree & disk folder

            DocumentHandler handler = GetDocumentHandler();
            if (handler == null)
                return true;

            //get project item
            ProjectItem item = handler.GetCurrentProjectItem();
            CodeItem code = handler.Generator.CreateCodeItem(item);

            //get new class name from user
            String txt =  "Rename class " + code.GetClassName() + " and the corresponding file(s).\n\n"
                        + "Type a new name and choose OK. If a file already exists, you will get an error message.";

            String newName = Microsoft.VisualBasic.Interaction.InputBox(txt, "Rename " + code.GetClassName(), code.GetClassName());
            if (newName == null || newName.Length == 0 || newName == code.GetClassName())
                return true;

            //actual rename
            code.RenameClass(newName);

            return true;
        }
    }
}
