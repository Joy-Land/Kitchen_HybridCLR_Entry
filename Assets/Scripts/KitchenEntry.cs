using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniGame.Kitchen
{
    public class KitchenEntry : MonoBehaviour
    {

        public static readonly bool isReleaseVersion = false;

        private static readonly string TestGameVersion = "1.0.0";

        private static readonly string ReleaseGameVersion = "10.0.0";

        void Awake()
        {
            Debug.Log("fzy kitchenEntry Awake");
            StartCoroutine(DownLoadAssets());
        }


        public static string GetPlatformString()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
        return "IOS";
#endif
            return "Android";
        }

        public static string DevTypeString(bool isDebug)
        {
            return isDebug ? "Debug" : "Release";
        }

        public class VersionInfo
        {
            public List<VersionDataList> androidVersionDataList = new List<VersionDataList>();
            public List<VersionDataList> iosVersionDataList = new List<VersionDataList>();
        }
        public class VersionDataList
        {
            public string version;
            public bool isDebug;
            public string firstBundleMd5;
        }

        static TextAsset configText = null;
        IEnumerator DownLoadVersionConfig()
        {
            UnityWebRequest www = UnityWebRequest.Get($"https://cdn.joylandstudios.com/Kitchen/versionInfo.json" + $"?v={UnityEngine.Random.Range(0f, 1f)}");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("fzy no version config: " + www.error);
            }
            else
            {
                //configText = ;
                configText = new TextAsset(www.downloadHandler.text);
                Debug.LogError("fzy version config:" + configText);
            }

        }

        IEnumerator DownLoadAssets()
        {

            yield return DownLoadVersionConfig();
            VersionInfo versionInfo = JsonConvert.DeserializeObject<VersionInfo>(configText.text);
            bool isDebug = false;
            string bunldePath = "minigamebundle";



            var bundleName = "minigamebundle";

            // 最终完整的文件名（包含MD5）
            string finalBundleFileName = "minigamebundle";

            string entryBundleUrl = string.Empty;
            string targetMd5 = "";

            if (isReleaseVersion == true) //3.0.1正式包
            {
                if (versionInfo != null)
                {
                    var platformStr = GetPlatformString();
                    List<VersionDataList> targetList = null;
                    if (platformStr == "Android")
                    {
                        targetList = versionInfo.androidVersionDataList;
                    }
                    else if (platformStr == "IOS")
                    {
                        targetList = versionInfo.iosVersionDataList;
                    }
                    else
                    {
                        targetList = versionInfo.androidVersionDataList;
                    }


                    foreach (var versionData in targetList)
                    {
                        if (versionData.version == ReleaseGameVersion)
                        {
                            isDebug = versionData.isDebug;
                            targetMd5 = versionData.firstBundleMd5;
                            // 拼接 MD5 后缀
                            finalBundleFileName = $"{bundleName}_{targetMd5}";
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(targetMd5))
                {
                    Debug.LogError($"fzy Version info not found for {ReleaseGameVersion}");
                    yield break;
                }

                entryBundleUrl = $"https://cdn.joylandstudios.com/Kitchen_{GetPlatformString()}/{GetPlatformString()}/{DevTypeString(isDebug)}/EntryBundle/{ReleaseGameVersion}/{finalBundleFileName}";
            }
            else
            {
                isDebug = true;
                finalBundleFileName = "minigamebundle_test";
                entryBundleUrl = $"https://cdn.joylandstudios.com/Kitchen_{GetPlatformString()}/{GetPlatformString()}/{DevTypeString(isDebug)}/EntryBundle/{TestGameVersion}/{finalBundleFileName}" + $"?v={UnityEngine.Random.Range(0f, 1f)}";
            }

            Debug.Log($"fzy kitchenEntry Target: {finalBundleFileName}, URL: {entryBundleUrl}");

            AssetBundle bundle = null;
            bool useCache = !string.IsNullOrEmpty(targetMd5);
            string localCachePath = Path.Combine(Application.persistentDataPath, "Kitchen", finalBundleFileName);

            if (useCache && File.Exists(localCachePath))
            {
                Debug.Log($"fzy Cache Hit! Loading from: {localCachePath}");
                try
                {
                    bundle = AssetBundle.LoadFromFile(localCachePath);
                    if (bundle == null)// 文件损坏，删除后重新下载.
                    {
                        Debug.LogError("fzy Failed to load from cache, maybe file corrupted. Deleting and redownloading...");
                        File.Delete(localCachePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("fzy Failed to load from cache, maybe file corrupted. Deleting and redownloading...");
                    File.Delete(localCachePath);
                }
            }

            if (bundle == null)
            {
                Debug.Log("fzy downloading from remote...");
                UnityWebRequest www = UnityWebRequest.Get(entryBundleUrl);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("fzy Download failed: " + www.error);
                    yield break;
                }
                else
                {
                    byte[] assetData = www.downloadHandler.data;
                    Debug.Log("fzy Downloaded size: " + assetData.Length);

                    // 如果需要缓存，则写入本地文件
                    if (useCache)
                    {
                        try
                        {
                            string directory = Path.GetDirectoryName(localCachePath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                                Debug.Log($"fzy Created cache directory: {directory}");
                            }

                            File.WriteAllBytes(localCachePath, assetData);
                            Debug.Log($"fzy Saved to cache: {localCachePath}");

                            // 从文件加载
                            bundle = AssetBundle.LoadFromFile(localCachePath);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"fzy Save cache failed: {ex.Message}, fallback to memory load.");
                            bundle = AssetBundle.LoadFromMemory(assetData);
                        }
                    }
                    else
                    {
                        bundle = AssetBundle.LoadFromMemory(assetData);
                    }
                }
            }

            if (bundle != null)
            {
                AssetBundleLoaderMgr.instance.SetAB(bundle, bundleName);
                try
                {
                    //加载热更依赖dll
                    Debug.LogError("fzy assetData1:" + bundle + "," + bundleName);
                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "PrimeTween.Runtime.dll").bytes);
                    Debug.LogError("fzy assetData2:");
                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "UniTask.dll").bytes);
                    Debug.LogError("fzy assetData3:");
                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "YooAsset.dll").bytes);
                    Debug.LogError("fzy assetData4:");

                    //Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "Unity.InputSystem.dll").bytes);

                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "KismetFramework.Runtime.dll").bytes);

                    Debug.LogError("fzy assetData5:");
                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "HighlightPlus.Runtime.dll").bytes);
                    Assembly.Load(AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "WooLocalization.dll").bytes);
                    Debug.LogError("fzy assetData6:");

                    //加载热更主dll
                    var tt = AssetBundleLoaderMgr.instance.LoadAsset<TextAsset>(bundleName, "JoyLandGame.dll");
                    Debug.LogError("fzy assetData7:");
                    Assembly hotfixAss = Assembly.Load(tt.bytes);
                    Debug.LogError("fzy assetData8:");

                    var minigameEntry = hotfixAss.GetType("MiniGameLauncher");
                    if (minigameEntry != null)
                    {
                        var initMethod = minigameEntry.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);

                        if (initMethod != null)
                        {
                            initMethod.Invoke(null, null);

                            Debug.LogError("fzy call init finish！");
                        }
                        else
                        {
                            Debug.LogError("fzy init not found");
                        }
                    }

                    GameObject.Destroy(this.gameObject);
                }
                catch (Exception e)
                {
                    Debug.LogError("fzy Error," + e);
                }
            }
            else
            {
                Debug.LogError("fzy No bundle");
            }

        }

    }
}

