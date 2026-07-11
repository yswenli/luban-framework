using LuBan.EMailKit.Models;

namespace WebApplication1.Models.Vos;

public class EMailInput : MsgInput
{
    /// <summary>
    /// 上传文件列表
    /// </summary>
    [Required(ErrorMessage = "上传文件不能为空")]
    public List<IFormFile> Files { get; set; }
}
