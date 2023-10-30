rem global
SET UnityBin="C:\Program Files\Unity\Hub\Editor\2021.3.31f1\Editor\Unity.exe"

rem prepare build cmd
SET ScriptPath=%~dp0
SET ScriptPath=%ScriptPath:~0,-1%

SET ProjectPath=%ScriptPath%
SET BuildCmd=%UnityBin% -quit -buildTarget android -batchmode -projectPath "%ProjectPath%" 

rem ==========================================================================================
rem build 2021_3_version1 patch
rem ==========================================================================================
@echo "start to build 2021_3_version1"

cd %ProjectPath%
git stash
git reset HEAD  --hard
git checkout 2021_3_version1
del %ProjectPath%\Assets\AndroidIl2cppPatchDemo\PrebuiltPatches\AllAndroidPatchFiles_version1.zip
%BuildCmd% -executeMethod AndroidBuilder.BuildPatch -logFile build_version1.log
if not exist "%ProjectPath%\Assets\AndroidIl2cppPatchDemo\PrebuiltPatches\AllAndroidPatchFiles_version1.zip" (
    echo "Build 2021_3_version1 Failed!"
	pause
	exit -1
)

rem ==========================================================================================
rem build 2021_3_version2 patch
rem ==========================================================================================
@echo "start to build 2021_3_version2"

cd %ProjectPath%
git reset HEAD  --hard
git checkout 2021_3_version2
del %ProjectPath%\Assets\AndroidIl2cppPatchDemo\PrebuiltPatches\AllAndroidPatchFiles_version2.zip
%BuildCmd% -executeMethod AndroidBuilder.BuildPatch -logFile build_version2.log
if not exist "%ProjectPath%\Assets\AndroidIl2cppPatchDemo\PrebuiltPatches\AllAndroidPatchFiles_version2.zip" (
    echo "Build 2021_3_version2 Failed!"
	pause
	exit -1
)

rem ==========================================================================================
rem build base apk
rem ==========================================================================================
@echo "start to build base version apk"

cd %ProjectPath%
git reset HEAD  --hard
git checkout 2021_3_base
%BuildCmd% -executeMethod AndroidBuilder.BuildWithoutPatch -logFile build_version0.log
if not exist "%ProjectPath%\AndroidGradleProject_v1.0\cn.noodle1983.unitypatchdemo.apk" (
    echo "Build Failed! Please Rerun %ProjectPath%\AndroidGradleProject_v1.0\Test\src\main\build_apk.bat to check the error."
	exit -1
)

copy /Y %ProjectPath%\AndroidGradleProject_v1.0\cn.noodle1983.unitypatchdemo.apk  %ScriptPath%\Il2cppDemo_com.test.test.apk
echo "Done!"
exit 0
