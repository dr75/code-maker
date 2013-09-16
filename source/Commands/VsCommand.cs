using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNavigator
{
    abstract class VsCommand
    {
        private String _name;
        private String _label;  //the label used on buttons
        private String _fullName;
        private String _toolTip;
        private int _iconIdx;

        protected FastCode _fastCode;

        private static String _commandPrefix = "FastCode.";

        public static DTE2 _applicationObject = null;
        public static AddIn _addInInstance = null;

        /// <summary>
        /// A VS command that can be registered and handles actions
        /// </summary>
        /// <param name="fastCode"></param>
        /// <param name="name"></param>
        /// <param name="toolTip"></param>
        /// <param name="iconIdx"></param>
        public VsCommand(FastCode fastCode, String name, String toolTip, int iconIdx)
        {
            _name = name;
            _label = name;
            _fullName = _commandPrefix + name;
            _toolTip = toolTip;
            _iconIdx = iconIdx;
            _fastCode = fastCode;
        }

        public String GetFullName()
        {
            return _fullName;
        }

        public DocumentHandler GetDocumentHandler()
        {
            return _fastCode.GetDocumentHandler();
        }

        /// <summary>
        /// Register at VS
        /// </summary>
        public void Register(CommandBarPopup popup, CommandBar buttonBar)
        {
            //Add a command to the Commands collection:
            Command cmd = GetNamedCommand();
            
            //add the command if not existing
            bool bCreated = false;
            if (cmd == null)
            {
                bCreated = true;
                cmd = AddNamedCommand((int)vsCommandStyle.vsCommandStylePictAndText /*vsCommandStylePict*/);
            }

            if (cmd != null)
            {
                //only add to button bar if the command did not exist
                if (buttonBar != null && bCreated)
                    AddToolbarCommand(buttonBar, cmd, 1);

                //add to pop menu
                if (popup != null)
                    AddToolbarCommand(popup.CommandBar, cmd, 1);
            }
        }

        private Command LookupCommand(CommandBar bar, Command command)
        {
            //CommandBarControl ctrl = bar.Controls[GetCommandShortName(command.Name)];
            return null;
            /*
                    FindControl(command.GetType(), command.ID);*/
        }

        private void AddToolbarCommand(CommandBar bar, Command command, int position = 1)
        {
            try
            {
                if (command != null && bar != null)
                {
                    //check if already there
                    Command cmdFound = LookupCommand(bar, command);

                    //add only if not found
                    if (cmdFound == null)
                        command.AddControl(bar, position);
                }
            }
            catch (ArgumentException)
            {

            }
        }

        /// <summary>
        /// Add a named command to the IDE
        /// </summary>
        /// <param name="commandStyleFlags"></param>
        /// <returns></returns>
        private Command AddNamedCommand(int commandStyleFlags)
        {
            Commands2 commands = (Commands2)_applicationObject.Commands;
            //object[] contextGUIDS = new object[] { };

            try
            {
                Command cmd = commands.AddNamedCommand2(_addInInstance, _name,
                    _label, _toolTip, true, _iconIdx, /*ref contextGUIDS*/ null,
                    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                    commandStyleFlags,
                    vsCommandControlType.vsCommandControlTypeButton);

                return cmd;
            }
            catch (ArgumentException)
            {
                //String err = ex.ToString();
            }

            //already there -> in this case we lookup the command
            return GetNamedCommand();
        }

        private Command GetNamedCommand()
        {
            try
            {
                Commands2 commands = (Commands2)_applicationObject.Commands;
                return commands.Item(_fullName);
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <returns>true if command was handled</returns>
        public abstract bool Exec();
    }
}
