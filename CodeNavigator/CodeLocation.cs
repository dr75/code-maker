using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CodeNavigator
{
    class CodeLocation
    {
        //char offset in the code that includes comments
        //- if a comment ends at the end of the file, CharOffsetWithoutComments
        //  is equal code.Length (ie. not a valid char)
        internal int CharOffsetWithoutComments { get; set; }

        internal int CharOffsetDifference { get; set; }

        //char offset in the code when comments have been removed
        internal int CharOffsetWithComments
        {
            get
            {
                int diff = CharOffsetDifference;
                return (diff == -1 ? CharOffsetWithoutComments : CharOffsetWithoutComments + diff);
            }
        }

        internal CodeLocation()
        {
            CharOffsetWithoutComments = -1;
            CharOffsetDifference = -1;
        }

        internal CodeLocation(int offsetWithComments, int offsetWithoutComments)
        {
            CharOffsetDifference = offsetWithComments - offsetWithoutComments;
            CharOffsetWithoutComments = offsetWithoutComments;
            Debug.Assert(CharOffsetWithoutComments < CharOffsetWithComments);
        }

        internal bool HasOffset()
        {
            return CharOffsetDifference != -1;
        }
    }
}
