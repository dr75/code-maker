using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.IO;
using System.Diagnostics;

namespace CodeNavigator
{
    class AutoCompletionFile
    {
        public AutoCompletionFile(AutoComplete autoComplete)
        {
            this._autoComplete = autoComplete;
        }

        String ReadText()
        {
            String res = "";
            while (_xmlReader.Read())
            {
                switch (_xmlReader.NodeType)
                {
                    //get the text
                    case XmlNodeType.Text:
                        res = _xmlReader.Value;
                        break;

                    //break if there is an opening or closing tag
                    case XmlNodeType.Element:
                        return res;
                    case XmlNodeType.EndElement:
                        return res;
                }
            }

            return res;
        }

        void ReadShortcut()
        {
            String key = "";
            String value = "";

            while (_xmlReader.Read())
            {
                if (_xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (_xmlReader.Name.Equals("key"))
                        key = ReadText();
                    else if (_xmlReader.Name.Equals("value"))
                        value = ReadText();
                }
                else if (_xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    //skip until "</shortcut>"
                    if (_xmlReader.Name.Equals("shortcut"))
                        break;
                }
            }

            if (   key.Length   > 0 && value.Length > 0)
                _shortcuts.AddShortcut(key, value);
        }

        private void ReadFileTypes()
        {
            String strTypes = ReadText();
            String[] types = strTypes.Split(',');
            foreach (string type in types)
            {
                _autoComplete.AddFileExt(type.Trim());
            }
        }

        private void ReadRootElement()
        {
            switch (_xmlReader.Name)
            {
                case "shortcut": ReadShortcut(); break;
                case "files"   : ReadFileTypes(); break;
            } 
        }

        internal static String GetShortcutFolder()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            int endOfDir = assemblyPath.LastIndexOf("\\");
            String assemblyDir = (endOfDir != -1 ? assemblyPath.Substring(0, endOfDir) : "");
            return assemblyDir;
        }

        public bool Read()
        {
            _shortcuts = new Shortcuts(_autoComplete.GetShortcuts().Language);

            String fileName = _shortcuts.Language + ".shortcuts";
            String path = GetShortcutFolder() + "\\" + fileName;
            _shortcuts.Path = path;

            //Textreader tr = new StreamReader("date.txt");
            using (_xmlReader = XmlReader.Create(path))
            {
                // Parse the file and display each of the nodes.
                while (_xmlReader.Read())
                {
                    switch (_xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            ReadRootElement();
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            break;
                        case XmlNodeType.Comment:
                            break;
                        case XmlNodeType.EndElement:
                            break;
                    }
                }
            }

            _autoComplete.SetShortcuts(_shortcuts);

            return true;
        }

        private Shortcuts _shortcuts;

        private AutoComplete _autoComplete;

        private XmlReader _xmlReader;
    }
}
