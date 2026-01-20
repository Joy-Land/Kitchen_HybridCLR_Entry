using System;
#if NATIVE_BIND_ENABLE
using UnityCallNative;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
// 获取当前用户ID处理器
/*public class GetCurrentUIDHandler : IMiniGameCallHostHandler
{
    public string Name => "getCurrentUID";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            int currentUid = GetCurrentUserUID();
            string currentSid = GetCurrentUserSID();

#if NATIVE_BIND_ENABLE
            UnityCallNativeImpl.GetCurrentUID((uid, sid) =>
            {
                Debug.Log($"getcurrent uid :{uid}, {sid}");
                var response = new GetCurrentUIDResponse
                {
                    uid = uid,
                    sid = sid
                };
                var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                callback(bridgeResponse);
            }, (code, msg) =>
            {
                Debug.Log("获取UID失败");
                var response = new GetCurrentUIDResponse
                {
                    uid = currentUid, // 使用默认值
                    sid = currentSid
                };
                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, msg);
                callback(bridgeResponse);
            });
#else
            // 非原生模式，直接返回
            var response = new GetCurrentUIDResponse
            {
                uid = currentUid,
                sid = currentSid
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in getCurrentUID: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }

    private int GetCurrentUserUID()
    {
        return 123456789;
    }

    private string GetCurrentUserSID()
    {
        return "session_abc123";
    }
}*/


public class GetCurrentOpenIdHandler : IMiniGameCallHostHandler
{
    public string Name => "getCurrentOpenId";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            int currentUid = GetCurrentUserUID();
            string currentSid = GetCurrentUserSID();

#if NATIVE_BIND_ENABLE 
            UnityCallNativeImpl.GetCurrentUID((uid, sid) =>
            {

                // 将 UID 转换为 OpenID
                HttpClient.Instance.RequestUid2OpenId(new int[] { uid },
                    // 成功回调
                    (result) =>
                    {
                        try
                        {

                            var openIds = HostMiniGameManager.Instance.GetOpenIdList(result);
                            var response = new GetOpenIdResponse
                            {

                                sid = sid,

                            };

                            if (openIds.ContainsKey(uid))
                            {
                                response.openId = openIds[uid];
                            }

                            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                            callback(bridgeResponse);
                        }
                        catch (Exception parseEx)
                        {
                            Debug.LogError($"Parse OpenID error: {parseEx.Message}");
                            var response = new GetOpenIdResponse
                            {
                                openId = string.Empty,
                                sid = sid
                            };
                            var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                            callback(bridgeResponse);
                        }
                    },
                    // 超时回调
                    (timeoutResult) =>
                    {
                        Debug.Log("get OpenID timeout");
                        var response = new GetOpenIdResponse
                        {
                            openId = string.Empty,
                            sid = sid
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "OpenID timeout");
                        callback(bridgeResponse);
                    }
                );
            },
            // 获取 UID 失败的回调
            (code, msg) =>
            {
                var response = new GetOpenIdResponse
                {
                    openId = string.Empty,
                    sid = currentSid
                };
                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, msg);
                callback(bridgeResponse);
            });
#else
            // 非原生模式，直接进行 UID 转 OpenID
            HttpClient.Instance.RequestUid2OpenId(new int[] { currentUid }, 
                // 成功回调
                (result) =>
                {
                    try
                    {
                        var openIds = HostMiniGameManager.Instance.GetOpenIdList(result);
                        var response = new GetOpenIdResponse
                        {

                            sid = currentSid,
                          
                        };

                        if(openIds.ContainsKey(currentUid))
                        {
                            response.openId = openIds[currentUid];
                        }
                       
                        var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                        callback(bridgeResponse);
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse OpenID error: {parseEx.Message}");
                        var response = new GetOpenIdResponse
                        {
                            openId = string.Empty,
                            sid = currentSid
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                        callback(bridgeResponse);
                    }
                },
                // 超时回调
                (timeoutResult) =>
                {
                    var response = new GetOpenIdResponse
                    {
                        openId = string.Empty,
                        sid = currentSid
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "OpenID timeout");
                    callback(bridgeResponse);
                }
            );
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in getCurrentOpenId: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }

    private int GetCurrentUserUID()
    {
        return 75228677;
    }

    private string GetCurrentUserSID()
    {
        return "session_abc123";
    }
}


// 加好友处理器
public class AddFriendHandler : IMiniGameCallHostHandler
{
    public string Name => "addFriend";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (AddFriendRequest)(request.data);
            Debug.Log($"Host processing addFriend - openId: {requestData.openId}, GameType: {requestData.game_type}");

            if (string.IsNullOrEmpty(requestData.openId))
            {
                Debug.LogError("AddFriendHandler: openId is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "openId is required");
                callback(response);
                return;
            }
            string[] openIdArray = { requestData.openId };
            HttpClient.Instance.RequestOpenId2uid(openIdArray,
                (result) =>
                {
                    try
                    {
                        // 解析返回的 uid 列表
                        var uids = HostMiniGameManager.Instance.GetUidList(result);
                        if (uids == null || uids.Count == 0)
                        {
                            Debug.LogError("RequestOpenId2uid returned no UIDs.");
                            var errorResponse = new BridgeResponse(request.id, request.method, false, new { success = false }, "Failed to resolve UID from OpenID");
                            callback(errorResponse);
                            return;
                        }

                        int targetUid = uids[0];
                        //Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.openId}");

                        bool nativeResult = true;
#if NATIVE_BIND_ENABLE
                        UnityCallNativeImpl.AddFriend(targetUid, requestData.game_type, requestData.sub_source, () =>
                        {
                            Debug.Log($"addfriend success for UID: {targetUid}");
                            var response = new BridgeResponse(request.id, request.method, true, new { success = true });
                            callback(response);
                        }, (code, msg) =>
                        {
                            nativeResult = false;
                            var response = new BridgeResponse(request.id, request.method, false, new { success = false }, msg);
                            callback(response);
                            Debug.Log($"addfriend failed for UID: {targetUid}, Code: {code}, Msg: {msg}");
                        });
#else
                        // 非原生模式，模拟成功
                        var response = new BridgeResponse(request.id, request.method, true, new { success = true });
                        callback(response);
#endif

                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse UID error: {parseEx.Message}");
                        var response = new BridgeResponse(request.id, request.method, false, new { success = false }, parseEx.Message);
                        callback(response);
                    }
                },
                // 超时回调
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOpenId2uid timeout for openId: " + requestData.openId);
                    var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "OpenID to UID conversion timeout");
                    callback(response);
                }
            );

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in addFriend: {ex.Message}");
            var response = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(response);
        }
    }
}

// 加好友（无对话框）处理器
public class AddFriendWithoutDialogHandler : IMiniGameCallHostHandler
{
    public string Name => "addFriendWithoutDialog";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (AddFriendWithoutDialogRequest)(request.data);
            Debug.Log($"Host processing addFriendWithoutDialog - openId: {requestData.openId}, Flowers: {requestData.target_flower}");

            if (string.IsNullOrEmpty(requestData.openId))
            {
                Debug.LogError("AddFriendWithoutDialogHandler: openId is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "openId is required");
                callback(response);
                return;
            }

            string[] openIdArray = { requestData.openId };

            HttpClient.Instance.RequestOpenId2uid(openIdArray,

                (result) =>
                {
                    try
                    {
                            // 解析返回的 uid 列表
                            var uids = HostMiniGameManager.Instance.GetUidList(result);
                        if (uids == null || uids.Count == 0)
                        {
                            Debug.LogError("RequestOpenId2uid returned no UIDs.");
                            var errorResponse = new BridgeResponse(request.id, request.method, false, new { success = false }, "Failed to resolve UID from OpenID");
                            callback(errorResponse);
                            return;
                        }

                        int targetUid = uids[0];
                        Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.openId}");

                        bool nativeResult = true;
#if NATIVE_BIND_ENABLE
                            UnityCallNativeImpl.addFriendWithoutDialog(targetUid, requestData.target_flower, requestData.content, () =>
                            {
                                Debug.Log($"addfriend without dialog success for UID: {targetUid}");
                                var response = new BridgeResponse(request.id, request.method, true, new { success = true });
                                callback(response);
                            }, (code, msg) =>
                            {
                                nativeResult = false;
                                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, msg);
                                callback(response);
                                Debug.Log($"addfriend without dialog failed for UID: {targetUid}, Code: {code}, Msg: {msg}");
                            });
#else
                        var response = new BridgeResponse(request.id, request.method, true, new { success = true });
                        callback(response);
#endif

                        }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse UID error: {parseEx.Message}");
                        var response = new BridgeResponse(request.id, request.method, false, new { success = false }, parseEx.Message);
                        callback(response);
                    }
                },
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOpenId2uid timeout for openId: " + requestData.openId);
                    var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "OpenID to UID conversion timeout");
                    callback(response);
                }
            );

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in addFriendWithoutDialog: {ex.Message}");
            var response = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(response);
        }
    }
}

// 检查是否是好友处理器
public class IsFriendHandler : IMiniGameCallHostHandler
{
    public string Name => "isFriend";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (IsFriendRequest)(request.data);
            Debug.Log($"Host checking friendship for openId: {requestData.openId}");

            if (string.IsNullOrEmpty(requestData.openId))
            {
                Debug.LogError("IsFriendHandler: openId is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "openId is required");
                callback(response);
                return;
            }

            string[] openIdArray = { requestData.openId };

            HttpClient.Instance.RequestOpenId2uid(openIdArray,
                (result) =>
                {
                    try
                    {
                        var uids = HostMiniGameManager.Instance.GetUidList(result);
                        if (uids == null || uids.Count == 0)
                        {
                            Debug.LogError("RequestOpenId2uid returned no UIDs.");
                            var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "Failed to resolve UID from OpenID");
                            callback(response);
                            return;
                        }

                        int targetUid = uids[0];
                        Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.openId}");

                        bool isFriend = CheckIsFriend(targetUid);
                        var isFriendResponse = new IsFriendResponse
                        {
                            is_friend = isFriend
                        };

                        var bridgeResponse = new BridgeResponse(request.id, request.method, true, isFriendResponse);
                        callback(bridgeResponse);
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse UID error: {parseEx.Message}");
                        var response = new BridgeResponse(request.id, request.method, false, new { success = false }, parseEx.Message);
                        callback(response);
                    }
                },
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOpenId2uid timeout for openId: " + requestData.openId);
                    var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "OpenID to UID conversion timeout");
                    callback(response);
                }
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in isFriend: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }

    private bool CheckIsFriend(int uid)
    {
        return true; // 示例返回
    }
}

// 获取用户个人信息处理器
public class GetUserInfoHandler : IMiniGameCallHostHandler
{
    public string Name => "getUserInfo";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (GetUserInfoRequest)(request.data);
            Debug.Log($"Host processing getUserInfo - openId: {requestData.openId}");

            if (string.IsNullOrEmpty(requestData.openId))
            {
                Debug.LogError("GetUserInfoHandler: openId is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "openId is required");
                callback(response);
                return;
            }

            string[] openIdArray = { requestData.openId };

            HttpClient.Instance.RequestOpenId2uid(openIdArray,
                (result) =>
                {
                    try
                    {
                        var uids = HostMiniGameManager.Instance.GetUidList(result);
                        if (uids == null || uids.Count == 0)
                        {
                            Debug.LogError("RequestOpenId2uid returned no UIDs for getUserInfo.");
                            var errorResponse = new BridgeResponse(request.id, request.method, false, null, "Failed to resolve UID from OpenID");
                            callback(errorResponse);
                            return;
                        }

                        int targetUid = uids[0];
                        Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.openId}");

#if NATIVE_BIND_ENABLE 
                        UnityCallNativeImpl.GetUserInfo(targetUid,
                            (userInfoJson) =>
                            {
                                try
                                {
                                    var userInfoRes = new GetUserInfoResponse
                                    {
                                        openId = requestData.openId,
                                        gender = (int)userInfoJson.gender,
                                        nickname = userInfoJson.nickname,
                                        headimgurl = userInfoJson.headimgurl

                                    };
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, userInfoRes);
                                    callback(bridgeResponse);
                                }
                                catch (Exception parseEx)
                                {
                                    Debug.LogError($"Parse userInfo error: {parseEx.Message}");
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, userInfoJson, "Failed to parse userInfo");
                                    callback(bridgeResponse);
                                }
                            },
                            (code, msg) =>
                            {
                                Debug.LogError($"getUserInfo native call failed - Code: {code}, Msg: {msg}");
                                var errorResponse = new BridgeResponse(request.id, request.method, false, null, msg);
                                callback(errorResponse);
                            });
#else
                        // 非原生模式，返回一个模拟的用户信息
                        var mockUserInfo = new GetUserInfoResponse
                        {
                            headimgurl = "https://example.com/avatar.png",
                            nickname = "MockUser",
                            gender = 0,
                        };
                        var mockBridgeResponse = new BridgeResponse(request.id, request.method, true, mockUserInfo);
                        callback(mockBridgeResponse);
#endif
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse UID error in getUserInfo: {parseEx.Message}");
                        var response = new BridgeResponse(request.id, request.method, false, null, parseEx.Message);
                        callback(response);
                    }
                },
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOpenId2uid timeout for getUserInfo, openId: " + requestData.openId);
                    var response = new BridgeResponse(request.id, request.method, false, null, "OpenID to UID conversion timeout");
                    callback(response);
                }
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in getUserInfo: {ex.Message}");
            var response = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(response);
        }
    }
}

// 分享处理器
public class ShareHandler : IMiniGameCallHostHandler
{
    public string Name => "share";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (ShareRequest)(request.data);

            Debug.Log($"Host processing share - Title: {requestData.title}, Type: {requestData.type}");

            bool result = true;
#if NATIVE_BIND_ENABLE
            var data = new UnityCallNativeImpl.ShareData
            {
                game_type = requestData.game_type,
                title = requestData.title,
                type = (UnityCallNativeImpl.ShareData.ShareType)requestData.type,
                desc = requestData.desc,
                link = requestData.link,
                icon_url = requestData.icon_url,
                image = requestData.image
            };

            UnityCallNativeImpl.Share(data, () =>
            {
                Debug.Log($"share success");
                var response = new BridgeResponse(request.id, request.method, result, new { success = result });
                callback(response);
            }, (code, msg) =>
            {
                result = false;
                var response = new BridgeResponse(request.id, request.method, result, new { success = result });
                callback(response);
                Debug.Log($"share failed - Code: {code}, Msg: {msg}");
            });
#else
            var response = new BridgeResponse(request.id, request.method, result, new { success = result });
            callback(response);
#endif
            Debug.Log($"Calling native share - Title: {requestData.title}, ShareType: {requestData.type}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in share: {ex.Message}");
            var response = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(response);
        }
    }
}

// 邀请处理器
public class InviteHandler : IMiniGameCallHostHandler
{
    public string Name => "invite";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (InviteRequest)(request.data);

            Debug.Log($"Host processing invite - GameType: {requestData.game_type}, GID: {requestData.gid}");

            bool result = true;
#if NATIVE_BIND_ENABLE
            UnityCallNativeImpl.Invite(requestData.game_type, requestData.gid, () =>
            {
                Debug.Log($"invite success");
                var response = new BridgeResponse(request.id, request.method, result, new { success = result });
                callback(response);
            }, (code, msg) =>
            {
                result = false;
                var response = new BridgeResponse(request.id, request.method, result, new { success = result });
                callback(response);
                Debug.Log("invite failed");
            });
#else
            var response = new BridgeResponse(request.id, request.method, result, new { success = result });
            callback(response);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in invite: {ex.Message}");
            var response = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(response);
        }
    }
}


// 获取最近联系人列表处理器
public class GetLatestFriendsHandler : IMiniGameCallHostHandler
{
    public string Name => "getLatestFriends";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (GetLatestFriendsRequest)(request.data);
            Debug.Log($"Host processing getLatestFriends - openId: {requestData.openId}, GameType: {requestData.game_type}, Count: {requestData.count}");

            if (string.IsNullOrEmpty(requestData.openId))
            {
                Debug.LogError("GetLatestFriendsHandler: openId is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "openId is required");
                callback(response);
                return;
            }

            string[] openIdArray = { requestData.openId };

            HttpClient.Instance.RequestOpenId2uid(openIdArray,
                (result) =>
                {
                    try
                    {
                        var uids = HostMiniGameManager.Instance.GetUidList(result);
                        if (uids == null || uids.Count == 0)
                        {
                            Debug.LogError("RequestOpenId2uid returned no UIDs.");
                            var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "Failed to resolve UID from OpenID");
                            callback(response);
                            return;
                        }

                        int targetUid = uids[0];
                        Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.openId}");

#if NATIVE_BIND_ENABLE
                            UnityCallNativeImpl.GetLatestFriends(targetUid, requestData.game_type, requestData.count,
                                (resultJson, totalResultJson) =>
                                {
                                    Debug.Log("GetLatestFriends native call success");
                                        // --- 关键修改：将返回的 UID 数据转换为 OpenID 对象列表 ---
                                        HostMiniGameManager.Instance.ConvertUidDataToOpenIdObjects(resultJson, totalResultJson,
                                        (convertedResultList, convertedTotalResultList) =>
                                        {
                                            var response = new GetLatestFriendsResponse
                                            {
                                                result = convertedResultList,
                                                total_result = convertedTotalResultList
                                            };
                                            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                            callback(bridgeResponse);
                                        },
                                        (error) =>
                                        {
                                            Debug.LogError($"UID to OpenID conversion failed: {error}");
                                            var errorResponse = new BridgeResponse(request.id, request.method, false, new { success = false }, error);
                                            callback(errorResponse);
                                        }
                                    );
                                },
                                (code, msg) =>
                                {
                                    Debug.Log($"GetLatestFriends native call failed - Code: {code}, Msg: {msg}");
                                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, msg);
                                    callback(errorResponse);
                                });
#else
                        // Mock 逻辑也需要转换 UID 为 OpenID
                        var mockResultJson = "[{\"uid\": 75304977, \"remark_name\": \"好友1\", \"nickname\": \"Nick1\", \"avatar\": \"avatar_url_1\", \"gender\": 1}]";
                        var mockTotalResultJson = "[{\"is_group\": false, \"uid\": 75304977, \"group_id\": 0, \"remark_name\": \"好友1\", \"nickname\": \"Nick1\", \"avatar\": \"avatar_url_1\", \"gender\": 1, \"group_uids\":[75228677,75228678,75228680,75228681,75228682,75228683,75228684,75228686]}]";
                        string json = "{\"code\":200,\"msg\":\"{\\\"result\\\":\\\"[]\\\",\\\"total_result\\\":\\\"[{\\\\\\\"is_group\\\\\\\":true,\\\\\\\"uid\\\\\\\":0,\\\\\\\"group_id\\\\\\\":28,\\\\\\\"remark_name\\\\\\\":\\\\\\\"殿下、清欢、七月半半半\\\\\\\",\\\\\\\"nickname\\\\\\\":\\\\\\\"殿下、清欢、七月半半半\\\\\\\",\\\\\\\"avatar\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"gender\\\\\\\":0,\\\\\\\"group_uids\\\\\\\":\\\\\\\"[75224472,75224320,75224434,4694,4707,4734,4097909,4853,2117,75228767,75228768,75228769,75228770,75228772,75228775,75228778,75228779,75228780,75228781,75228782,75228783,75228784,75228785,75228786,75228787,75228788,75228789,75228790,75228791,75228792,75228793,75228794,75228795,75228796,75228797,75228799,75228800,75228801,75228802,75228803,75228804,75228805,75228806,75228807,75228808,75228809,75228811,75228812,75228814,75228815,75228816,75228817,75228819,75228820,75228821,75228822,75228824,75228825,75228826,75228827,75228828,75228829,75228830,75228831,75228832,75228833,75228834,75228835,75228836,75228837,75228840,75228842,75228843,75228844,75228845,75228846,75228847,75228852,75228854,75228855,75228856,75228858,75228860,75228861,75228862,75228863,75228864,75228865,75228866,75228868,75228869,75228872,75228874,75228875,75228876,75228887,75228889,75228890,75228891,75228892,75228893,75228894,75228895,75228896,75228897,75228899,75228900,75228901,75228902,75228903,75228904,75228905,75228906,75228907,75228908,75228910,75228911,75228912,75228913,75228914,75228915,75228918,75228919,75228920,75228921,75228922,75228924,75228925,75228926,75228927,75228929,75228930,75228931,75228932,75228935,75228937,75228938,75228939,75228940,75228942,75228943,75228944,75228947,75228948,75228949,75228950,75228951,75228953,75228954,75228956,75228957,75228958,75228959,75228961,75228962,75228964,75228965,75228966,75228967,75228968,75228969,75228971,75228972,75228973,75228975,75228976,75228977,75228978,75228979,75228980,75228981,75228982,75228983,75228984,75228985,75228986,75228988,75228989,75228990,75228991,75228992,75228994,75228996,75228997,75228998,75229001,75229002,75229005,75229007,75229012,4743,75300574,1206]\\\\\\\"}]\\\"}\"}";
                    //    json = "{\"code\":200,\"msg\":\"{\\\"result\\\":\\\"[{\\\\\\\"uid\\\\\\\":75228697,\\\\\\\"remark_name\\\\\\\":\\\\\\\"余生\\\\\\\",\\\\\\\"nickname\\\\\\\":\\\\\\\"余生\\\\\\\",\\\\\\\"avatar\\\\\\\":\\\\\\\"https:\\\\\\\\\\\\/\\\\\\\\\\\\/pic.weplayapp.com\\\\\\\\\\\\/server\\\\\\\\\\\\/avatar\\\\\\\\\\\\/o_1d8iju3n31stsovl2ufqcspjn2n.jpg\\\\\\\",\\\\\\\"gender\\\\\\\":0}]\\\",\\\"total_result\\\":\\\"[{\\\\\\\"is_group\\\\\\\":false,\\\\\\\"uid\\\\\\\":75228697,\\\\\\\"group_id\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"remark_name\\\\\\\":\\\\\\\"余生\\\\\\\",\\\\\\\"nickname\\\\\\\":\\\\\\\"余生\\\\\\\",\\\\\\\"avatar\\\\\\\":\\\\\\\"https:\\\\\\\\\\\\/\\\\\\\\\\\\/pic.weplayapp.com\\\\\\\\\\\\/server\\\\\\\\\\\\/avatar\\\\\\\\\\\\/o_1d8iju3n31stsovl2ufqcspjn2n.jpg\\\\\\\",\\\\\\\"gender\\\\\\\":0}]\\\"}\"}";
                     //   var resultRes = JsonConvert.DeserializeObject<NativeBridgeResponse>(json);
                     //   var latestFriendsRsp = JsonConvert.DeserializeObject<UnityCallNative.UnityCallNativeImpl.GetLatestFriendsRsp>(resultRes.msg);
                      //  Debug.Log(latestFriendsRsp.total_result);

                        HostMiniGameManager.Instance.ConvertUidDataToOpenIdObjects(mockResultJson, mockTotalResultJson,
                            (convertedResultList, convertedTotalResultList) =>
                            {
                               
                                var response = new GetLatestFriendsResponse
                                {
                                    result = convertedResultList,
                                    total_result = convertedTotalResultList
                                };
                                var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                callback(bridgeResponse);
                            },
                            (error) =>
                            {
                                Debug.LogError($"Mock UID to OpenID conversion failed: {error}");
                                var errorResponse = new BridgeResponse(request.id, request.method, false, new { success = false }, error);
                                callback(errorResponse);
                            }
                        );
#endif
                        }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse UID error: {parseEx.Message}");
                        var response = new BridgeResponse(request.id, request.method, false, new { success = false }, parseEx.Message);
                        callback(response);
                    }
                },
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOpenId2uid timeout for openId: " + requestData.openId);
                    var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "OpenID to UID conversion timeout");
                    callback(response);
                }
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in getLatestFriends: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}



public class ShareThirdPlatformH5Handler : IMiniGameCallHostHandler
{
    public string Name => "shareThirdPlatformH5"; // 注意：图片中是 shareThirdPlatformH5（无大写 H）

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (ShareThirdPlatformH5Req)(request.data);
            Debug.Log($"Host processing shareThirdPlatformH5 - Platform: {requestData.platform_type}, Media: {requestData.media_type}, Title: {requestData.title}, Url: {requestData.url}");

#if NATIVE_BIND_ENABLE
            // 原生模式：调用 UnityCallNativeImpl.shareThirdPlatformH5（假设接口已更新）
            UnityCallNativeImpl.shareThirdPlatformH5(
                requestData.platform_type,
                requestData.media_type,
                requestData.title,
                requestData.desc,
                requestData.url,
                requestData.image_url,
                requestData.game_type,
                requestData.rid,
                requestData.topic, // 传入 JSON 字符串
                (code, successMsg) =>
                {
                    Debug.Log($"[Native] shareThirdPlatformH5 success: {successMsg}");
                    var response = new ShareThirdPlatformH5Rsp
                    {
                        success = true,
                        message = successMsg ?? "Share success"
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                    callback(bridgeResponse);
                },
                (code, errorMsg) =>
                {
                    Debug.LogError($"[Native] shareThirdPlatformH5 failed: Code={code}, Msg={errorMsg}");
                    var response = new ShareThirdPlatformH5Rsp
                    {
                        success = false,
                        message = errorMsg ?? "Share failed"
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, errorMsg);
                    callback(bridgeResponse);
                }
            );
#else
            // 非原生模式：模拟分享成功
            Debug.Log($"[NATIVE_BIND_DISABLE] Mocking shareThirdPlatformH5 - Platform: {requestData.platform_type}, Title: {requestData.title}, Url: {requestData.url}");
            var mockSuccess = true;
            var mockMessage = mockSuccess ? "Mock share success" : "Mock share failed";

            var response = new ShareThirdPlatformH5Rsp
            {
                success = mockSuccess,
                message = mockMessage
            };

            var bridgeResponse = new BridgeResponse(request.id, request.method, mockSuccess, response, mockSuccess ? null : "Mock failure");
            callback(bridgeResponse);
#endif

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in shareThirdPlatformH5: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}



// 请求支付处理器
public class RequestPayHandler : IMiniGameCallHostHandler
{
    public string Name => "requestPay";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (RequestPayRequest)(request.data);

            Debug.Log($"Host processing requestPay - Portrait: {requestData.portrait}");

#if NATIVE_BIND_ENABLE
            UnityCallNativeImpl.RequestPay(() =>
            {

            }, (payResult, payMsg) =>
             {
                 Debug.Log($"Pay request completed - Result: {payResult}, Message: {payMsg}");
                 var response = new RequestPayResponse { message = payMsg };
                 var bridgeResponse = new BridgeResponse(request.id, request.method, true, response, payMsg);
                 callback(bridgeResponse);
             });
#else
            // 非原生模式，模拟支付流程
            Debug.Log($"[Simulated Pay] Portrait mode: {requestData.portrait}");
            var response = new BridgeResponse(request.id, request.method, true, new RequestPayResponse { message = "success" });
            callback(response);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in requestPay: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

public class GetAudioAuthStatusHandler : IMiniGameCallHostHandler
{
    public string Name => "getAudioAuthStatus";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            bool defaultAuthStatus = false; // 设置一个默认状态

#if NATIVE_BIND_ENABLE
            UnityCallNativeImpl.GetAudioAuthStatus((authStatus) =>
            {
                Debug.Log($"GetAudioAuthStatus success: {authStatus}");
                var response = new GetAudioAuthStatusResponse
                {
                    authStatus = authStatus
                };
                var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                callback(bridgeResponse);
            }, (code, msg) =>
            {
                Debug.LogError($"GetAudioAuthStatus failed: {code}, {msg}");
                var response = new GetAudioAuthStatusResponse
                {
                    authStatus = defaultAuthStatus // 使用默认值或错误状态
                    };
                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, msg);
                callback(bridgeResponse);
            });
#else
            // 非原生模式，直接返回默认状态或模拟状态
            Debug.Log($"[NATIVE_BIND_DISABLE] GetAudioAuthStatus returning default status: {defaultAuthStatus}");
            var response = new GetAudioAuthStatusResponse
            {
                authStatus = defaultAuthStatus
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response); // 假设非原生模式下操作成功
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in GetAudioAuthStatus: {ex.Message}");
            var errorResponse = new GetAudioAuthStatusResponse { authStatus = false }; // 或其他错误标识
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}

public class RequestAudioNoRemindPermissionHandler : IMiniGameCallHostHandler
{
    public string Name => "requestAudioNoRemindPermission";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            bool defaultGranted = false; // 设置一个默认结果

#if NATIVE_BIND_ENABLE
            UnityCallNativeImpl.RequestAudioNoRemindPermission((granted) =>
            {
                Debug.Log($"RequestAudioNoRemindPermission result: {granted}");
                var response = new RequestAudioNoRemindPermissionResponse
                {
                    granted = granted
                };
                var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                callback(bridgeResponse);
            }, (code, msg) =>
            {
                Debug.LogError($"RequestAudioNoRemindPermission failed: {code}, {msg}");
                var response = new RequestAudioNoRemindPermissionResponse
                {
                    granted = defaultGranted // 使用默认值
                    };
                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, msg);
                callback(bridgeResponse);
            });
#else
            // 非原生模式，直接返回默认结果或模拟结果
            Debug.Log($"[NATIVE_BIND_DISABLE] RequestAudioNoRemindPermission returning default result: {defaultGranted}");
            var response = new RequestAudioNoRemindPermissionResponse
            {
                granted = defaultGranted
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response); // 假设非原生模式下操作成功
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in RequestAudioNoRemindPermission: {ex.Message}");
            var errorResponse = new RequestAudioNoRemindPermissionResponse { granted = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}


public class InitAudioMgrHandler : IMiniGameCallHostHandler
{
    public string Name => "initAudioMgr";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();

            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            // --- 调用 C# 侧的 TRTCTeamAudio 初始化逻辑 ---
            audioMgr.Init();

            var response = new InitAudioMgrResponse { status = true };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理初始化语音管理器请求时出错: {ex.Message}");
            var errorResponse = new InitAudioMgrResponse { status = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}

public class UnInitAudioMgrHandler : IMiniGameCallHostHandler
{
    public string Name => "unInitAudioMgr";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();
            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            audioMgr.UnInit();

            var response = new UnInitAudioMgrResponse { status = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理销毁语音管理器请求时出错: {ex.Message}");
            var errorResponse = new UnInitAudioMgrResponse { status = true };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}

public class EnterRoomHandler : IMiniGameCallHostHandler
{
    public string Name => "enterRoom";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (EnterRoomRequest)(request.data);
            string roomId = requestData.roomId;
            string streamId = requestData.streamId; // 可能为空

            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();
            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            // --- 调用 C# 侧的 TRTCTeamAudio 进入房间逻辑 ---
            audioMgr.EnterRoom(roomId, streamId);

            var response = new EnterRoomResponse { success = true, message = "Enter room request sent." };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理进入语音房请求时出错: {ex.Message}");
            var errorResponse = new EnterRoomResponse { success = false, message = ex.Message };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }


}

public class ExitRoomHandler : IMiniGameCallHostHandler
{
    public string Name => "exitRoom";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();
            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            audioMgr.ExitRoom();

            var response = new ExitRoomResponse { status = true };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理退出语音房请求时出错: {ex.Message}");
            var errorResponse = new ExitRoomResponse { status = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }

}

public class SetMicStateHandler : IMiniGameCallHostHandler
{
    public string Name => "setMicState";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (SetMicStateRequest)(request.data);
            bool enable = requestData.enable;

            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();
            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            audioMgr.SetMic(enable);

            var response = new SetMicStateResponse { status = true };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理设置麦克风状态请求时出错: {ex.Message}");
            var errorResponse = new SetMicStateResponse { status = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}

public class SetSpeakerStateHandler : IMiniGameCallHostHandler
{
    public string Name => "setSpeakerState";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (SetSpeakerStateRequest)(request.data);
            bool enable = requestData.enable;

            var audioMgr = HostMiniGameManager.Instance.GetTRTCTeamAudio();
            if (audioMgr == null)
            {
                throw new InvalidOperationException("TRTCTeamAudio instance is not available.");
            }

            // --- 调用 C# 侧的 TRTCTeamAudio 设置扬声器状态逻辑 ---
            audioMgr.SetSpeaker(enable);

            var response = new SetSpeakerStateResponse() { status = true };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
            callback(bridgeResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理设置扬声器状态请求时出错: {ex.Message}");
            var errorResponse = new SetSpeakerStateResponse { status = false };
            var bridgeResponse = new BridgeResponse(request.id, request.method, false, errorResponse, ex.Message);
            callback(bridgeResponse);
        }
    }
}


// wp.login Handler
public class WPLoginHandler : IMiniGameCallHostHandler
{
    public string Name => "wpLogin";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (WPLoginRequest)(request.data);

            if (requestData.client_id <= 0)
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "client_id is invalid");
                callback(errorResponse);
                return;
            }

#if NATIVE_BIND_ENABLE
            // 序列化为JSON字符串
            string jsonData = JsonConvert.SerializeObject(requestData);

            NativeBridge.UnityCallNative("wpLogin", jsonData, (rsp) =>
            {
                if (rsp.code == NativeBridgeResponse.Code.Success)
                {
                    var response = JsonConvert.DeserializeObject<WPLoginResponse>(rsp.msg);
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                    callback(bridgeResponse);
                }
                else
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                    callback(errorResponse);
                }
            });
#else
            // 非原生环境模拟
            var mockResponse = new WPLoginResponse { code = "mock_login_code_" + DateTime.Now.Ticks.ToString() };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResponse);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in wpLogin: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

// wp.loadFinish Handler
public class WPLoadFinishHandler : IMiniGameCallHostHandler
{
    public string Name => "wpLoadFinish";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (WPLoadFinishRequest)(request.data);

            if (requestData.client_id <= 0)
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "client_id is invalid");
                callback(errorResponse);
                return;
            }

            if (requestData.result != 1 && requestData.result != 2)
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "result must be 1 (success) or 2 (failed)");
                callback(errorResponse);
                return;
            }

#if NATIVE_BIND_ENABLE
            string jsonData = JsonConvert.SerializeObject(requestData);

            NativeBridge.UnityCallNative("wpLoadFinish", jsonData, (rsp) =>
            {
                if (rsp.code == NativeBridgeResponse.Code.Success)
                {
                    var response = new WPLoadFinishResponse();
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                    callback(bridgeResponse);
                }
                else
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                    callback(errorResponse);
                }
            });
#else
            Debug.Log($"[NATIVE_BIND_DISABLE] Simulating wpLoadFinish for client_id: {requestData.client_id}, result: {requestData.result}");
            var mockResponse = new WPLoadFinishResponse();
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResponse);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in wpLoadFinish: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

// wp.recharge Handler
public class WPRechargeHandler : IMiniGameCallHostHandler
{
    public string Name => "wpRecharge";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (WPRechargeRequest)(request.data);

            if (requestData.client_id <= 0 || requestData.gold_num <= 0 || string.IsNullOrEmpty(requestData.order_des) || string.IsNullOrEmpty(requestData.order_Info))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "client_id, gold_num, order_des, and order_info are required");
                callback(errorResponse);
                return;
            }

#if NATIVE_BIND_ENABLE
            string jsonData = JsonConvert.SerializeObject(requestData);

            NativeBridge.UnityCallNative("wpRecharge", jsonData, (rsp) =>
            {
                if (rsp.code == NativeBridgeResponse.Code.Success)
                {
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, null,rsp.msg);
                    callback(bridgeResponse);
                }
                else
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                    callback(errorResponse);
                }
            });
#else
            Debug.Log($"[NATIVE_BIND_DISABLE] Simulating wpRecharge for client_id: {requestData.client_id}, gold_num: {requestData.gold_num}");
            var mockResponse = new WPRechargeResponse
            {
                success = true
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResponse);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in wpRecharge: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

// wp.share Handler
public class WPShareHandler : IMiniGameCallHostHandler
{
    public string Name => "wpShare";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (WPShareRequest)(request.data);

            if (requestData.client_id <= 0 || string.IsNullOrEmpty(requestData.title) || string.IsNullOrEmpty(requestData.desc) || string.IsNullOrEmpty(requestData.link) || requestData.shareContentType < 1 || requestData.shareContentType > 2 || string.IsNullOrEmpty(requestData.ext))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "Required parameters missing or invalid");
                callback(errorResponse);
                return;
            }

            // 根据shareContentType校验必传参数
            if (requestData.shareContentType == 1 && string.IsNullOrEmpty(requestData.imgUrl))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "imgUrl is required for shareContentType=1");
                callback(errorResponse);
                return;
            }
            if (requestData.shareContentType == 2 && (string.IsNullOrEmpty(requestData.imgUrl) || !requestData.deepLinkToGame))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "imgUrl and deepLinkToGame are required for shareContentType=2");
                callback(errorResponse);
                return;
            }

#if NATIVE_BIND_ENABLE
            string jsonData = JsonConvert.SerializeObject(requestData);

            NativeBridge.UnityCallNative("wpShare", jsonData, (rsp) =>
            {
                if (rsp.code == NativeBridgeResponse.Code.Success)
                {
                    var response = new WPShareResponse
                    {
                        success = true,
                        message = "Share success"
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                    callback(bridgeResponse);
                }
                else
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                    callback(errorResponse);
                }
            });
#else
            Debug.Log($"[NATIVE_BIND_DISABLE] Simulating wpShare - Title: {requestData.title}, Type: {requestData.shareContentType}, Client: {requestData.client_id}");
            var mockResponse = new WPShareResponse
            {
                success = true,
                message = "Mock share success"
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResponse);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in wpShare: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

// wp.addFriend Handler 
public class WPAddFriendHandler : IMiniGameCallHostHandler
{
    public string Name => "wpAddFriend";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (WPAddFriendRequest)(request.data);

            if (requestData.client_id <= 0 || string.IsNullOrEmpty(requestData.open_id))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "client_id and open_id are required");
                callback(errorResponse);
                return;
            }

#if NATIVE_BIND_ENABLE
            string jsonData = JsonConvert.SerializeObject(requestData);

            NativeBridge.UnityCallNative("wpAddFriend", jsonData, (rsp) =>
            {
                if (rsp.code == NativeBridgeResponse.Code.Success)
                {
                    var response = JsonConvert.DeserializeObject<WPAddFriendResponse>(rsp.msg);
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                    callback(bridgeResponse);
                }
                else
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                    callback(errorResponse);
                }
            });
#else
            Debug.Log($"[NATIVE_BIND_DISABLE] Simulating wpAddFriend - Client: {requestData.client_id}, Target OpenID: {requestData.open_id}");
            var mockResponse = new WPAddFriendResponse
            {
                status = "success" // 模拟成功添加
            };
            var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResponse);
            callback(bridgeResponse);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in wpAddFriend: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}

public class InviteUserToGameHandler : IMiniGameCallHostHandler
{
    public string Name => "inviteUserToGame";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (InviteUserToGameRequest)(request.data);

            // 参数校验
            if (requestData.game_type <= 0 || (requestData.gid <= 0 && (requestData.user_info == null)))
            {
                var errorResponse = new BridgeResponse(request.id, request.method, false, null, "game_type is required, and either gid or user_info must be provided");
                callback(errorResponse);
                return;
            }

            // 如果指定了 gid，则直接调用原生接口
            if (requestData.gid > 0)
            {
#if NATIVE_BIND_ENABLE
                // 调用原生接口时，gid 不需要转换
                string jsonData = JsonConvert.SerializeObject(requestData); // requestData.user_info 可能为 null，不影响 gid 的调用

                NativeBridge.UnityCallNative("inviteUserToGame", jsonData, (rsp) =>
                {
                    if (rsp.code == NativeBridgeResponse.Code.Success)
                    {
                        var response = JsonConvert.DeserializeObject<InviteUserToGameResponse>(rsp.msg);
                        var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                        callback(bridgeResponse);
                    }
                    else
                    {
                        var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                        callback(errorResponse);
                    }
                });
#else
                Debug.Log($"[NATIVE_BIND_DISABLE] Simulating inviteUserToGame by GID - Game: {requestData.game_type}, GID: {requestData.gid}, RID: {requestData.rid}");
                var mockResult = "{\"success\": true, \"message\": \"Invite sent successfully\", \"invite_id\": \"mock_invite_123\"}";
                var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResult);
                callback(bridgeResponse);
#endif
            }
            else
            {
                if (requestData.user_info == null && requestData.gid == 0)
                {
                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, "user_info is required when gid is not provided");
                    callback(errorResponse);
                    return;
                }

                if (!string.IsNullOrEmpty(requestData.user_info.openId))
                {

                    string[] openIdArray = { requestData.user_info.openId };

                    HttpClient.Instance.RequestOpenId2uid(openIdArray,
                        (result) =>
                        {
                            try
                            {
                                var uids = HostMiniGameManager.Instance.GetUidList(result);
                                if (uids == null || uids.Count == 0)
                                {
                                    Debug.LogError("RequestOpenId2uid returned no UIDs.");
                                    var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "Failed to resolve UID from OpenID");
                                    callback(response);
                                    return;
                                }

                                int targetUid = uids[0];
                                Debug.Log($"Resolved UID: {targetUid} from OpenID: {requestData.user_info.openId}");

                                var nativeRequestData = new NativeInviteUserToGameRequest
                                {
                                    method = requestData.method,
                                    gid = requestData.gid, // gid 保持不变
                                game_type = requestData.game_type,
                                    rid = requestData.rid,
                                    msg_content = requestData.msg_content,
                                    follow_type = requestData.follow_type,
                                    validendtime = requestData.validendtime,
                                    user_info = new NativeInviteUserToGameUserInfo // 转换 user_info
                                {
                                        uid = targetUid, // 使用转换后的 uid
                                    remark_name = requestData.user_info.remark_name,
                                        nickname = requestData.user_info.nickname,
                                        avatar = requestData.user_info.avatar,
                                        gender = requestData.user_info.gender
                                    }
                                };

#if NATIVE_BIND_ENABLE
                            string jsonData = JsonConvert.SerializeObject(nativeRequestData);
                            NativeBridge.UnityCallNative("inviteUserToGame", jsonData, (rsp) =>
                            {
                                if (rsp.code == NativeBridgeResponse.Code.Success)
                                {
                                    var response = JsonConvert.DeserializeObject<InviteUserToGameResponse>(rsp.msg);
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                    callback(bridgeResponse);
                                }
                                else
                                {
                                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                                    callback(errorResponse);
                                }
                            });
#else
                            Debug.Log($"[NATIVE_BIND_DISABLE] Simulating inviteUserToGame by UID - Game: {nativeRequestData.game_type}, UID: {targetUid}, RID: {nativeRequestData.rid}");
                                var mockResult = "{\"success\": true, \"message\": \"Invite sent successfully\", \"invite_id\": \"mock_invite_123\"}";
                                var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResult);
                                callback(bridgeResponse);
#endif
                        }
                            catch (Exception parseEx)
                            {
                                Debug.LogError($"Parse UID error: {parseEx.Message}");
                                var response = new BridgeResponse(request.id, request.method, false, new { success = false }, parseEx.Message);
                                callback(response);
                            }
                        },
                        (timeoutResult) =>
                        {
                            Debug.LogError("RequestOpenId2uid timeout for openId: " + requestData.user_info.openId);
                            var response = new BridgeResponse(request.id, request.method, false, new { success = false }, "OpenID to UID conversion timeout");
                            callback(response);
                        }
                    );
                }
                else
                {

                    var targetUid = HostMiniGameManager.Instance.DecodeInviteId(requestData.user_info.inviteId);
                    var nativeRequestData = new NativeInviteUserToGameRequest
                    {
                        method = requestData.method,
                        gid = requestData.gid, // gid 保持不变
                        game_type = requestData.game_type,
                        rid = requestData.rid,
                        msg_content = requestData.msg_content,
                        follow_type = requestData.follow_type,
                        validendtime = requestData.validendtime,
                        user_info = new NativeInviteUserToGameUserInfo // 转换 user_info
                        {
                            uid = targetUid, // 使用转换后的 uid
                            remark_name = requestData.user_info.remark_name,
                            nickname = requestData.user_info.nickname,
                            avatar = requestData.user_info.avatar,
                            gender = requestData.user_info.gender
                        }
                    };

#if NATIVE_BIND_ENABLE
                            string jsonData = JsonConvert.SerializeObject(nativeRequestData);
                            NativeBridge.UnityCallNative("inviteUserToGame", jsonData, (rsp) =>
                            {
                                if (rsp.code == NativeBridgeResponse.Code.Success)
                                {
                                    var response = JsonConvert.DeserializeObject<InviteUserToGameResponse>(rsp.msg);
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                    callback(bridgeResponse);
                                }
                                else
                                {
                                    var errorResponse = new BridgeResponse(request.id, request.method, false, null, rsp.msg);
                                    callback(errorResponse);
                                }
                            });
#else
                    Debug.Log($"[NATIVE_BIND_DISABLE] Simulating inviteUserToGame by UID - Game: {nativeRequestData.game_type}, UID: {targetUid}, RID: {nativeRequestData.rid}");
                    var mockResult = "{\"success\": true, \"message\": \"Invite sent successfully\", \"invite_id\": \"mock_invite_123\"}";
                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, mockResult);
                    callback(bridgeResponse);
#endif

                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in inviteUserToGame: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }
}


public class RequestAuthorizeHandler : IMiniGameCallHostHandler
{
    public string Name => "requestAuthorize";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (AuthorizeRequest)(request.data);
            Debug.Log($"Host processing requestAuthorize - ClientId: {requestData.client_id}, Scope: {requestData.scope}");

            if (string.IsNullOrEmpty(requestData.client_id))
            {
                Debug.LogError("RequestAuthorizeHandler: client_id is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "client_id is required");
                callback(response);
                return;
            }

            if (string.IsNullOrEmpty(requestData.scope))
            {
                Debug.LogError("RequestAuthorizeHandler: scope is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "scope is required");
                callback(response);
                return;
            }

            // --- 获取当前用户的 UID 和 SID ---
            int currentUid = GetCurrentUserUID();
            string currentSid = GetCurrentUserSID();

#if NATIVE_BIND_ENABLE
                UnityCallNativeImpl.GetCurrentUID(
                    (uid, sid) =>
                    {
                        Debug.Log($"Got UID and SID from native: {uid}, {sid}");
                        // 调用 HttpClient 发起实际的网络请求
                        HttpClient.Instance.RequestAuthorize(
                            uid, // 使用从原生获取的 uid
                            requestData.client_id,
                            requestData.scope,
                            requestData.state,
                            // 成功回调
                            (result) =>
                            {
                                try
                                {
                                    // 假设 result 是一个包含授权结果的对象
                                    var response = new RequestAuthorizeResponse
                                    {
                                        access_token = HostMiniGameManager.Instance.GetToekn(result),
                                        success = true,
                                        message = "Authorization successful"
                                    };
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                    callback(bridgeResponse);
                                }
                                catch (Exception parseEx)
                                {
                                    Debug.LogError($"Parse authorize result error: {parseEx.Message}");
                                    var response = new RequestAuthorizeResponse
                                    {
                                        success = false,
                                        message = "Failed to parse result"
                                    };
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                                    callback(bridgeResponse);
                                }
                            },
                            // 超时回调
                            (timeoutResult) =>
                            {
                                Debug.LogError("RequestAuthorize timeout");
                                var response = new RequestAuthorizeResponse
                                {
                                    success = false,
                                    message = "Request timeout"
                                };
                                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "Request timeout");
                                callback(bridgeResponse);
                            }
                        );
                    },
                    (code, msg) =>
                    {
                        Debug.LogError($"Failed to get UID from native: {code}, {msg}");
                        var errorResponse = new BridgeResponse(request.id, request.method, false, null, msg);
                        callback(errorResponse);
                    }
                );
#else
            // 非原生模式，直接使用模拟的 UID 和 SID
            Debug.Log($"[NATIVE_BIND_DISABLE] Using mock UID: {currentUid}, SID: {currentSid}");
            HttpClient.Instance.RequestAuthorize(
                currentUid, // 使用模拟的 uid
                requestData.client_id,
                requestData.scope,
                requestData.state,
                // 成功回调
                (result) =>
                {
                    try
                    {
                        var response = new RequestAuthorizeResponse
                        {
                            success = true,
                            access_token = HostMiniGameManager.Instance.GetToekn(result),
                            message = "Authorization successful (mock)"
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                        callback(bridgeResponse);
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse authorize result error: {parseEx.Message}");
                        var response = new RequestAuthorizeResponse
                        {
                            success = false,
                            message = "Failed to parse result"
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                        callback(bridgeResponse);
                    }
                },
                // 超时回调
                (timeoutResult) =>
                {
                    Debug.LogError("RequestAuthorize timeout");
                    var response = new RequestAuthorizeResponse
                    {
                        success = false,
                        message = "Request timeout"
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "Request timeout");
                    callback(bridgeResponse);
                }
            );
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in requestAuthorize: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }

    private int GetCurrentUserUID()
    {
        // 模拟 UID
        return 75228697; // 与 Handler 文件中的示例一致
    }

    private string GetCurrentUserSID()
    {
        // 模拟 SID
        return "session_abc123"; // 与 Handler 文件中的示例一致
    }
}

// 请求创建订单处理器 (更新版)
public class RequestOrderHandler : IMiniGameCallHostHandler
{
    public string Name => "requestOrder";

    public void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback)
    {
        try
        {
            var requestData = (OrderRequest)(request.data);
            Debug.Log($"Host processing requestOrder - ClientId: {requestData.client_id}, PriceId: {requestData.price_id}, Name: {requestData.name}");

            if (string.IsNullOrEmpty(requestData.client_id))
            {
                Debug.LogError("RequestOrderHandler: client_id is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "client_id is required");
                callback(response);
                return;
            }

            if (string.IsNullOrEmpty(requestData.name))
            {
                Debug.LogError("RequestOrderHandler: name is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "name is required");
                callback(response);
                return;
            }

            if (string.IsNullOrEmpty(requestData.trade_id))
            {
                Debug.LogError("RequestOrderHandler: trade_id is null or empty!");
                var response = new BridgeResponse(request.id, request.method, false, null, "trade_id is required");
                callback(response);
                return;
            }

            // --- 获取当前用户的 UID 和 SID ---
            int currentUid = GetCurrentUserUID();
            string currentSid = GetCurrentUserSID();

#if NATIVE_BIND_ENABLE
                UnityCallNativeImpl.GetCurrentUID(
                    (uid, sid) =>
                    {
                        Debug.Log($"Got UID and SID from native: {uid}, {sid}");
                        // 调用 HttpClient 发起实际的网络请求
                        HttpClient.Instance.RequestOrder(
                            uid, // 使用从原生获取的 uid
                            requestData.client_id,
                            requestData.price_id,
                            requestData.name,
                            requestData.trade_id,
                            requestData.extra,
                            requestData.callback,
                            // 成功回调
                            (result) =>
                            {
                                try
                                {
                                    // 假设 result 是一个包含订单创建结果的对象
                                    var response = new RequestOrderResponse
                                    {
                                        success = true,
                                        order_id = "mock_order_123", // 模拟返回订单ID
                                        message = "Order created successfully"
                                    };
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                                    callback(bridgeResponse);
                                }
                                catch (Exception parseEx)
                                {
                                    Debug.LogError($"Parse order result error: {parseEx.Message}");
                                    var response = new RequestOrderResponse
                                    {
                                        success = false,
                                        message = "Failed to parse result"
                                    };
                                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                                    callback(bridgeResponse);
                                }
                            },
                            // 超时回调
                            (timeoutResult) =>
                            {
                                Debug.LogError("RequestOrder timeout");
                                var response = new RequestOrderResponse
                                {
                                    success = false,
                                    message = "Request timeout"
                                };
                                var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "Request timeout");
                                callback(bridgeResponse);
                            }
                        );
                    },
                    (code, msg) =>
                    {
                        Debug.LogError($"Failed to get UID from native: {code}, {msg}");
                        var errorResponse = new BridgeResponse(request.id, request.method, false, null, msg);
                        callback(errorResponse);
                    }
                );
#else
            // 非原生模式，直接使用模拟的 UID 和 SID
            Debug.Log($"[NATIVE_BIND_DISABLE] Using mock UID: {currentUid}, SID: {currentSid}");
            HttpClient.Instance.RequestOrder(
                currentUid, // 使用模拟的 uid
                requestData.client_id,
                requestData.price_id,
                requestData.name,
                requestData.trade_id,
                requestData.extra,
                requestData.callback,
                // 成功回调
                (result) =>
                {
                    try
                    {
                        var response = new RequestOrderResponse
                        {
                            success = true,
                            order_id = "mock_order_123",
                            message = "Order created successfully (mock)"
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, true, response);
                        callback(bridgeResponse);
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogError($"Parse order result error: {parseEx.Message}");
                        var response = new RequestOrderResponse
                        {
                            success = false,
                            message = "Failed to parse result"
                        };
                        var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, parseEx.Message);
                        callback(bridgeResponse);
                    }
                },
                // 超时回调
                (timeoutResult) =>
                {
                    Debug.LogError("RequestOrder timeout");
                    var response = new RequestOrderResponse
                    {
                        success = false,
                        message = "Request timeout"
                    };
                    var bridgeResponse = new BridgeResponse(request.id, request.method, false, response, "Request timeout");
                    callback(bridgeResponse);
                }
            );
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in requestOrder: {ex.Message}");
            var errorResponse = new BridgeResponse(request.id, request.method, false, null, ex.Message);
            callback(errorResponse);
        }
    }

    private int GetCurrentUserUID()
    {
        // 模拟 UID
        return 75304977; // 与 Handler 文件中的示例一致
    }

    private string GetCurrentUserSID()
    {
        // 模拟 SID
        return "session_abc123"; // 与 Handler 文件中的示例一致
    }
}

[System.Serializable]
public class NativeInviteUserToGameRequest
{
    public string method { get; set; } = "inviteUserToGame";
    public int gid { get; set; }              // 群ID
    public int game_type { get; set; }        // 游戏类型
    public int rid { get; set; }              // 房间ID
    public string msg_content { get; set; } = "";   // 消息内容
    public int follow_type { get; set; } = 1;       // 邀请跟随类型
    public int validendtime { get; set; }           // 邀请有效期截止时间戳
    public NativeInviteUserToGameUserInfo user_info { get; set; } // 用户信息（使用 uid）
}

[System.Serializable]
public class NativeInviteUserToGameUserInfo
{
    public int uid { get; set; } // 使用 uid
    public string remark_name { get; set; }
    public string nickname { get; set; }
    public string avatar { get; set; }
    public int gender { get; set; }
}