/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Logs
*文件名： LogInfo
*版本号： V1.0.0.0
*唯一标识：1bcf4493-7645-4017-b350-371f8071eea0
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/4/8 18:10:22
*描述：
*
*=================================================
*修改标记
*修改时间：2024/4/8 18:10:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.LogCom;

[JsonObject]
public class LogInfo
{
    [JsonProperty(Order = 0)]
    public DateTime Created { get; set; } = DateTimeUtil.Now;


    [JsonProperty(Order = 0)]
    public string ServiceName { get; set; } = ConfigUtil.GetServiceName();


    [JsonProperty(Order = 1)]
    public int Level { get; set; } = 0;


    [JsonProperty(Order = 2)]
    public string Description { get; set; }

    [JsonProperty(Order = 9)]
    public object[] Params { get; set; }

    [JsonProperty(Order = 12)]
    public Exception? Exception { get; set; }

    [JsonIgnore]
    public bool EnableDebug { get; set; }
}
