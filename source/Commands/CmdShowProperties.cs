using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeNavigator
{
    class CmdShowProperties : VsCommand
    {
        public CmdShowProperties(FastCode fastCode)
            :base(fastCode, "ShowProperties", "Show properties", 917)
        {
            
        }

        public override bool Exec()
        {
            return false;
        }
    }
}
