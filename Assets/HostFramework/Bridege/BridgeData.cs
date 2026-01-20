using System;
using System.Collections.Generic;
using UnityEngine;


public class GetCurrentOpenIdRequest
{
    public string method { get; } = "getCurrentOpenId";
}

// 获取当前用户ID响应
public class GetOpenIdResponse
{
    public string openId { get; set; }
    public string sid { get; set; }
}


// 加好友请求
public class AddFriendRequest
{
    public string method { get; } = "addFriend";
    public string openId { get; set; }
    public int game_type { get; set; }
    public string sub_source { get; set; } = "";
    public string screen_name { get; set; } = "";
}

// 加好友（无对话框）请求
public class AddFriendWithoutDialogRequest
{
    public string method { get; } = "addFriendWithoutDialog";
    public string openId { get; set; }
    public int target_flower { get; set; }
    public string content { get; set; } = "";
}

// 是否是好友请求
public class IsFriendRequest
{
    public string method { get; } = "isFriend";
    public string openId { get; set; }
}

// 是否是好友响应
public class IsFriendResponse
{
    public bool is_friend { get; set; }
}

// 获取用户个人信息请求
public class GetUserInfoRequest
{
    public string method { get; } = "getUserInfo";
    public string openId { get; set; }
}

// 获取用户个人信息响应
public class GetUserInfoResponse
{
    public string openId { get; set; }
    public string headimgurl { get; set; }
    public string nickname { get; set; }
    public int gender { get; set; }
 
}

// 分享请求
public class ShareRequest
{
    public string method { get; } = "share";
    public int game_type { get; set; }
    public int type { get; set; } //  20=推特、21=复制链家、5=玩友圈 
    public string title { get; set; }
    public string desc { get; set; }
    public string link { get; set; }
    public string icon_url { get; set; }
    public string image { get; set; } // base64
}

// 邀请请求
public class InviteRequest
{
    public string method { get; } = "invite";
    public int game_type { get; set; }
    public int gid { get; set; }
    public bool notify_result { get; set; }
}

// 获取最近联系人列表请求
public class GetLatestFriendsRequest
{
    public string method { get; } = "getLatestFriends";
    public string openId { get; set; }
    public int game_type { get; set; }
    public int count { get; set; }
}

// 弹出原生Toast请求
public class ToastRequest
{
    public string method { get; } = "toast";
    public int type { get; set; }
    public string content { get; set; }
}

// 请求支付请求
public class RequestPayRequest
{
    public string method { get; } = "requestPay";
    public bool portrait { get; set; }
}

// 获取最近联系人列表响应
public class GetLatestFriendsResponse
{
    public List<ConvertedFriendInfo> result;
    public List<ConvertedTotalFriendInfo> total_result;
}

// Toast响应
public class ToastResponse
{
    public bool success { get; set; }
}

// 请求支付响应
public class RequestPayResponse
{
    public string message { get; set; }
    // 可能需要根据实际支付结果添加更多字段
}

// 获取当前授权状态请求
public class GetAudioAuthStatusRequest
{
    public string method { get; } = "getAudioAuthStatus"; 
    // 无参数
}

// 获取当前授权状态响应
public class GetAudioAuthStatusResponse
{
    public bool authStatus { get; set; } 
}

// 申请麦克风授权(不带不再提醒)请求
public class RequestAudioNoRemindPermissionRequest
{
    public string method { get; } = "requestAudioNoRemindPermission";
    // 无参数
}

// 申请麦克风授权(不带不再提醒)响应
public class RequestAudioNoRemindPermissionResponse
{
    public bool granted { get; set; } // true 同意, false 拒绝
}


public class InitAudioMgrRequest
{
    public string method { get; } = "initAudioMgr";
    // 无额外参数
}

// 初始化语音管理器响应
public class InitAudioMgrResponse
{
    // 通常初始化操作成功与否通过 BridgeResponse 的 success 字段表示
    // 但也可以包含一些初始化后返回的状态信息
    public bool status { get; set; } = true;
}

// 销毁语音管理器请求 (无参数)
public class UnInitAudioMgrRequest
{
    public string method { get; } = "unInitAudioMgr";
    // 无额外参数
}

// 销毁语音管理器响应
public class UnInitAudioMgrResponse
{
    public bool status { get; set; } = true;
}

// 进入语音房请求
public class EnterRoomRequest
{
    public string method { get; } = "enterRoom";
    public string roomId { get; set; }
    public string streamId { get; set; } // 可选，根据实际需要决定是否传递
}

// 进入语音房响应
public class EnterRoomResponse
{
    // 通常通过 BridgeResponse.success 表示操作是否被成功发起
    // 如果 TRTC 有更详细的进房结果，可以在这里添加字段
    public bool success { get; set; } = false;
    public string message { get; set; } = "";
}

// 退出语音房请求 (无参数)
public class ExitRoomRequest
{
    public string method { get; } = "exitRoom";
    // 无额外参数
}

// 退出语音房响应 (无数据或仅含状态)
public class ExitRoomResponse
{
    public bool status { get; set; } = true;
}

// 设置麦克风状态请求
public class SetMicStateRequest
{
    public string method { get; } = "setMicState";
    public bool enable { get; set; }
}

// 设置麦克风状态响应
public class SetMicStateResponse
{
    public bool status { get; set; } = true;
}

// 设置扬声器状态请求
public class SetSpeakerStateRequest
{
    public string method { get; } = "setSpeakerState";
    public bool enable { get; set; }
}

// 设置扬声器状态响应
public class SetSpeakerStateResponse
{
    public bool status { get; set; } = true;
}




// wp.login 请求
public class WPLoginRequest
{
    public string method { get; } = "wpLogin";
    public int client_id { get; set; }
    public string state { get; set; } // 可选参数
}

// wp.login 响应
public class WPLoginResponse
{
    public string code { get; set; } // 文档中返回的result.code
}

// wp.loadFinish 请求
public class WPLoadFinishRequest
{
    public string method { get; } = "wpLoadFinish";
    public int client_id { get; set; }
    public int result { get; set; } // 1=加载成功，2=加载失败
    public string failMsg { get; set; } // 加载失败原因
}

// wp.loadFinish 响应
public class WPLoadFinishResponse
{
    // 无具体数据，仅表示操作完成
}

// wp.recharge 请求
public class WPRechargeRequest
{
    public string method { get; } = "wpRecharge";
    public int client_id { get; set; }
    public int gold_num { get; set; } // 需要兑换的金币数量
    public string order_des { get; set; } // 充值弹窗描述信息
    public string order_Info { get; set; } // 订单相关信息
}

// wp.recharge 响应 (假设返回订单ID)
public class WPRechargeResponse
{
    public bool success { get; set; } // 支付是否成功
}

// wp.share 请求
public class WPShareRequest
{
    public string method { get; } = "wpShare";
    public int client_id { get; set; }
    public string title { get; set; } // 分享标题
    public string desc { get; set; } // 分享描述
    public string imgUrl { get; set; } // 分享图片链接 (非必传)
    public string link { get; set; } // 分享URL (必传)
    public bool deepLinkToGame { get; set; } // 是否打开游戏 (非必传)
    public int shareContentType { get; set; } // 1=纯图片, 2=h5方式 (必传)
    public string ext { get; set; } // 三方拓展字段 (必传)
}

// wp.share 响应 (假设返回分享状态)
public class WPShareResponse
{
    public bool success { get; set; }
    public string message { get; set; }
}

// wp.addFriend 请求 
public class WPAddFriendRequest
{
    public string method { get; } = "wpAddFriend";
    public int client_id { get; set; }
    public string open_id { get; set; } // 目标用户ID (不是uid!)
    public string sub_source { get; set; } // 场景描述，可选
    public string screen_name { get; set; } // 界面描述，可选
}

// wp.addFriend 响应
public class WPAddFriendResponse
{
    public string status { get; set; } // "success" 或 "cancel"
}

public class InviteUserToGameRequest
{
    public string method { get; } = "inviteUserToGame";
    public int gid { get; set; }              // 群ID（优先级高于 user_info）
    public int game_type { get; set; }        // 游戏类型
    public int rid { get; set; }              // 房间ID
    public string msg_content { get; set; } = "";   // 消息内容（非空时使用此内容发送消息）
    public int follow_type { get; set; } = 1;       // 邀请跟随类型，1 - 召唤
    public int validendtime { get; set; }           // 邀请有效期截止时间戳（秒），为0时默认10分钟
    public InviteUserToGameUserInfo user_info { get; set; } // 用户信息（当gid为空时使用）
}

// 邀请用户进入某游戏请求中的用户信息
public class InviteUserToGameUserInfo
{
    public string openId { get; set; }
    public int inviteId { get; set; }
    public string remark_name { get; set; }
    public string nickname { get; set; }
    public string avatar { get; set; }
    public int gender { get; set; }
}

public class InviteUserToGameResponse
{ 
     public bool success { get; set; }
     public string message { get; set; }
}

[System.Serializable]
public class WrapperArray<T>
{
    public T[] data;
}

[System.Serializable]
public class ConvertedFriendInfo
{
    public int inviteId;
    public string remark_name;
    public string nickname;
    public string avatar;
    public int gender;

}

[System.Serializable]
public class ConvertedTotalFriendInfo
{
    public bool is_group;
    public int inviteId;
    public int group_id;
    public string remark_name;
    public string nickname;
    public string avatar;
    public int gender;
    public int[]? group_inviteIds;
}


[System.Serializable]
public class AuthorizeRequest
{
    public string method { get; } = "requestAuthorize"; // 定义方法名，与 Handler 对应
    public string client_id { get; set; }
    public string scope { get; set; }
    public string state { get; set; } = "";
}


[System.Serializable]
public class OrderRequest
{
    public string method { get; } = "requestOrder"; // 定义方法名，与 Handler 对应
    public string client_id { get; set; }
    public int price_id { get; set; }     // 价格ID
    public string name { get; set; }      // 商品名称
    public string trade_id { get; set; }  // 交易ID（商户自定义）
    public string extra { get; set; }     // 附加数据（可选）
    public string callback { get; set; }  // 支付成功回调地址（可选）
}

[System.Serializable]
public class RequestAuthorizeResponse
{
    public bool success { get; set; }
    public string access_token { get; set; }

    public string message { get; set; }
}

// RequestOrder 响应
[System.Serializable]
public class RequestOrderResponse
{
    public bool success { get; set; }
    public string order_id { get; set; } // 订单ID
    public string message { get; set; }
   
}

[System.Serializable]
public class ShareThirdPlatformH5Req
{
    public string method { get; } = "shareThirdPlatformH5"; // 定义方法名，与 Handler 对应
    public int platform_type;   // 平台类型
    public int media_type;      // 媒体类型
    public string title;        // 标题
    public string desc;         // 描述
    public string url;          // 分享链接
    public string image_url;    // 图片地址
    public int game_type;       // 游戏类型（新增）
    public string rid;          // rid（新增）
    public string topic;        // JSON 字符串，如 "{\"topic_name\":\"xxx\",\"topic_id\":\"1233\"}"
}


[System.Serializable]
public class ShareThirdPlatformH5Rsp
{
    public bool success;
    public string message;
}