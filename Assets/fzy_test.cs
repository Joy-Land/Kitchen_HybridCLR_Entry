using HybridCLR;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;


public class fzy_test : MonoBehaviour
{

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        StartCoroutine(DownLoadAssets());
    }

    private string GetWebRequestPath(string asset)
    {
        var path = $"{Application.streamingAssetsPath}/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        return path;
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

        entryBundlePath = $"https://cdn.joylandstudios.com/Kitchen_{GetPlatformString()}/{GetPlatformString()}/Debug/EntryBundle/1.0.0/minigamebundle" + $"?v={UnityEngine.Random.Range(0f, 1f)}";
        UnityWebRequest www = UnityWebRequest.Get(entryBundlePath);
        yield return www.SendWebRequest();

        //*******************测试代码******************* ↓↓↓
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("fzy dll " + www.error);
        }
        else
        {
            byte[] assetData = www.downloadHandler.data;

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

                var minigameEntry = hotfixAss.GetType("MiniGameHandlerManager");
                if(minigameEntry != null )
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

            }
            catch (Exception e)
            {
                Debug.LogError("fzy Error," + e);
            }
        }
    }

    float counter = 0;
    // Update is called once per frame
    void Update()
    {
        return;
        //if (counter > 1)
        //{
        //    //console.error("fzy ddd:", Camera.main.worldToCameraMatrix);
        //    //SetTransformParam(Camera.main.transform.position, Vector3.zero, Camera.main.transform.eulerAngles);
        //    GetViewMatrix_TRS_LookRotation(Vector3.zero, Camera.main.transform.forward);
        //    //Matrix4x4 view = GetViewMatrix_AxisMulTrans(Camera.main.transform.position, Camera.main.transform.forward);
        //    //console.error("fzy zzzz", view);
        //    counter = 0;
        //}
        //counter += Time.deltaTime;
    }

    //public void SetTransformParam(Vector3 pos, Vector3 lookat, Vector3 _eulerAngle)
    //{
    //    var position = pos;
    //    var forward = Camera.main.transform.forward;

    //    var right = Vector3.Cross(forward, Vector3.up).normalized;
    //    var up = Vector3.Cross(right, forward).normalized;

    //    Matrix4x4 viewMatrix = new Matrix4x4();
    //    viewMatrix.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
    //    viewMatrix.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
    //    viewMatrix.SetColumn(2, new Vector4(-forward.x, -forward.y, -forward.z, 0));
    //    viewMatrix.SetColumn(3, new Vector4(-Vector3.Dot(position, right), -Vector3.Dot(position, up), Vector3.Dot(position, forward), 1));
    //    console.error("fzy zzzz:", viewMatrix);

    //}

    //Matrix4x4 GetViewMatrix_TRS_LookRotation(Vector3 lookAtPos, Vector3 camForward)
    //{
    //    Vector3 lightPos = Camera.main.transform.position;
    //    Quaternion rot = Camera.main.transform.rotation;
    //    var mm = Matrix4x4.Inverse(Matrix4x4.TRS(lightPos, rot, new Vector3(1, 1, -1))); ;
    //    //console.error("fzy zzzzx:", mm, rot*Vector3.forward, Camera.main.transform.forward);
    //    return mm;
    //}

    //// view矩阵, basicAxis * translate 方式 //
    //Matrix4x4 GetViewMatrix_AxisMulTrans(Vector3 lookAtPos, Vector3 camForward)
    //{
    //    Vector3 forward = Vector3.Normalize(-camForward);
    //    Vector3 up = new Vector3(0, 1, 0);
    //    Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
    //    up = Vector3.Normalize(Vector3.Cross(forward, right));

    //    // translate //
    //    Vector3 lightPos = lookAtPos;
    //    Matrix4x4 translate = Matrix4x4.identity;
    //    // 也可以用 Matrix4x4.Translate(-translatePos); 构建平移矩阵 //
    //    translate.SetColumn(3, new Vector4(-lightPos.x, -lightPos.y, -lightPos.z, 1));
    //    //translate.SetColumn(3, -lightPos);

    //    // basicAxis //
    //    Matrix4x4 basicAxis = Matrix4x4.identity;
    //    basicAxis.SetRow(0, new Vector4(right.x, right.y, right.z, 0));
    //    basicAxis.SetRow(1, new Vector4(up.x, up.y, up.z, 0));
    //    basicAxis.SetRow(2, new Vector4(forward.x, forward.y, forward.z, 0));

    //    // 先平移，再计算投影在基向量的长度，即新空间的坐标 //
    //    return basicAxis * translate;
    //}


}
