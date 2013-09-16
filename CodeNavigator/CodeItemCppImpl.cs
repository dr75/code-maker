using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class CodeItemCppImpl : CodeItemCpp
    {
        internal CodeItemCppImpl(ProjectItem item)
            :base(item)
        {}

        internal override bool IsCppImpl() { return true; }

        internal void SetCppHeader(CodeItemCppHeader header)
        {
            _header = header;
        }

        internal override CodeItemCppHeader GetCppHeader()
        {
            if (_header == null)
            {
                //try to find the corresponding header
                ProjectItems items = _projectItem.ContainingProject.ProjectItems;
                _header = CodeItemCppHeader.Lookup(items, GetClassName());

                //still not found?
                if (_header != null)
                    _header.SetCppImpl(this);
            }
                
            return _header;
        }

        internal override CodeItemCppImpl GetCppImpl()
        {
            return this;
        }

        internal override CodeItemCpp GetCorrespondingItem()
        {
            return GetCppHeader();
        }

        CodeItemCppHeader _header = null;
    }
}
