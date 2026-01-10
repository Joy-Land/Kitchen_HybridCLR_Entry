using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniGame.Kitchen
{
    public class KitchenEntry : MonoBehaviour
    {

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


        IEnumerator DownLoadAssets()
        {

            //yield return DownLoadVersionConfig();
            //List<PlatformConfig> platformConfigList = JsonConvert.DeserializeObject<List<PlatformConfig>>(configText.text);

            var bundleName = "minigamebundle";
            string entryBundlePath = string.Empty;

            //体验服
            entryBundlePath = $"https://cdn.joylandstudios.com/Kitchen_{GetPlatformString()}/{GetPlatformString()}/Debug/EntryBundle/10.0.0/minigamebundle" + $"?v={UnityEngine.Random.Range(0f, 1f)}";

            //测试服
            //entryBundlePath = $"https://cdn.joylandstudios.com/Kitchen_{GetPlatformString()}/{GetPlatformString()}/Debug/EntryBundle/1.0.0/minigamebundle" + $"?v={UnityEngine.Random.Range(0f, 1f)}";

            Debug.Log("fzy kitchenEntry send Req:"+bundleName+","+ entryBundlePath);
            UnityWebRequest www = UnityWebRequest.Get(entryBundlePath);
            yield return www.SendWebRequest();
            Debug.Log("fzy kitchenEntry send Req:" + bundleName + "," + entryBundlePath);
            //*******************测试代码******************* ↓↓↓
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("fzy dll " + www.error);
            }
            else
            {
                byte[] assetData = www.downloadHandler.data;
                Debug.Log("fzy kitchenEntry rec Req:" + assetData.Length);
                Debug.LogError("fzy assetData:" + assetData);
                var bundle = AssetBundle.LoadFromMemory(assetData);
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
        }

    }
}

