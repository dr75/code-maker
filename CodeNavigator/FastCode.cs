using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using CodeNavigator;
using CodeNavigator.Commands;

//for an example on context menus see here (mid of page): http://www.mztools.com/articles/2011/MZ2011015.aspx

/// <summary>The object for implementing an Add-in.</summary>
/// <seealso class='IDTExtensibility2' />
public class FastCode : IDTExtensibility2, IDTCommandTarget
{
	/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
	public FastCode()
	{
        _INSTANCE = this;
		AddCommand(new CmdNewClass(this));
		AddCommand(new CmdExtractClass(this));
        AddCommand(new CmdRenameClass(this));
        AddCommand(new CmdShowProperties(this));
		AddCommand(new CmdListShortcuts(this));
        AddCommand(new CmdEditShortcuts(this));
        AddCommand(new CmdToggleCppHeader(this));
        AddCommand(new CmdToggleImplDecl(this));
	}

    internal static FastCode Instance
    {
        get { return FastCode._INSTANCE; }
    }

	//add a bar or return existing
	private CommandBar AddCommandBar(string name, MsoBarPosition position)
	{
		// Get the command bars collection
		CommandBars cmdBars = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars);
		CommandBar bar = null;

		try
		{
			try
			{
				// Create the new CommandBar
				bar = cmdBars.Add(name, position, false, false);
			}
			catch (ArgumentException)
			{
				// Try to find an existing CommandBar
				bar = cmdBars[name];
			}
		}
		catch
		{
		}

		return bar;
	}

	private CommandBarPopup AddCommandBarPopup(CommandBarPopup parent, String name)
	{
		CommandBarPopup popup = null;
		try
		{
			popup = (CommandBarPopup)parent.Controls[name];
		}
		catch
		{
			popup = (CommandBarPopup)parent.Controls.Add(MsoControlType.msoControlPopup);
			popup.Caption = name;
		}

		return popup;
	}

	private void AddCommand(VsCommand cmd)
	{
		_commands.Add(cmd.GetFullName(), cmd);
	}
		
	/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
	/// <param term='application'>Root object of the host application.</param>
	/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
	/// <param term='addInInst'>Object representing this Add-in.</param>
	/// <seealso class='IDTExtensibility2' />
	public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
	{
		_applicationObject = (DTE2)application;
		_addInInstance = (AddIn)addInInst;

		VsCommand._applicationObject = _applicationObject;
		VsCommand._addInInstance = _addInInstance;
		
		//ext_cm_UISetup is executed when initializing the addin the first time
		
		if (   connectMode == ext_ConnectMode.ext_cm_UISetup
			|| connectMode == ext_ConnectMode.ext_cm_Startup)
		{
			object []contextGUIDS = new object[] { };
			Commands2 commands = (Commands2)_applicationObject.Commands;
			string toolsMenuName = "Tools";

			EnvDTE80.Windows2 wins2obj = (Windows2)_applicationObject.Windows;
			EnvDTE.Window window = wins2obj.Item(EnvDTE.Constants.vsWindowKindOutput);

			/*
			OutputWindow outputWindow = (OutputWindow)window.Object;
			_outputPane = outputWindow.OutputWindowPanes.Add("FastCode");
			 */

			//Place the command on the tools menu.
			//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
			CommandBars cmdBars = ((CommandBars)_applicationObject.CommandBars);
			CommandBar menuBarCommandBar = cmdBars["MenuBar"];

			//Find the Tools command bar on the MenuBar command bar:
			CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
			CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;            
			CommandBarPopup popup = null;
			
			if (toolsPopup != null)
				popup = AddCommandBarPopup(toolsPopup, "FastCode");
			
			// Add new command bar with button
			CommandBar buttonBar = AddCommandBar("FastCode", MsoBarPosition.msoBarFloating);

			//register all commands
			foreach (KeyValuePair<String, VsCommand> cmd in _commands)
				cmd.Value.Register(popup, buttonBar);
		}


        if ( _textDocKeyEvents == null &&
            (connectMode == ext_ConnectMode.ext_cm_Startup || connectMode == ext_ConnectMode.ext_cm_AfterStartup))
            InitDocumentHandler();
	}

    DocumentHandler GetLastDocHandler()
    {
        if (_lastDocHandler == null)
            InitDocumentHandler();

        return _lastDocHandler;
    }

    private void InitDocumentHandler()
    {
        if (_lastDocHandler == null)
        {
            _documentHandlerList.Add(new DocumentHandlerCpp(_applicationObject));
            _documentHandlerList.Add(new DocumentHandler(_applicationObject, "cs"));
            RegisterShortcutChangeHandler();
            _lastDocHandler = _documentHandlerList[0];
        }

        //keypress handler
        if (_textDocKeyEvents == null)
        {
            EnvDTE80.Events2 events = (EnvDTE80.Events2)_applicationObject.Events;
            _textDocKeyEvents = (EnvDTE80.TextDocumentKeyPressEvents)events.get_TextDocumentKeyPressEvents(null);
            _textDocKeyEvents.BeforeKeyPress += new EnvDTE80._dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler(BeforeKeyPress);

            _commandEvents = events.get_CommandEvents(null, 0);
            _commandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(BeforeExecute);
        }
    }

    private void RegisterShortcutChangeHandler()
    {
        if (AutoCompletionFile.GetShortcutFolder().Length == 0)
            return;

        // Create a new FileSystemWatcher and set its properties.
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = AutoCompletionFile.GetShortcutFolder();

        /* Watch for changes in LastAccess and LastWrite times, and
            the renaming of files or directories. */
        watcher.NotifyFilter = NotifyFilters.LastWrite;

        // Only watch shortcut files.
        watcher.Filter = "*.shortcuts";

        // Add event handlers.
        watcher.Changed += new FileSystemEventHandler(OnShortcutsChanged);

        // Begin watching.
        watcher.EnableRaisingEvents = true;
    }

    private void OnShortcutsChanged(object sender, FileSystemEventArgs e)
    {
        foreach (DocumentHandler handler in _documentHandlerList)
        {
            if (handler.GetAutoComplete().GetShortcutPath().Equals(e.FullPath))
            {
                handler.GetAutoComplete().OnShortcutsChanged();
                break;
            }
        }
    }

	/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
	/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
	/// <param term='custom'>Array of parameters that are host application specific.</param>
	/// <seealso class='IDTExtensibility2' />
	public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
	{
		if (disconnectMode != ext_DisconnectMode.ext_dm_UISetupComplete && _textDocKeyEvents != null)
		{
			_textDocKeyEvents.BeforeKeyPress -= new
				_dispTextDocumentKeyPressEvents_BeforeKeyPressEventHandler
				(BeforeKeyPress);
		}
	}

	/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
	/// <param term='custom'>Array of parameters that are host application specific.</param>
	/// <seealso class='IDTExtensibility2' />		
	public void OnAddInsUpdate(ref Array custom)
	{
	}

	/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
	/// <param term='custom'>Array of parameters that are host application specific.</param>
	/// <seealso class='IDTExtensibility2' />
	public void OnStartupComplete(ref Array custom)
	{
	}

	/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
	/// <param term='custom'>Array of parameters that are host application specific.</param>
	/// <seealso class='IDTExtensibility2' />
	public void OnBeginShutdown(ref Array custom)
	{
	}
		
	/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
	/// <param term='commandName'>The name of the command to determine state for.</param>
	/// <param term='neededText'>Text that is needed for the command.</param>
	/// <param term='status'>The state of the command in the user interface.</param>
	/// <param term='commandText'>Text requested by the neededText parameter.</param>
	/// <seealso class='Exec' />
	public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
	{
		if(    neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
		{
			if (   _commands.ContainsKey(commandName)
				|| commandName.Equals("FastCode"))  //is this needed to enabled the popup menu??
				status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}
	}
	
	/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
	/// <param term='commandName'>The name of the command to execute.</param>
	/// <param term='executeOption'>Describes how the command should be run.</param>
	/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
	/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
	/// <param term='handled'>Informs the caller if the command was handled or not.</param>
	/// <seealso class='Exec' />
	public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
	{
		handled = false;
		if(    executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
		{
			VsCommand cmd = null;
			if (_commands.TryGetValue(commandName, out cmd))
				ExecuteCmd(cmd);
		}
	}

	private void ExecuteCmd(VsCommand cmd)
	{
		try
		{
			cmd.Exec();
		}
		catch (Exception e)
		{
            if (!e.Message.Equals("abort"))
            {
#if DEBUG
                MessageBox.Show("Command execution failed: " + e.ToString());
#else
                MessageBox.Show(e.Message);
#endif
            }
		}
	}

	/// <summary>
	/// Get the event handler for the current document.
	/// </summary>
	/// <returns></returns>
    internal DocumentHandler GetDocumentHandler()
	{
		if (_applicationObject.ActiveDocument==null)
			return null;

        if (GetLastDocHandler().GetDocument() == _applicationObject.ActiveDocument)
			return _lastDocHandler;

		String doc = _applicationObject.ActiveDocument.Name;
        DocumentHandler handler = null;

		int dot = doc.LastIndexOf('.');
		String ext = (dot == -1 ? doc : doc.Substring(dot + 1, doc.Length - dot - 1));
		String extLower = ext.ToLower();

		//lookup in cache and add if not found
		if (!_documentHandlers.TryGetValue(extLower, out handler))
		{
			//lookup handler for document type
            foreach (DocumentHandler item in _documentHandlerList)
			{
				if (item.HandlesFileExt(extLower))
				{
					handler = item;

					//store in cache
					_documentHandlers.Add(extLower, handler);
					break;
				}
			}                
		}

        if (handler!=null)
		    handler.SetDocument(_applicationObject.ActiveDocument, extLower);

        _lastDocHandler = handler;

        return _lastDocHandler;
	}

	private void BeforeKeyPress(string Keypress, EnvDTE.TextSelection selection, bool InStatementCompletion, ref bool CancelKeypress)
	{
        DocumentHandler handler = GetDocumentHandler();

		if (handler != null)
			CancelKeypress = handler.GetAutoComplete().DoAutoComplete(selection, Keypress);
	}

	private void BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
	{
        /*Command command = null;
        try
        {
            command = _applicationObject.Commands.Item(Guid, ID);
        }
        catch (ArgumentException )
        {
        }

		if (command != null)
		{
			//_outputPane.Activate();
			//_outputPane.OutputString("Command:" + command.Name + " Guid:" + Guid + " ID:" + ID + "\n");
		}
        */
		//if (command.Name.Equals(""))
		/*if (ID == 4)
		{
			foreach (Project project in _applicationObject.Solution.Projects)
			{
				foreach (ProjectItem item in project.ProjectItems)
				{
					CodeElement e = item.FileCodeModel.CodeElements.Item(0);
					MessageBox.Show(e.Name);
				}
			}
		}*/
		/*
			* 2  - Backspace
			* 3  - Return
			* 4  - Tab
			* 5  - Shift + Tab
			* 6  - ???
			* 7  - Cursor Left
			* 8  - Shift + Cursor Left
			* 9  - Cursor Right
			* 10 - Shift + Cursor Right
			* 11 - Cursor Up
			* 12 - Shift + Cursor Up
			* 13 - Cursor Down
			* 14 - Shift + Cursor Down
			* 15 - Copy
			* 16 - Cut 
			* 17 - Edit.Delete
			* 19 - Edit.LineStart
			* 23 - Edit.LineEnd
			* 26 - Paste
			**/
		/*if (ID >= 1 && ID <=100)
		{
			MessageBox.Show((command!=null ? command.Name : "null") + " - " + ID + " - " + command.ID);
		}*/

		/*
		//16-Cut, 26-Paste, 136-CommentSelection, 137-UnCommentSelection
		if (ID == 16 || ID == 26 || ID == 136 || ID == 137)
		{
		}
		//1113-Add Reference, 17-Remove Reference
		else if (ID == 1113 || ID == 17)
		{
				
		}*/
	}

	private DTE2 _applicationObject;
    internal DTE2 GetApplicationObject() { return _applicationObject; }

	private AddIn _addInInstance;
    internal AddIn GetAddin() { return _addInInstance;  }

	private EnvDTE80.TextDocumentKeyPressEvents _textDocKeyEvents;
	private CommandEvents _commandEvents;
	//private OutputWindowPane _outputPane;

    private List<DocumentHandler> _documentHandlerList = new List<DocumentHandler>();

	private Dictionary<String, VsCommand> _commands = new Dictionary<String, VsCommand>();

    private Dictionary<String, DocumentHandler> _documentHandlers = new Dictionary<String, DocumentHandler>();
    private DocumentHandler _lastDocHandler = null;
    private static FastCode _INSTANCE = null;
}
