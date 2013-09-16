using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace CodeNavigator
{
    class ClassRename
    {
        ProjectItem _item = null;
        String _code = null;
        String[] _nameSpace = null;
        String _oldID;
        String _newID;

        internal ClassRename(ProjectItem item, String code)
        {
            _item = item;
            _code = code;
        }

        internal void Rename(String[] nameSpace, String oldID, String newID)
        {
            this._nameSpace = nameSpace;
            this._oldID = oldID;
            this._newID = newID;
            RenameClassInItem();
        }

        private void RenameClassInItem()
        {
            //now replace item in code if it is either of the following
            //- fwd. decl within correct namespace
            //- usage within correct namespace or after correct using

            StringBuilder res = new StringBuilder();
            int p = 0;
            int lastPos = 0;
            while ((p = CodeAnalyzer.NextId(_code, _nameSpace, _oldID, p)) != -1)
            {
                //check if usage
                res.Append(_code.Substring(lastPos, p - lastPos));
                res.Append(_newID);
                p += _oldID.Length;
                lastPos = p;
            }

            //if lastpos has changed, we have replaced something
            if (lastPos != 0)
            {
                res.Append(_code.Substring(lastPos, _code.Length - lastPos));
                SaveFile(res.ToString());
            }
        }

        private void SaveFile(String newCode)
        {
            if (_item.Document != null)
                CodeItem.SaveTextToOpenDoc(_item.Document, newCode);
            else
                System.IO.File.WriteAllText(_item.Document.FullName, newCode);
        }

        internal String GetItemPath() { return _item.Document.FullName;  }
    }
}
