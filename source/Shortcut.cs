using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    /// <summary>Implements a shortcut the is identified by the text typed to create it.</summary>
    public class Shortcut
    {
        /// <summary>
        /// The the key to lookup while typing
        /// </summary>
        public String shortcut { get; set; }

        /// <summary>
        /// The text of the shortcut that replaces the typed key
        /// </summary>
        public String text { get; set; }

        /// <summary>
        /// the position of the cursor in the replaced text
        /// </summary>
        public int cursor { get; set; }

        /// <summary>
        /// Create a new shortcut
        /// </summary>
        /// <param name="shortcut">the key to lookup while typing</param>
        /// <param name="text">the replaced text</param>
        public Shortcut(String shortcut, String text)
        {
            //we add a placeholder for correctly indenting the cursor
            //position when formating the pasted text
            this.cursor = text.IndexOf("$");
            if (cursor != -1)
            {
                this.text = text.Replace('$', 'x');
                //this.text = text.Remove(cursor, 1);
            }
            else
            {
                this.text = text + 'x';
                this.cursor = text.Length;
            }

            this.shortcut = shortcut;
        }

        /// <summary>
        /// returns true if the shortcut ends with str
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool PartialMatch(String str)
        {
            return this.shortcut.EndsWith(str);
        }
    }

}
