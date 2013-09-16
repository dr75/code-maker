using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeNavigator
{
    class CodeFragment
    {
        public CodeFragment()
        {
            this.Start = new CodeLocation();
            this.End = new CodeLocation();
        }
        public CodeFragment(string code)
            :this()
        {
            this.Code = code;
        }

        public CodeLocation Start { get; set; }
        public CodeLocation End { get; set; }

        public int Length { 
            get { return Code == null ? 0 : Code.Length; } 
        }

        public string Code { get; set; }

        public override string ToString()
        {
            return Code;
        }

        internal virtual bool UpdateOffsetFromMapping(CodeItem code)
        {
            //check if we have already done this
            if (Start.HasOffset())
                return false;

            _code = code;
            List<CodeLocation> mapping = _code.GetSourceCharMapping();

            //scan through the mapping and correct start and end position
            int diff = 0;
            foreach (CodeLocation loc in mapping)
            {
                int pWithComments = loc.CharOffsetWithComments;
                int pWithoutComments = loc.CharOffsetWithoutComments;
                diff = pWithComments - pWithoutComments;

                //if the location is before the fragments position, update the
                //position in code with comments
                if (pWithoutComments <= Start.CharOffsetWithoutComments)
                    Start.CharOffsetDifference = diff;
                else if (Start.CharOffsetDifference == -1)
                    Start.CharOffsetDifference = 0;

                //the same for the end pos; break if bigger than end
                if (pWithoutComments <= End.CharOffsetWithoutComments)
                    End.CharOffsetDifference = diff;
                else
                    break;
            }

            return true;
        }

        CodeItem _code;
    }
}
