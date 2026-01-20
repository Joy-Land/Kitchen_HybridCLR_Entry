using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class HostBridgeManager
{
    private static readonly ConcurrentDictionary<string, IHostCallMiniGameHandler> _hostCallMiniGameHandlers = new ConcurrentDictionary<string, IHostCallMiniGameHandler>();
    private static readonly ConcurrentDictionary<string, IMiniGameCallHostHandler> _miniGameCallHostHandlers = new ConcurrentDictionary<string, IMiniGameCallHostHandler>();
    private static readonly ConcurrentDictionary<string, Action<BridgeResponse>> _pendingRequests = new ConcurrentDictionary<string, Action<BridgeResponse>>();

    // 存储待处理的请求队列
    private static readonly Queue<HostToMiniGameRequest> _hostToMiniGameRequestQueue = new Queue<HostToMiniGameRequest>();
    private static readonly Queue<MiniGameToHostRequest> _miniGameToHostRequestQueue = new Queue<MiniGameToHostRequest>();

    // 存储待执行的回调队列
    private static readonly Queue<Action> _responseCallbackQueue = new Queue<Action>();

    // --- 注册Handler ---

    public static void Init()
    {
      
        //HostBridgeManager.RegisterMiniGameCallHostHandler(new GetCurrentUIDHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new GetCurrentOpenIdHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new AddFriendHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new AddFriendWithoutDialogHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new IsFriendHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new ShareHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new InviteHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new GetLatestFriendsHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new RequestPayHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new GetAudioAuthStatusHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new RequestAudioNoRemindPermissionHandler());

        HostBridgeManager.RegisterMiniGameCallHostHandler(new InitAudioMgrHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new UnInitAudioMgrHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new EnterRoomHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new ExitRoomHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new SetMicStateHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new SetSpeakerStateHandler());

        HostBridgeManager.RegisterMiniGameCallHostHandler(new WPLoginHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new WPLoadFinishHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new WPRechargeHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new WPShareHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new WPAddFriendHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new InviteUserToGameHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new RequestAuthorizeHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new RequestOrderHandler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new ShareThirdPlatformH5Handler());
        HostBridgeManager.RegisterMiniGameCallHostHandler(new GetUserInfoHandler());



    }
    public static void RegisterHostCallMiniGameHandler(IHostCallMiniGameHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.Name))
        {
            Debug.Log("Invalid handler or name for HostCallMiniGame.");
            return;
        }

        _hostCallMiniGameHandlers.TryAdd(handler.Name, handler);
        Debug.Log($"Registered HostCallMiniGame handler: {handler.Name}");
    }

    public static void RegisterMiniGameCallHostHandler(IMiniGameCallHostHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.Name))
        {
            Debug.Log("Invalid handler or name for MiniGameCallHost.");
            return;
        }

        _miniGameCallHostHandlers.TryAdd(handler.Name, handler);
        Debug.Log($"Registered MiniGameCallHost handler: {handler.Name}");
    }

    public static void SendToMiniGame(string method, object data, Action<BridgeResponse> callback = null)
    {
        var request = new HostToMiniGameRequest(method, data);
        if (callback != null)
        {
            _pendingRequests.TryAdd(request.id, callback);
        }
        // 将请求放入队列，等待外部Tick处理
        lock (_hostToMiniGameRequestQueue)
        {
            _hostToMiniGameRequestQueue.Enqueue(request);
        }
    }

    public static void SendToHost(string method, object data, Action<BridgeResponse> callback = null)
    {
        var request = new MiniGameToHostRequest(method, data);
        if (callback != null)
        {
            _pendingRequests.TryAdd(request.id, callback);
        }
        // 将请求放入队列，等待外部Tick处理
        lock (_miniGameToHostRequestQueue) 
        {
            _miniGameToHostRequestQueue.Enqueue(request);
        }
    }

    public static void ProcessTick()
    {
        // 1. 处理宿主到MiniGame的请求队列
        lock (_hostToMiniGameRequestQueue)
        {
            while (_hostToMiniGameRequestQueue.Count > 0)
            {
                var request = _hostToMiniGameRequestQueue.Dequeue();
                ProcessHostToMiniGameRequest(request);
            }
        }

        // 2. 处理MiniGame到宿主的请求队列
        lock (_miniGameToHostRequestQueue)
        {
            while (_miniGameToHostRequestQueue.Count > 0)
            {
                var request = _miniGameToHostRequestQueue.Dequeue();
                ProcessMiniGameToHostRequest(request);
            }
        }

        // 3. 执行所有待处理的回调
        lock (_responseCallbackQueue)
        {
            while (_responseCallbackQueue.Count > 0)
            {
                var callback = _responseCallbackQueue.Dequeue();
                callback?.Invoke();
            }
        }
    }


    private static void ProcessMiniGameToHostRequest(MiniGameToHostRequest request)
    {
        Debug.Log($"Bridge processing MiniGame request: {request.method}");
        _miniGameCallHostHandlers.TryGetValue(request.method, out var handler);

        BridgeResponse response;
        if (handler != null)
        {
            try
            {
                handler.Handle(request,(response) =>
                {
                    // 将响应放入回调队列
                    EnqueueResponse(response);
                });
            }
            catch (Exception e)
            {
                Debug.Log($"Error in MiniGameCallHost handler '{request.method}': {e.Message}");
                response = new BridgeResponse(request.id, request.method, false, null, e.Message);
                EnqueueResponse(response);
            }
        }
        else
        {
            Debug.Log($"No handler found for MiniGame call: {request.method}");
            response = new BridgeResponse(request.id, request.method, false, null, "Handler not found");
            EnqueueResponse(response);
        }
    }

    private static void ProcessHostToMiniGameRequest(HostToMiniGameRequest request)
    {
        Debug.Log($"Bridge processing Host request: {request.method}");
        _hostCallMiniGameHandlers.TryGetValue(request.method, out var handler);

        BridgeResponse response;
        if (handler != null)
        {
            try
            {
                handler.Handle(request, (response) =>
                {
                    // 将响应放入回调队列
                    EnqueueResponse(response);
                });
            }
            catch (Exception e)
            {
                Debug.Log($"Error in HostCallMiniGame handler '{request.method}': {e.Message}");
                response = new BridgeResponse(request.id, request.method, false, null, e.Message);
                EnqueueResponse(response);
            }
        }
        else
        {
            Debug.Log($"No handler found for Host call: {request.method}");
            response = new BridgeResponse(request.id, request.method, false, null, "Handler not found");
            EnqueueResponse(response);
        }

    }

    private static void EnqueueResponse(BridgeResponse response)
    {
        Action<BridgeResponse> callback = null;
        if (_pendingRequests.TryRemove(response.id, out callback))
        {
            // 将回调执行加入主线程队列
            lock (_responseCallbackQueue)
            {
                _responseCallbackQueue.Enqueue(() => callback(response));
            }
        }
        else
        {
            Debug.Log($"No callback found for response ID: {response.id}");
        }
    }

    // --- 清理 ---
    public static void ClearPendingRequests()
    {
        _pendingRequests.Clear();
        lock (_hostToMiniGameRequestQueue)
        {
            _hostToMiniGameRequestQueue.Clear();
        }
        lock (_miniGameToHostRequestQueue)
        {
            _miniGameToHostRequestQueue.Clear();
        }
        lock (_responseCallbackQueue)
        {
            _responseCallbackQueue.Clear();
        }
    }

    public static void UnregisterHostCallMiniGameHandler(string name)
    {
        _hostCallMiniGameHandlers.TryRemove(name, out _);
        Debug.Log($"Unregistered HostCallMiniGame handler: {name}");
    }

    public static void UnregisterMiniGameCallHostHandler(string name)
    {
        _miniGameCallHostHandlers.TryRemove(name, out _);
        Debug.Log($"Unregistered MiniGameCallHost handler: {name}");
    }


    public static void ExitGame()
    {
#if NATIVE_BIND_ENABLE
        UnityCallNative.UnityCallNativeImpl.Exit();
#else
        Application.Quit();

#endif
    }

    public static int GetRoomId()
    {

#if NATIVE_BIND_ENABLE
        if (string.IsNullOrEmpty(EnvironmentManager.Instance.getSceneData))
        {
            return 0;
        }

        var data = JsonUtility.FromJson<UnityCallNative.UnityCallNativeImpl.SceneDataInfo>(EnvironmentManager.Instance.getSceneData);
        int rid = data?.room_info?.rid ?? 0;
        return rid;
#else
        return 0;
#endif
    }

    public static void setAudioCaptureVolume(int volume)
    {
        HostMiniGameManager.Instance.setAudioCaptureVolume(volume);
    }


    public static void setAudioPlayoutVolume(int volume)
    {
        HostMiniGameManager.Instance.setAudioPlayoutVolume(volume);
    }

    public static int GetClientId()
    {
       return HostMiniGameManager.Instance.GetClientId();
    }


    public static int GetGameId()
    {
        return HostMiniGameManager.Instance.GetGameId();
    }

    public static string GetLanguageName()
    {
        return HostMiniGameManager.Instance.GetLanguageName();
    }


    public static void TrackInit()
    {

        HostMiniGameManager.Instance.TrackInit();
    }

    public static void Tracking(string eventName, Dictionary<string, object> properties)
    {
        HostMiniGameManager.Instance.Tracking(eventName, properties);
    }

}
