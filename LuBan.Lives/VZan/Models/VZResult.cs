/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Lives.VZan.Models
*文件名： VZResult
*版本号： V1.0.0.0
*唯一标识：3aaf45e3-99ee-4ab0-8163-82eeb319d19e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/19 17:13:45
*描述：
*
*=================================================
*修改标记
*修改时间：2024/9/19 17:13:45
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Lives.VZan.Models
{
    public class VZResult<T>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public T data { get; set; }
    }

    [DataContract]
    public class VZLiveUserInfo
    {
        /// <summary>
        /// Examples: "用户Id（密文）"
        /// </summary>
        [DataMember(Name = "uid")]
        public string Uid { get; set; }

        /// <summary>
        /// Examples: "用户Id"
        /// </summary>
        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Examples: "用户昵称"
        /// </summary>
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// Examples: "用户头像"
        /// </summary>
        [DataMember(Name = "avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// Examples: "用户性别（0:未知,1:男,2:女）"
        /// </summary>
        [DataMember(Name = "gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Examples: "用户手机号码"
        /// </summary>
        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Examples: "用户真实姓名"
        /// </summary>
        [DataMember(Name = "realName")]
        public string RealName { get; set; }

        /// <summary>
        /// Examples: "采集来源"
        /// </summary>
        [DataMember(Name = "gather_from")]
        public string GatherFrom { get; set; }

        /// <summary>
        /// Examples: "首次进入时间"
        /// </summary>
        [DataMember(Name = "in_time")]
        public string InTime { get; set; }

        /// <summary>
        /// Examples: "最后离开时间"
        /// </summary>
        [DataMember(Name = "out_time")]
        public string OutTime { get; set; }

        /// <summary>
        /// Examples: "用户ip"
        /// </summary>
        [DataMember(Name = "ip")]
        public string Ip { get; set; }

        /// <summary>
        /// Examples: "第三方用户Id"
        /// </summary>
        [DataMember(Name = "tuid")]
        public string Tuid { get; set; }

        /// <summary>
        /// Examples: "第三方openid"
        /// </summary>
        [DataMember(Name = "third_openid")]
        public string ThirdOpenid { get; set; }

        /// <summary>
        /// Examples: "用户观看直播时长(单位：秒)"
        /// </summary>
        [DataMember(Name = "live_duration")]
        public int LiveDuration { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长(单位：秒)"
        /// </summary>
        [DataMember(Name = "review_duration")]
        public int ReviewDuration { get; set; }

        /// <summary>
        /// 观看话题数
        /// </summary>
        [DataMember(Name = "watch_topic_count")]
        public int WatchTopicCount { get; set; }

        /// <summary>
        /// Examples: "用户观看直播时长"
        /// </summary>
        [DataMember(Name = "live_time")]
        public string LiveTime { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长"
        /// </summary>
        [DataMember(Name = "review_time")]
        public string ReviewTime { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长"
        /// </summary>
        [DataMember(Name = "dwell_time")]
        public string DwellTime { get; set; }

        /// <summary>
        /// Examples: "最新访问IP"
        /// </summary>
        [DataMember(Name = "recent_ip")]
        public string RecentIp { get; set; }

        /// <summary>
        /// Examples: "最近访问时间"
        /// </summary>
        [DataMember(Name = "recent_enter_time")]
        public string RecentEnterTime { get; set; }

        /// <summary>
        /// Examples: "最近访问终端"
        /// </summary>
        [DataMember(Name = "recent_login_terminal")]
        public string RecentLoginTerminal { get; set; }

        /// <summary>
        /// Examples: "邀请人Id"
        /// </summary>
        [DataMember(Name = "share_uid")]
        public string ShareUid { get; set; }

        /// <summary>
        /// Examples: "邀请人第三方用户Id"
        /// </summary>
        [DataMember(Name = "share_tuid")]
        public string ShareTuid { get; set; }

        /// <summary>
        /// Examples: "邀请人名称"
        /// </summary>
        [DataMember(Name = "share_username")]
        public string ShareUsername { get; set; }

        /// <summary>
        /// Examples: "渠道来源"
        /// </summary>
        [DataMember(Name = "channel_origin")]
        public string ChannelOrigin { get; set; }
    }


    [DataContract]
    public class VZLiveUserList
    {
        /// <summary>
        /// Examples: [{"uid":"用户Id（密文）","userId":"用户Id","nickname":"用户昵称","avatar":"用户头像","gender":"用户性别（0:未知,1:男,2:女）","phone":"用户手机号码","realName":"用户真实姓名","gather_from":"采集来源","in_time":"首次进入时间","out_time":"最后离开时间","ip":"用户ip","tuid":"第三方用户Id","third_openid":"第三方openid","live_duration":"用户观看直播时长(单位：秒)","review_duration":"用户观看回放时长(单位：秒)","dwell_duration":"用户观看回放时长(单位：秒)","live_time":"用户观看直播时长","review_time":"用户观看回放时长","dwell_time":"用户观看回放时长","recent_ip":"最新访问IP","recent_enter_time":"最近访问时间","recent_login_terminal":"最近访问终端","share_uid":"邀请人Id","share_tuid":"邀请人第三方用户Id","share_username":"邀请人名称","channel_origin":"渠道来源"}]
        /// </summary>
        [DataMember(Name = "list")]
        public List<VZLiveUserInfo> List { get; set; }

        /// <summary>
        /// Examples: "总数"
        /// </summary>
        [DataMember(Name = "total")]
        public int Total { get; set; }

        /// <summary>
        /// Examples: "当前页的分页Id，用于获取下一页数据"
        /// </summary>
        [DataMember(Name = "lastId")]
        public string LastId { get; set; }
    }

    [DataContract]
    public class VZUserInfo
    {
        /// <summary>
        /// Examples: "用户Id（密文）"
        /// </summary>
        [DataMember(Name = "uid")]
        public string Uid { get; set; }

        /// <summary>
        /// Examples: "用户Id"
        /// </summary>
        [DataMember(Name = "userId")]
        public long UserId { get; set; }

        /// <summary>
        /// Examples: "用户昵称"
        /// </summary>
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// Examples: "用户头像"
        /// </summary>
        [DataMember(Name = "avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// Examples: "用户性别（0:未知,1:男,2:女）"
        /// </summary>
        [DataMember(Name = "gender")]
        public int Gender { get; set; }

        /// <summary>
        /// Examples: "用户手机号码"
        /// </summary>
        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Examples: "用户真实姓名"
        /// </summary>
        [DataMember(Name = "realName")]
        public string RealName { get; set; }

        /// <summary>
        /// Examples: "采集来源"
        /// </summary>
        [DataMember(Name = "gather_from")]
        public string GatherFrom { get; set; }

        /// <summary>
        /// Examples: "首次进入时间"
        /// </summary>
        [DataMember(Name = "in_time")]
        public string InTime { get; set; }

        /// <summary>
        /// Examples: "最后离开时间"
        /// </summary>
        [DataMember(Name = "out_time")]
        public string OutTime { get; set; }

        /// <summary>
        /// Examples: "用户ip"
        /// </summary>
        [DataMember(Name = "ip")]
        public string Ip { get; set; }

        /// <summary>
        /// Examples: "第三方用户Id"
        /// </summary>
        [DataMember(Name = "tuid")]
        public string Tuid { get; set; }

        /// <summary>
        /// Examples: "第三方openid"
        /// </summary>
        [DataMember(Name = "third_openid")]
        public string ThirdOpenid { get; set; }

        /// <summary>
        /// Examples: "用户观看直播时长(单位：秒)"
        /// </summary>
        [DataMember(Name = "live_duration")]
        public int? LiveDuration { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长(单位：秒)"
        /// </summary>
        [DataMember(Name = "review_duration")]
        public long? ReviewDuration { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长(单位：秒)"
        /// </summary>
        [DataMember(Name = "dwell_duration")]
        public long? DwellDuration { get; set; }

        /// <summary>
        /// Examples: "用户观看直播时长"
        /// </summary>
        [DataMember(Name = "live_time")]
        public string LiveTime { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长"
        /// </summary>
        [DataMember(Name = "review_time")]
        public string ReviewTime { get; set; }

        /// <summary>
        /// Examples: "用户观看回放时长"
        /// </summary>
        [DataMember(Name = "dwell_time")]
        public string DwellTime { get; set; }

        /// <summary>
        /// Examples: "最新访问IP"
        /// </summary>
        [DataMember(Name = "recent_ip")]
        public string RecentIp { get; set; }

        /// <summary>
        /// Examples: "最近访问时间"
        /// </summary>
        [DataMember(Name = "recent_enter_time")]
        public string RecentEnterTime { get; set; }

        /// <summary>
        /// Examples: "最近访问终端"
        /// </summary>
        [DataMember(Name = "recent_login_terminal")]
        public string RecentLoginTerminal { get; set; }

        /// <summary>
        /// Examples: "邀请人Id"
        /// </summary>
        [DataMember(Name = "share_uid")]
        public string ShareUid { get; set; }

        /// <summary>
        /// Examples: "邀请人第三方用户Id"
        /// </summary>
        [DataMember(Name = "share_tuid")]
        public string ShareTuid { get; set; }

        /// <summary>
        /// Examples: "邀请人名称"
        /// </summary>
        [DataMember(Name = "share_username")]
        public string ShareUsername { get; set; }

        /// <summary>
        /// Examples: "渠道来源"
        /// </summary>
        [DataMember(Name = "channel_origin")]
        public string ChannelOrigin { get; set; }
    }
}
