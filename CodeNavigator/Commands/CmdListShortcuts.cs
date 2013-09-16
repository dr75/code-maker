using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace CodeNavigator.Commands
{
    class CmdListShortcuts : VsCommand
    {
        public CmdListShortcuts(FastCode fastCode)
            : base(fastCode, "ListShortcuts", "List shortcuts", 1548)
        {
        }


        public override bool Exec()
        {
            AutoComplete handler = GetDocumentHandler().GetAutoComplete();
            if (handler == null)
                return false;

            EnvDTE.Window wnd = GetShortcutWnd();
            if (wnd != null)
                wnd.Visible = true;

            return true;
        }

        private EnvDTE.Window GetShortcutWnd()
        {
            if (_shortcutWindow == null)
            {
                // A toolwindow must be connected to an add-in, so this line 
                // references one.
                EnvDTE80.Windows2 wins2obj = (Windows2)_applicationObject.Windows;

                // This section specifies the path and class name of the windows 
                // control that you want to host in the new tool window, as well as 
                // its caption and a unique GUID.
                string assemblypath = Assembly.GetExecutingAssembly().Location;
                //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //"E:\\work\\CodeNavigator\\CodeNavigator\\bin\\Debug\\CodeNavigator.dll";
                string classname = "CodeNavigator.ShortcutListWindow";
                string guidpos = "{CDFE9B1B-40BA-4BC2-9CF4-AFA8C727533E}";
                string caption = "Files in Solution";

                // Create the new tool window and insert the user control in it.
                object ctlobj = null;
                _shortcutWindow = wins2obj.CreateToolWindow2(_addInInstance, assemblypath,
                    classname, caption, guidpos, ref ctlobj);
            }

            return _shortcutWindow;
        }

        private EnvDTE.Window _shortcutWindow;
    }
}
