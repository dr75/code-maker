@echo off

SET ADDIN=FastCode - For Testing_Test
SET FOLDER=%USERPROFILE%\Documents\Visual Studio 2012\Addins
pushd "%FOLDER%"

IF EXIST "%ADDIN%Debug.Addin" goto STEP1EX
:STEP1END

IF EXIST "%ADDIN%Release.Addin_" goto STEP2EX
:STEP2END

popd
echo OK

sleep 2
goto :END

:STEP1EX
@echo rename "%ADDIN%Debug.Addin"
ren "%ADDIN%Debug.Addin" "%ADDIN%Debug.Addin_"
goto STEP1END

:STEP2EX
@echo rename "%ADDIN%Release.Addin_"
ren "%ADDIN%Release.Addin_" "%ADDIN%Release.Addin"
goto STEP2END

:END
