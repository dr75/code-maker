using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    class CodeManipulate
    {
        internal CodeManipulate(CodeItem item)
        {
            _item = item;
        }

        internal void Delete(CodeFragment fragment)
        {
            if (_toDelete == null)
                _toDelete = new List<CodeFragment>();

            //sorted insert into delete list (sort by start position)
            for (int i = 0; i < _toDelete.Count; i++)
            {
                CodeFragment cf = _toDelete[i];
                if (cf.Start.CharOffsetWithComments > fragment.Start.CharOffsetWithComments)
                {
                    _toDelete.Insert(i, fragment);
                    return;
                }
            }

            _toDelete.Add(fragment);
        }

        internal void Commit()
        {
            if (_toDelete == null)
                return;

            //start with last fragment to make sure the text position
            //for deletion is correct
            for (int i = _toDelete.Count-1; i >= 0 ; i--)
            {
                CodeFragment cf = _toDelete[i];

                int start = cf.Start.CharOffsetWithComments;
                int end = cf.End.CharOffsetWithComments;

                _item.DeleteRange(start-1, end);
            }
        }

        CodeItem _item;
        List<CodeFragment> _toDelete;
    }
}
