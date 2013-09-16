using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class CodeItemCs : CodeItem
    {
        internal CodeItemCs(ProjectItem item)
            :base(item)
        {}
    }
}
