/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.ApiServices
*文件名： HomeService.cs
*版本号： V1.0.0.0
*唯一标识：6abddb3f-77af-4eca-9030-39d1183a2fa9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：HomeService 服务类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：HomeService 服务类
*
*****************************************************************************/

using LuBan.Common.Errors;

namespace WebApplication1.Services.ApiServices
{
    /// <summary>
/// HomeService 服务类
/// </summary>
    public class HomeService : BaseService<HomeService>
    {
        public async Task<Result> Hello3(int a)
        {
            return await GetResultAsync(async () =>
            {
                if (a == 1) throw new Exception("异常了");
                if (a == 2) throw FriendlyError.Ex(FrameworkErrors.Dict.DictDataDuplicate);
                return await Task.FromResult(3);

            });
        }
    }
}
