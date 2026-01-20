
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TRTCTeamAudio
{
    public void Init() { }

    /// <summary>
    /// 销毁引擎
    /// </summary>
    public void UnInit() { }

    /// <summary>
    /// 进入语音房
    /// </summary>
    public void EnterRoom(string roomId, string streamId) { }

    /// <summary>
    /// 退出语音房
    /// </summary>
    public void ExitRoom() { }

    /// <summary>
    /// 设置麦克风状态
    /// </summary>
    /// <param name="state"></param>
    public void SetMic(bool state) { }

    /// <summary>
    /// 设置扬声器状态
    /// </summary>
    /// <param name="state"></param>
    public void SetSpeaker(bool state) { }

}

public class HttpClient
{
    private static HttpClient _instance = null;
    public static HttpClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HttpClient();
            }
            return _instance;
        }
    }
    public delegate void ProcessData(Hashtable response);
    public void RequestUid2OpenId(int[] uids, ProcessData callback, ProcessData timeoutCallback)
    {

    }

    public void RequestOpenId2uid(string[] openids, ProcessData callback, ProcessData timeoutCallback)
    {

    }

    public void RequestAuthorize(int uid, string clientId, string scope, string state, ProcessData callback, ProcessData timeoutCallback)
    {

    }
    public void RequestOrder(
    int uid,
    string clientId,
    int priceId,
    string productName,
    string tradeId,
    string extra = "",
    string callbackUrl = "",
    ProcessData callback = null,
    ProcessData timeoutCallback = null)
    {

    }
}

public class HostMiniGameManager
{
    private static HostMiniGameManager _instance = null;
    public static HostMiniGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HostMiniGameManager();
            }
            return _instance;
        }
    }

    private TRTCTeamAudio _trtcTeamAudio;

    public List<int> GetUidList(Hashtable result)
    {
        return null;
    }

    public Dictionary<int,string> GetOpenIdList(Hashtable result)
    {
        return null;
    }

    public int DecodeInviteId(int id)
    {
        return id;
    }

    public void ConvertUidDataToOpenIdObjects(string resultJson, string totalResultJson,
        Action<List<ConvertedFriendInfo>, List<ConvertedTotalFriendInfo>> onSuccess, Action<string> onError)
    {
    }
    private HostMiniGameManager()
    {
        Init();
    }

    public void Update()
    {
        HostBridgeManager.ProcessTick();
    }

    public void Init()
    {
        HostBridgeManager.Init();
    }

    private void InitializeTRTCTeamAudio()
    {
        if (_trtcTeamAudio == null)
        {
            _trtcTeamAudio = new TRTCTeamAudio(); // 创建 TRTCTeamAudio 实例
            Debug.Log("[HostMiniGameManager] TRTCTeamAudio instance created.");
        }
    }

    public TRTCTeamAudio GetTRTCTeamAudio()
    {
        if (_trtcTeamAudio == null)
        {
            Debug.LogWarning("[HostMiniGameManager] TRTCTeamAudio instance is null. Attempting to initialize...");
            InitializeTRTCTeamAudio();
        }
        return _trtcTeamAudio;
    }

    public  void TrackInit()
    {

    }

    public  void Tracking(string eventName, Dictionary<string, object> properties)
    {
    }

    public  void setAudioCaptureVolume(int volume)
    {
        
    }


    public  void setAudioPlayoutVolume(int volume)
    {
        
    }

    public string GetToekn(Hashtable result)
    {
        return "";
    }

    public  int GetClientId()
    {
        return 0;
    }

    public int GetGameId()
    {
        return 0;
    }
    public string GetLanguageName()
    {
        return "";
    }
}