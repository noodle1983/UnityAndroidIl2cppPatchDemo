using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class VersionSettor : MonoBehaviour {
    public int updateVersion = 0;
    public MessageBoxUI messageBox;

    public static string ZIP_PATCH_FORMAT { get { return Application.streamingAssetsPath + "/AllAndroidPatchFiles_Version{0}.zip"; } }
    public static string RUNTIME_PATCH_PATH_FORMAT { get { return Application.persistentDataPath + "/Version_{0}"; } }
    public static string WWW_RUNTIME_PATCH_PATH_FORMAT { get { return "file://" + RUNTIME_PATCH_PATH_FORMAT; } }

    // Use this for initialization
    void Start () {
		
	}

    public void OnClickSetVersion()
    {
        if (messageBox.gameObject.activeInHierarchy) { return; }

        if (updateVersion <= 0)
        {
            string error = Bootstrap.use_data_dir("", "");
            if (!string.IsNullOrEmpty(error))
            {
                messageBox.Show("use failed. empty path error:" + error, "ok", ()=> { messageBox.Close(); });
            }
            else
            {
                StartCoroutine(Restart());
            }
            return;
        }
        StartCoroutine(PreparePatchAndRestart());
    }

    //-----------------------------------------------------------------------------------------------

    private IEnumerator PreparePatchAndRestart()
    {
        //1. clear files if exist
        string runtimePatchPath = string.Format(RUNTIME_PATCH_PATH_FORMAT, updateVersion);
        if (Directory.Exists(runtimePatchPath)) { Directory.Delete(runtimePatchPath, true); }
        Directory.CreateDirectory(runtimePatchPath);

        //2. extract files from zip
        string zipPatchFile = string.Format(ZIP_PATCH_FORMAT, updateVersion);
        WWW zipPatchFileReader = new WWW(zipPatchFile);
        while (!zipPatchFileReader.isDone) { yield return null; }
        if (zipPatchFileReader.error != null)
        {      
            messageBox.Show("failed to get zip patch file:" + zipPatchFile, "ok", () => { messageBox.Close(); });
            yield break;
        }
        byte[] zipContent = zipPatchFileReader.bytes;
        ZipHelper.UnZipBytes(zipContent, runtimePatchPath, "", true);

        //3. prepare libil2cpp, unzip with name: libil2cpp.so.new
        string zipLibil2cppPath = runtimePatchPath + "/lib_" + Bootstrap.get_arch_abi() + "_libil2cpp.so.zip";
        if (!File.Exists(zipLibil2cppPath))
        {
            messageBox.Show("file not found:" + zipLibil2cppPath, "ok", () => { messageBox.Close(); });
            yield break;
        }
        ZipHelper.UnZip(zipLibil2cppPath, runtimePatchPath, "", true);

        //4. tell libboostrap.so to use the right patch after reboot

        //!!!! wrong streamingAssetsPath in Google aab
        //jar:file:///data/app/x.x.x/base.apk!/assets/
        //int apkPathStart = Application.streamingAssetsPath.IndexOf("/data");
        //int apkPathEnd = Application.streamingAssetsPath.IndexOf("!");
        //string apkPath = Application.streamingAssetsPath.Substring(apkPathStart,apkPathEnd - apkPathStart);
        //if (string.IsNullOrEmpty(apkPath))
        //{
        //    messageBox.Show("use failed. apk path not found in [" + Application.streamingAssetsPath + "]", "ok", () => { messageBox.Close(); });
        //    yield break;
        //}
        string apkPath = "";
        string error = Bootstrap.use_data_dir(runtimePatchPath, apkPath);
        if (!string.IsNullOrEmpty(error))
        {
            messageBox.Show("use failed. path:" + zipLibil2cppPath + ", error:" + error, "ok", () => { messageBox.Close(); });
            yield break;
        }

        //5. clear unity cache
        string cacheDir = Application.persistentDataPath + "/il2cpp";
        if (Directory.Exists(cacheDir)) {
            DeleteDirectory(cacheDir);
        }
        else
        {
            messageBox.Show("pre Unity cached file not found. path:" + cacheDir, "ok", () => { messageBox.Close(); });
            yield break;
        }

        //6. reboot app
        yield return StartCoroutine(Restart());
    }

    private IEnumerator Restart()
    {
        messageBox.Show("The patch is ready.", "Reboot", Bootstrap.reboot_app);
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    //-----------------------------------------------------------------------------------------------

    public static void DeleteDirectory(string target_dir)
    {
        try
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
        catch(System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}
