rem global
SET UnityBin="C:\Program Files\Unity\Editor\Unity.exe"

rem prepare build cmd
SET ScriptPath=%~dp0
SET ScriptPath=%ScriptPath:~0,-1%

rem ==========================================================================================
rem build base apk
rem ==========================================================================================
@echo "start to build base version apk"
SET ProjectPath=%ScriptPath%
SET BuildCmd=%UnityBin% -quit -buildTarget android -batchmode -projectPath "%ProjectPath%" 

cd %ProjectPath%
%BuildCmd% -executeMethod AndroidBuilder.BuildWithoutPatch
if not exist "%ProjectPath%\AndroidGradleProject\Test\src\main\bin\com.test.test.apk" (
    echo "Build Failed! Please Rerun %ProjectPath%\AndroidGradleProject\Test\src\main\build_apk.bat to check the error."
	exit -1
)

copy /Y %ProjectPath%\AndroidGradleProject\Test\src\main\bin\com.test.test.apk  %ScriptPath%\Il2cppDemo_com.test.test.apk
echo "Done!"
exit 0
