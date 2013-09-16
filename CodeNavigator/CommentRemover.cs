using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeNavigator
{
    class CommentRemover
    {
        private string code;
        private List<CodeLocation> charMapping;
        private int pCopyPos = 0;
        private int pCurrentPos = 0;
        private StringBuilder res = null;
        bool bMLC = false;
        bool bSLC = false;

        internal CommentRemover(string code, List<CodeLocation> charMapping = null)
        {
            this.code = code;
            this.charMapping = charMapping;
        }

        private bool FindComment()
        {
            bMLC = false;
            bSLC = false;

            while (pCurrentPos < code.Length)
            {
                pCurrentPos = code.IndexOf('/', pCurrentPos);

                //remaining text too short
                if (pCurrentPos == code.Length - 1)
                    pCurrentPos = -1;

                //end of doc
                if (pCurrentPos == -1)
                    return false;

                //check next char
                if (code[pCurrentPos + 1] == '/')  //SLC
                {
                    bSLC = true;
                    return true;
                }
                else if (code[pCurrentPos + 1] == '*')    //MLC
                {
                    bMLC = true;
                    return true;
                }

                //currently not '/', so we can inc.
                pCurrentPos++;
            }

            return false;
        }

        private void SkipComment()
        {
            if (!FindComment())
                return;

            //begin of SL comment
            //-> add text before comment and move to next line
            if (bSLC)
            {
                Append();
                MoveUntil("\n", false);

                //EOF but comment did not end
                if (pCurrentPos == -1)
                    pCopyPos = pCurrentPos;
            }

            //check for begin of multi line comment
            else if (bMLC)
            {
                Append();
                MoveUntil("*/", true);

                //EOF but comment did not end
                if (pCurrentPos == -1)
                    pCopyPos = pCurrentPos;
            }
        }

        internal String RemoveComments()
        {
            while (pCurrentPos < code.Length && pCurrentPos!=-1)
            {
                //skip the next comment
                SkipComment();
            }

            //no comments in doc?
            if (pCopyPos == 0)
                return code;
            else
            {
                //append the rest of the document
                Append();
                return res.ToString();
            }
        }

        private void Append()
        {
            if (res == null) 
                res = new StringBuilder();

            //nothing to append
            if (pCopyPos == -1)
                return;
            
            //if we need a char mapping, put the positions pCopyPos
            if (charMapping != null && pCopyPos!=0)
                //pCopyPos is the next char in code with comments; 
                //res.Length is the next char in code without comments
                charMapping.Add(new CodeLocation(pCopyPos, res.Length));  

            //if current pos is -1, append entire text
            int pEnd = (pCurrentPos == -1 ? code.Length : pCurrentPos);
            res.Append(code.Substring(pCopyPos, pEnd - pCopyPos));
        }

        private void MoveUntil(String delim, bool bSkipDelim)
        {
            pCurrentPos = code.IndexOf(delim, pCurrentPos);

            if (pCurrentPos == -1)
                return;

            //skip delim
            if (bSkipDelim)
                pCurrentPos += delim.Length;

            //move copy pos to current pos
            pCopyPos = pCurrentPos;
        }
    }
}
