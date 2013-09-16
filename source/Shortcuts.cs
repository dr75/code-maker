using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeNavigator
{
    /// <summary>
    /// A lsit of shortcuts for a language
    /// </summary>
    internal class Shortcuts
    {

        private String language;

        /// <summary>
        /// the language these shortcuts are used for
        /// </summary>
        internal String Language { get { return language; }  }

        internal String Path { get; set; }

        /// <summary>
        /// Shortcuts for the given language
        /// </summary>
        /// <param name="language"></param>
        internal Shortcuts(String language)
        {
            this.language = language;
        }

        /// <summary>
        /// Add a new shortcut
        /// </summary>
        /// <param name="shortcutKey">the key used for completion</param>
        /// <param name="text">the replacement text</param>
        internal void AddShortcut(String shortcutKey, String text)
        {
            Shortcut shortcut = new Shortcut(shortcutKey, text);
            
            //for each substring starting at the first char add an entry
            //to the list
            for (int i = 0; i < shortcutKey.Length; i++)
            {
                String substr = shortcutKey.Substring(i, shortcutKey.Length - i);

                //lookup entry
                Shortcut dummy = null;
                bool bEntryExists = _shortcutMap.TryGetValue(substr, out dummy);

                //entry already there, check if it is a shortcut list
                if (i==0 && bEntryExists && dummy != _shortcutListDummy)
                {
                    _errors.Add(
                        "Shortcut '" + shortcutKey + 
                        "' used multiple times.");

                    //cannot add an entry with the same key
                    return;
                }
                
                //not there -> add
                if (i==0)
                {
                    //remove the dummy
                    if (bEntryExists)
                        _shortcutMap.Remove(substr);

                    _shortcutMap.Add(substr, shortcut);
                }
                else if (!bEntryExists)
                    //new entry
                    _shortcutMap.Add(substr, _shortcutListDummy);
                //otherwise, the dummy is already there
            }
		
            _shortcutList.Add(shortcut);
        }

        internal Shortcut Get(String key)
        {
            Shortcut shortcut = null;
            _shortcutMap.TryGetValue(key, out shortcut);
            if (shortcut == _shortcutListDummy)
                shortcut = null;
            
            return shortcut;
        }

        internal bool HasSubstring(String key)
        {
            return _shortcutMap.ContainsKey(key);
        }

        internal int Count()
        {
            return _shortcutList.Count;
        }



        //map from key to shortcut
        //- also includes all possible endings of keys and their mapping
        //  to a list of keywords; 
        //- e.g., fori results in the follwoing entries
        //          - i     -> List
        //          - ri    -> List
        //          - ori   -> List
        //          - fori  -> Shortcut
        //- currently we use a dummy entry instead the list because we do not need the list
        private System.Collections.Generic.Dictionary<String, Shortcut> _shortcutMap
            = new System.Collections.Generic.Dictionary<String, Shortcut>();

        //list of shortcuts
        private System.Collections.Generic.List<Shortcut> _shortcutList
            = new System.Collections.Generic.List<Shortcut>();

        public System.Collections.Generic.List<Shortcut> ShortcutList
        {
            get { return _shortcutList; }
            set { _shortcutList = value; }
        }

        private Shortcut _shortcutListDummy = new Shortcut("<<this is a dummy entry>>","");        
        private System.Collections.Generic.List<String> _errors = new System.Collections.Generic.List<String>();
    }
}
