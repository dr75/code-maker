SET ADDIN=FastCode - For Testing_Test
SET FOLDER=%USERPROFILE%\Documents\Visual Studio 2012\Addins
pushd "%FOLDER%"
ren "%ADDIN%Debug.Addin" "%ADDIN%Debug.Addin_"
ren "%ADDIN%Release.Addin" "%ADDIN%Release.Addin_"
popd

start "%VS110COMNTOOLS%\..\IDE\devenv.exe" FastCode.sln

Sleep 5

pushd "%FOLDER%"
ren "%ADDIN%Debug.Addin_" "%ADDIN%Debug.Addin"
popd
