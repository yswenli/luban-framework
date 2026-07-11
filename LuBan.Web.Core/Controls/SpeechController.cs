/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Controls
*文件名： SpeechController
*版本号： V1.0.0.0
*唯一标识：782aba03-8ce8-4bd1-9a3c-e0faa61c5db6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/25 14:39:35
*描述：语音识别控制器
*
*=================================================
*修改标记
*修改时间：2023/12/25 14:39:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：语音识别控制器
*
*****************************************************************************/

using LuBan.Speech;


namespace LuBan.Web.Core.Controls;

/// <summary>
/// 语音识别控制器
/// </summary>
public class SpeechController : BaseApiController
{

    /// <summary>
    /// 语音识别控制器
    /// </summary>
    public SpeechController()
    {

    }

    /// <summary>
    /// 语音识别
    /// </summary>
    /// <returns></returns>
    [DisplayName("语音识别"), HttpPost]
    public async Task<Result> RecognitionAsync([Required] IFormFile file)
    {
        var stream = file.OpenReadStream();
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            ms.Position = 0;
            return await SpeechFactory.Create().RecognitionAsync(ms, Path.GetExtension(file.FileName).Replace(".", ""));
        }
    }
}
