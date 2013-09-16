VS ADDINS
-------------------------------------------------------------------------------
- cf.: http://msdn.microsoft.com/de-de/library/19dax6cz(v=vs.110).aspx
- The addins are loaded via a folder in the user documents:
	C:\Users\108014315\Documents\Visual Studio 2010\Addins
	C:\Users\108014315\Documents\Visual Studio 2012\Addins
- In this folder, there must be a ".AddIn" xml file that points to the location 
  of the assembly:
	<Assembly>E:\work\VsAddins\CodeNavigator\bin\CodeNavigator.dll</Assembly>
- The rest of the .AddIn file is the same as the kfile located in the project
- The file also contains the name of the class to be loaded:
	<FullClassName>CodeNavigator.Connect</FullClassName>


Debug Setup
-------------------------------------------------------------------------------
Setup for debugging with visual studio 2012:

1. Copy the files in _debug to folder "<user>\Documents\Visual Studio 2012\Addins"
2. Edit both files and set the path to the Addin project folder correctly
3. Start Visual Studio with "StartVS.bat". This renames the files such that the
   Addin is NOT LOADED. This is required to build release versions that can be
   used by other VS instances. After start, the batch file will activate the
   debug version of the addin.
4. Debug by setting the path to the external program to 
   C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe
5. To start instances of VS that sould use the release version of the plugin
   run "SwitchToRelease.bat" and start VS. Run "SwitchToDebug.bat" to enable
   debugging again.


Deploy Release Locally
-------------------------------------------------------------------------------
- Set the path to the release assembly in the .AddIn file
	<Assembly>E:\...\bin\Release\CodeNavigator.dll</Assembly>
- Addins usually work with both, VS 2010 and VS 2012
- What are the new features in VS 2012 that do not work with VS 2010???


Deploy using VSIX
-------------------------------------------------------------------------------
- Read: http://msdn.microsoft.com/en-us/library/dd997148.aspx
- Install VS SDK: http://www.microsoft.com/en-us/download/confirmation.aspx?id=30668

- Create new project
	- new project -> templates -> other -> extensibility -> VS package


VS Text Editor API
-------------------------------------------------------------------------------
The char offset of a selection seems to be based on index 1 and "\r\n" is 
represented by a single char

VS Icon ID List
http://www.mztools.com/articles/2008/MZ2008020.aspx