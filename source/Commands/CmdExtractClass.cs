using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator.Commands
{
    class CmdExtractClass : VsCommand
    {
        public CmdExtractClass(FastCode fastCode)
            : base(fastCode, "ExtractClass", "Extract class from selection", 59)
        {
            
        }

        public override bool Exec()
        {
            DocumentHandler handler = GetDocumentHandler();
            if (handler == null)
                return true;

            //use "newClass"
            ProjectItem newClass = handler.ExtractClass();

            //after creation, the correct item should be open because it
            //was formatted last
            /*TODO: DELETE
             * if (newClass != null && newClass.Open() != null)
                //activate the document
                newClass.Document.Activate();*/

            return true;
        }
    }
}
