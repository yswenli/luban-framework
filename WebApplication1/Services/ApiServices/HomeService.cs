namespace WebApplication1.Services.ApiServices
{
    public class HomeService : BaseService<HomeService>
    {
        public async Task<Result> Hello3(int a)
        {
            return await GetResultAsync(async () =>
            {
                if (a == 1) throw new Exception("异常了");
                if (a == 2) throw FriendlyError.Ex(EnumErrorCode.D3003);
                return await Task.FromResult(3);

            });
        }
    }
}
