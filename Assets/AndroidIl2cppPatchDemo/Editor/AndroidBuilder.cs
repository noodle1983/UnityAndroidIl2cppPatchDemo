﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AndroidBuilder : MonoBehaviour {

    //-----------------------------------------------------------------------------------
    public static readonly string PROJECT_DIR = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
    public static readonly string ANDROID_EXPORT_PATH = PROJECT_DIR + "/AndroidGradleProject_v1.0";
    public static string ANDROID_PROJECT_PATH { get { return ANDROID_EXPORT_PATH; } }
    public static string ANDROID_UNITYLIBRARY_PATH =  ANDROID_PROJECT_PATH + "/unityLibrary/";
    public static string ANDROID_MANIFEST_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/";
    public static string JAVA_SRC_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/java/";
    public static string JAR_LIB_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/libs/";
    public static string SO_DIR_NAME = "jniLibs";
    public static string SO_LIB_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/jniLibs/";
    public static string EXPORTED_ASSETS_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/assets";
    public static string R_JAVA_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/gen/";
    public static string LAUNCHER_RES_PATH = ANDROID_PROJECT_PATH + "/launcher/src/main/res";
    public static string LAUNCHER_MANIFEST_XML_PATH = ANDROID_PROJECT_PATH + "/launcher/src/main/AndroidManifest.xml";
    public static string RES_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/res";
    public static string MANIFEST_XML_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/AndroidManifest.xml";
    public static string JAVA_OBJ_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/objs/";
    public static string BUILD_SCRIPTS_PATH = ANDROID_PROJECT_PATH + "/unityLibrary/src/main/";
    public static string ZIP_PATH = PROJECT_DIR + "/Assets/AndroidIl2cppPatchDemo/Editor/Exe/zip.exe";

    public static string UNITY_EXE_PATH = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
    public static string UNITY_EXE_DIR = Path.GetDirectoryName(UNITY_EXE_PATH);
    public static string DEFAULT_SDK_PATH = UNITY_EXE_DIR + "/Data/PlaybackEngines/AndroidPlayer/SDK/";
    public static string DEFAULT_JDK_PATH = UNITY_EXE_DIR + "/Data/PlaybackEngines/AndroidPlayer/OpenJDK/";
    public static string DEFAULT_NDK_PATH = UNITY_EXE_DIR + "/Data/PlaybackEngines/AndroidPlayer/NDK/";
    public static string GRADLE_PROXY_STRING = " \"-Dhttp.proxyHost=127.0.0.1\" \"-Dhttp.proxyPort=1080\" \"-Dhttps.proxyHost=127.0.0.1\" \"-Dhttps.proxyPort=1080\" ";
    //public static string GRADLE_PROXY_STRING = "";

    static bool Exec(string filename, string args)
    {
        Debug.Log("========================================================");
        Debug.LogFormat("exec:{0}, args:{1}", filename, args);
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = filename;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;              
        process.OutputDataReceived += (sender, args) => Debug.Log(args.Data);
        process.ErrorDataReceived += (sender, args) => Debug.LogError(args.Data);

        int exit_code = -1;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log("--------------------------------------------------------");
            return false;
        }
        process.WaitForExit();
        exit_code = process.ExitCode;
        process.Close();
        Debug.LogFormat("exec:{0}, args:{1}, ret:{2}", filename, args, exit_code);
        Debug.Log("--------------------------------------------------------");
        return exit_code == 0;
    }
    
    public static bool ValidateConfig()
    {
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot", "");
        if (string.IsNullOrEmpty(sdkPath)) { sdkPath = DEFAULT_SDK_PATH; }
        if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
        {
            Debug.LogError("sdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        string jdkPath = EditorPrefs.GetString("JdkPath", "");
        if (string.IsNullOrEmpty(jdkPath)) { jdkPath = DEFAULT_JDK_PATH; }
        if (string.IsNullOrEmpty(jdkPath) || !Directory.Exists(jdkPath))
        {
            Debug.LogError("jdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        string ndkPath = EditorPrefs.GetString("AndroidNdkRootR16b", "");
        if (string.IsNullOrEmpty(ndkPath)) { ndkPath = EditorPrefs.GetString("AndroidNdkRoot", ""); }
        if (string.IsNullOrEmpty(ndkPath)) { ndkPath = DEFAULT_NDK_PATH; }
        if (string.IsNullOrEmpty(ndkPath) || !Directory.Exists(ndkPath))
        {
                Debug.LogError("ndk path is empty! please config via menu path:Edit/Preference->External tools.");
                return false;
            }

        Debug.Log("Build Env is ready!");
        Debug.Log("Build Options:");
        Debug.Log("SDK PATH=" + sdkPath);
        Debug.Log("JDK PATH=" + jdkPath);
        Debug.Log("NDK PATH=" + ndkPath);
        return true;
    }

    [MenuItem("AndroidBuilder/Step 1: Export Gradle Project", false, 101)]
    public static bool ExportGradleProject()
    {
        //build settings
        if (!ValidateConfig()) { return false; }

        PlayerSettings.applicationIdentifier = "cn.noodle1983.unitypatchdemo";
        PlayerSettings.companyName = "noodle1983";
        PlayerSettings.productName = "UnityAndroidIl2cppPatchDemo";
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        //export project
        string error_msg = string.Empty;
        string[] levels = new string[] { "Assets/AndroidIl2cppPatchDemo/Scene/0.unity" };
        BuildOptions options = BuildOptions.None;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        if (Directory.Exists(ANDROID_EXPORT_PATH)) { FileUtil.DeleteFileOrDirectory(ANDROID_EXPORT_PATH);}
        Directory.CreateDirectory(ANDROID_EXPORT_PATH);
        try
        {
            error_msg = BuildPipeline.BuildPlayer(levels, ANDROID_EXPORT_PATH, EditorUserBuildSettings.activeBuildTarget, options).summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? string.Empty : "Failed to export project!";
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }

        if (!string.IsNullOrEmpty(error_msg))
        {
            Debug.LogError(error_msg);
            return false;
        }

        //copy the prebuild patch to the assets directory instead of downloading.
        string androidProjectPatch1Zip = EXPORTED_ASSETS_PATH + "/AllAndroidPatchFiles_Version1.zip";
        string androidProjectPatch2Zip = EXPORTED_ASSETS_PATH + "/AllAndroidPatchFiles_Version2.zip";
        if (File.Exists(androidProjectPatch1Zip)) { FileUtil.DeleteFileOrDirectory(androidProjectPatch1Zip); }
        if (File.Exists(androidProjectPatch2Zip)) { FileUtil.DeleteFileOrDirectory(androidProjectPatch2Zip); }

        string patchVersion1Zip = PROJECT_DIR + "/Assets/AndroidIl2cppPatchDemo/PrebuiltPatches/AllAndroidPatchFiles_Version1.zip";
        string patchVersion2Zip = PROJECT_DIR + "/Assets/AndroidIl2cppPatchDemo/PrebuiltPatches/AllAndroidPatchFiles_Version2.zip";
        if (File.Exists(patchVersion1Zip)) { FileUtil.CopyFileOrDirectory(patchVersion1Zip, androidProjectPatch1Zip); }
        if (File.Exists(patchVersion2Zip)) { FileUtil.CopyFileOrDirectory(patchVersion2Zip, androidProjectPatch2Zip); }
        return true;
    }

    [MenuItem("AndroidBuilder/Step 2: Build Il2cpp So", false, 102)]
    public static bool BuildIl2cppSoLib()
    {
        string jdkPath = EditorPrefs.GetString("JdkPath", "");
        if (string.IsNullOrEmpty(jdkPath)) { jdkPath = DEFAULT_JDK_PATH; }
        if (string.IsNullOrEmpty(jdkPath) || !Directory.Exists(jdkPath))
        {
            Debug.LogError("jdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        //must use the jdk in Unity
        string gradlePath = jdkPath + "/../Tools/Gradle";
        string[] gradleMainJarFiles = Directory.GetFiles(gradlePath + "/lib", "gradle-launcher*.jar", SearchOption.TopDirectoryOnly);
        if (gradleMainJarFiles.Length == 0)
        {
            Debug.LogError("gradle-launcher jar file not found in " + gradlePath + "/lib");
            return false;
        }
        string gradleMainJarFile = gradleMainJarFiles[0];

        StringBuilder allCmd = new StringBuilder();
        allCmd.AppendFormat("cd \"{0}\"\n\n", ANDROID_UNITYLIBRARY_PATH);
        allCmd.AppendFormat("call \"{0}\" "
            + " -classpath \"{1}\" org.gradle.launcher.GradleMain \"-Dorg.gradle.jvmargs=-Xmx4096m\" \"BuildIl2CppTask\""
            + GRADLE_PROXY_STRING
            + " \n\n",
            jdkPath + "/bin/java.exe",
            gradleMainJarFile);

        string buildFile = ANDROID_UNITYLIBRARY_PATH + "/build_il2cpp.bat";
        File.WriteAllText(ANDROID_UNITYLIBRARY_PATH + "/build_il2cpp.bat", allCmd.ToString());

        if (!Exec(buildFile, ""))
        {
            Debug.LogError("exec failed:" + buildFile);
            return false;
        }

        string testSoPath = SO_LIB_PATH + "/arm64-v8a/libil2cpp.so";
        if (!File.Exists(testSoPath))
        {
            Debug.LogError("libil2cpp.so not found:" + testSoPath + ", exec failed:" + buildFile);
            return false;
        }
        return true;
    }

    [MenuItem("AndroidBuilder/Step 3: Patch Gradle Project", false, 102)]
    public static bool PatchAndroidProject()
    {
        //1. patch java file
        string[] javaEntranceFiles = Directory.GetFiles(JAVA_SRC_PATH, "UnityPlayerActivity.java", SearchOption.AllDirectories);
        if (javaEntranceFiles.Length != 1)
        {
            Debug.LogError("UnityPlayerActivity.java not found or more than one.");
            return false;
        }
        string javaEntranceFile = javaEntranceFiles[0];
        string allJavaText = File.ReadAllText(javaEntranceFile);
        if (allJavaText.IndexOf("noodle1983") > 0)
        {
            Debug.Log("UnityPlayerActivity.java already patched.");
            return true;
        }
        allJavaText = allJavaText.Replace("import android.view.WindowManager;",
            @"import android.view.WindowManager;
import io.github.noodle1983.Boostrap;");

        allJavaText = allJavaText.Replace("mUnityPlayer = new UnityPlayer(this, this);",
            @"Boostrap.InitNativeLibBeforeUnityPlay(getApplication().getApplicationContext().getFilesDir().getPath());
        mUnityPlayer = new UnityPlayer(this, this);");
        File.WriteAllText(javaEntranceFile, allJavaText);

        // generate removal test file
        string assetBinDataPath = EXPORTED_ASSETS_PATH + "/bin/Data/";
        string uselessFile1 = assetBinDataPath + "to_be_removed1.txt";
        File.WriteAllText(uselessFile1, "useless1");

        string uselessFile2 = assetBinDataPath + "to_be_removed2.txt";
        File.WriteAllText(uselessFile2, "useless2");

        return true;
    }


    [MenuItem("AndroidBuilder/Step 4: Generate Bin Patches", false, 103)]
    public static bool GenerateBinPatches()
    {
        string assetBinDataPath = EXPORTED_ASSETS_PATH + "/bin/Data/";
        string patchTopPath = PROJECT_DIR + "/AllAndroidPatchFiles/";
        string assertBinDataPatchPath = patchTopPath + "/assets_bin_Data/";
     
        if (Directory.Exists(patchTopPath)) { FileUtil.DeleteFileOrDirectory(patchTopPath); }
        Directory.CreateDirectory(assertBinDataPatchPath);

        string[][] soPatchFile =
        {
                // path_in_android_project, filename inside zip, zip file anme
                new string[3]{ "/"+ SO_DIR_NAME + "/armeabi-v7a/libil2cpp.so", "libil2cpp.so.new", "lib_armeabi-v7a_libil2cpp.so.zip" },            
                new string[3]{ "/"+ SO_DIR_NAME + "/arm64-v8a/libil2cpp.so", "libil2cpp.so.new", "lib_arm64-v8a_libil2cpp.so.zip" },
        };

        for (int i = 0; i < soPatchFile.Length; i++)
        {
            string[] specialPaths = soPatchFile[i];
            string projectRelativePath = specialPaths[0];
            string pathInZipFile = specialPaths[1];
            string zipFileName = specialPaths[2];

            string projectFullPath = BUILD_SCRIPTS_PATH + projectRelativePath;
            ZipHelper.ZipFile(projectFullPath, pathInZipFile, patchTopPath + zipFileName, 9);
        }

        string[] allAssetsBinDataFiles = Directory.GetFiles(assetBinDataPath, "*", SearchOption.AllDirectories);
        StringBuilder allZipCmds = new StringBuilder();
        allZipCmds.AppendFormat("if not exist \"{0}\" (MD \"{0}\") \n", PROJECT_DIR + "/AllAndroidPatchFiles/");
        allZipCmds.AppendFormat("if not exist \"{0}\" (MD \"{0}\") \n", PROJECT_DIR + "/AllAndroidPatchFiles/assets_bin_Data/");
        foreach (string apk_file in allAssetsBinDataFiles)
        {
            string relativePathHeader = "assets/bin/Data/";
            int relativePathStart = apk_file.IndexOf(relativePathHeader);
            string filenameInZip = apk_file.Substring(relativePathStart);                                                //file: assets/bin/Data/xxx/xxx
            string relativePath = apk_file.Substring(relativePathStart + relativePathHeader.Length).Replace('\\', '/'); //file: xxx/xxx
            string zipFileName = relativePath.Replace("/", "__").Replace("\\", "__") + ".bin";                                     //file: xxx__xxx.bin

            allZipCmds.AppendFormat("cd {0} && {1} -8 \"{2}\" \"{3}\"\n", BUILD_SCRIPTS_PATH, ZIP_PATH, PROJECT_DIR + "/AllAndroidPatchFiles/assets_bin_Data/" + zipFileName, filenameInZip);
        }

        string zippedPatchFile = PROJECT_DIR + "/Assets/AndroidIl2cppPatchDemo/PrebuiltPatches/AllAndroidPatchFiles_Version1.zip";
        if (File.Exists(zippedPatchFile)) { FileUtil.DeleteFileOrDirectory(zippedPatchFile);  }
        allZipCmds.AppendFormat("sleep 1 && cd {0} && {1} -9 -r \"{2}\" \"{3}\"\n", patchTopPath, ZIP_PATH, zippedPatchFile, "*");
        allZipCmds.AppendFormat("explorer.exe {0} \n\n", (PROJECT_DIR + "/Assets/AndroidIl2cppPatchDemo/PrebuiltPatches/").Replace("//", "/").Replace("/", "\\"));
        allZipCmds.AppendFormat("@echo on\n\n"); //explorer as the last line wont return success, so...

        if (allZipCmds.Length > 0)
        {
            string zipPatchesFile = ANDROID_EXPORT_PATH + "/" + "zip_patches.bat";
            File.WriteAllText(zipPatchesFile, allZipCmds.ToString());
            File.WriteAllText(zipPatchesFile, allZipCmds.ToString());
            if (!Exec(zipPatchesFile, zipPatchesFile))
            {
                Debug.LogError("exec failed:" + zipPatchesFile);
                return false;
            }
        }
        return true;
    }

    [MenuItem("AndroidBuilder/Step 5: Generate Build Scripts", false, 104)]
    public static bool GenerateBuildScripts()
    {
        string jdkPath = EditorPrefs.GetString("JdkPath", "");
        if (string.IsNullOrEmpty(jdkPath)) { jdkPath = DEFAULT_JDK_PATH; }
        if (string.IsNullOrEmpty(jdkPath) || !Directory.Exists(jdkPath))
        {
            Debug.LogError("jdk path is empty! please config via menu path:Edit/Preference->External tools.");
            return false;
        }

        //must use the jdk in Unity
        string gradlePath = jdkPath + "/../Tools/Gradle";
        string[] gradleMainJarFiles = Directory.GetFiles(gradlePath + "/lib", "gradle-launcher*.jar", SearchOption.TopDirectoryOnly);
        if (gradleMainJarFiles.Length == 0)
        {
            Debug.LogError("gradle-launcher jar file not found in " + gradlePath + "/lib");
            return false;
        }
        string gradleMainJarFile = gradleMainJarFiles[0];

        //sign
        string keystoreDir = PROJECT_DIR + "/AndroidKeystore";
        if (!Directory.Exists(keystoreDir)) { Directory.CreateDirectory(keystoreDir); }
        string keystoreFile = keystoreDir + "/test.keystore";
        if (!File.Exists(keystoreFile))
        {
            string keytoolPath = jdkPath + "/bin/keytool.exe";
            string genKeyParam = "-genkey -alias test -validity 1000 -keyalg RSA -keystore " + keystoreFile + " -dname \"CN = Test, OU = Test, O = Test, L = Test, S = Test, C = Test\" -keysize 4096 -storepass testtest -keypass testtest";
            if (!Exec(keytoolPath, genKeyParam))
            {
                Debug.LogError("exec failed:" + keytoolPath + " " + genKeyParam);
                return false;
            }
        }

        StringBuilder allCmd = new StringBuilder();
        allCmd.AppendFormat("cd \"{0}\"\n\n", ANDROID_EXPORT_PATH);
        allCmd.AppendFormat("call \"{0}\" "
            + " -classpath \"{1}\" org.gradle.launcher.GradleMain \"-Dorg.gradle.jvmargs=-Xmx4096m\" \"assembleRelease\""
            + GRADLE_PROXY_STRING
            + " -Pandroid.injected.signing.store.file=\"{2}\""
            + " -Pandroid.injected.signing.store.password=testtest "
            + " -Pandroid.injected.signing.key.alias=test "
            + " -Pandroid.injected.signing.key.password=testtest"
            + " \n\n",
            jdkPath + "/bin/java.exe",
            gradleMainJarFile,
            keystoreFile);

        allCmd.AppendFormat("copy /Y \"{0}\\launcher\\build\\outputs\\apk\\release\\launcher-release.apk\"  \"{0}\\{1}.apk\" \n\n",
            ANDROID_EXPORT_PATH.Replace("//", "/").Replace("/", "\\"),
            Application.identifier);

        allCmd.AppendFormat("explorer.exe {0} \n\n", ANDROID_EXPORT_PATH.Replace("//", "/").Replace("/", "\\"));
        allCmd.AppendFormat("@echo on\n\n"); //explorer as the last line wont return success, so...
        File.WriteAllText(ANDROID_EXPORT_PATH + "/build_apk.bat", allCmd.ToString());
        
        return true;
    }


    [MenuItem("AndroidBuilder/Step 6: Build Apk File", false, 105)]
    public static bool BuildApk()
    {
        string buildApkPath = ANDROID_EXPORT_PATH + "/build_apk.bat";
        string alignedApkName = Application.identifier + ".apk";
        string alignedApkPath = ANDROID_EXPORT_PATH + "/" + alignedApkName;

        if (!Exec(buildApkPath, ""))
        {
            Debug.LogError("exec failed:" + buildApkPath);
            return false;
        }

        if (!File.Exists(alignedApkPath))
        {
            Debug.LogError("apk not found:" + alignedApkPath + ", exec failed:" + buildApkPath);
            return false;
        }
        return true;
    }

    [MenuItem("AndroidBuilder/Run Step 1-6", false, 1)]
    public static void BuildAll()
    {
        if (!ExportGradleProject())
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        if (!BuildIl2cppSoLib())
        {
            Debug.LogError("failed to BuildIl2cppSoLig");
            return;
        }

        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        if (!GenerateBinPatches())
        {
            Debug.LogError("failed to GenerateBinPatches");
            return;
        }

        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }

        if (!BuildApk())
        {
            Debug.LogError("failed to BuildApk");
            return;
        }
        Debug.Log("Done!");
    }

    [MenuItem("AndroidBuilder/Run Step 1, 2, 3, 5, 6 for base version", false, 2)]
    public static void BuildWithoutPatch()
    {
        if (!ExportGradleProject())
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        if (!BuildIl2cppSoLib())
        {
            Debug.LogError("failed to BuildIl2cppSoLig");
            return;
        }


        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }

        if (!BuildApk())
        {
            Debug.LogError("failed to BuildApk");
            return;
        }
        Debug.Log("Done!");
    }

    [MenuItem("AndroidBuilder/Run Step 1-5 for Patch Version", false, 3)]
    public static void BuildPatch()
    {
        if (!ExportGradleProject())
        {
            Debug.LogError("failed to ExportGradleProject");
            return;
        }

        if (!BuildIl2cppSoLib())
        {
            Debug.LogError("failed to BuildIl2cppSoLig");
            return;
        }


        if (!PatchAndroidProject())
        {
            Debug.LogError("failed to PatchAndroidProject");
            return;
        }

        if (!GenerateBinPatches())
        {
            Debug.LogError("failed to GenerateBinPatches");
            return;
        }

        if (!GenerateBuildScripts())
        {
            Debug.LogError("failed to GenerateBuildScripts");
            return;
        }
        
        Debug.Log("Done!");
    }
}
