/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow
*文件名： OpenApiClient
*版本号： V1.0.0.0
*唯一标识：fc3ea823-f28a-4d61-af21-4ec4816f3482
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 16:58:59
*描述：轻流接口客户端
*
*=================================================
*修改标记
*修改时间：2024/12/23 16:58:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：轻流接口客户端
*
*****************************************************************************/

namespace LuBan.Qingflow;

/// <summary>
/// 轻流接口客户端，
/// OpenApi:https://exiao.yuque.com/ixwxsb/cqfg2y/aec4bgwwblaol97b
/// 错误码：https://exiao.yuque.com/ixwxsb/cqfg2y/bh5hctvv2etapr5n
/// 结构：https://exiao.yuque.com/ixwxsb/cqfg2y/gpmeb799eb1hrqbz
/// </summary>
public class OpenApiClient : BaseSingleInstance<OpenApiClient>
{
    const string CacheKey = $"{CacheConst.KeySystem}Qingflow:AccessToken";

    private const int QingflowTokenExpiredCode = 49300;
    private const int HttpTimeoutSeconds = 180;

    /// <summary>
    /// 轻流接口客户端配置
    /// </summary>
    public QingflowOptions Options { get; private set; }

    private readonly Lazy<List<QAppInfo>> _appList;


    /// <summary>
    /// 获取访问令牌，
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/khq44vuz7xb0idan
    /// </summary>
    /// <param name="clearCache"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    AccessToken GetAccessToken(bool clearCache = false)
    {
        try
        {
            var key = $"{CacheKey}:{Options.ToJson().GetMD5Str()}";
            if (clearCache)
            {
                MemoryCache.Instance.Delete(key);
            }
            var result = MemoryCache.Instance.GetOrSet(key, (k) =>
            {
                var url = $"{Options.Domain}/accessToken?wsId={Options.WorkspaceId.UrlEncode()}&wsSecret={Options.WorkspaceSecret.UrlEncode()}";
                var tokenResult = url.HttpGetAsync<QResult<QAccessToken>>().Result;
                if (tokenResult.ErrCode != 0) throw new Exception($"获取轻流Token失败,url:{url}，code{tokenResult.ErrCode},err:{tokenResult.ErrMsg}");
                return new AccessToken()
                {
                    Token = tokenResult.Result.AccessToken,
                    ExpireTime = DateTime.Now.AddSeconds(tokenResult.Result.ExpireTime - 1200)
                };
            }) ?? throw new Exception("从轻流获取访问令牌失败");
            if (result.ExpireTime < DateTime.Now)
            {
                MemoryCache.Instance.Delete(key);
                return GetAccessToken();
            }
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error("获取轻流AccessToken失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 获取请求头
    /// </summary>
    /// <returns></returns>
    Dictionary<string, string> GetHeaders()
    {
        var token = GetAccessToken();
        var headers = new Dictionary<string, string>
        {
            { "accessToken", token.Token }
        };
        return headers;
    }

    /// <summary>
    /// 轻流接口客户端
    /// </summary>
    /// <param name="options"></param>
    public OpenApiClient(QingflowOptions? options)
    {
        if (options == null)
        {
            var config = NacosConfigUtil.Read<QingflowOptions>();
            if (config == null)
            {
                throw new Exception("请配置轻流接口客户端配置文件");
            }
            else
            {
                Options = config;
            }
        }
        else
        {
            Options = options;
        }
        _appList = new Lazy<List<QAppInfo>>(() =>
        {
            var result = GetAppListAsync().GetAwaiter().GetResult();
            return result?.Result?.AppList ?? new List<QAppInfo>();
        });
    }

    /// <summary>
    /// 轻流接口客户端
    /// </summary>
    public OpenApiClient() : this(null)
    {

    }

    #region 轻流应用

    /// <summary>
    /// 获取轻流应用列表，
    /// 仅超级管理员可调用
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wqwm2f6byxobl55e
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// 处理轻流接口返回的错误码：Token 过期时刷新并允许重试一次，其余错误码直接抛异常。
    /// 用于消除各 API 方法中重复的错误处理逻辑。
    /// </summary>
    /// <param name="errCode">接口返回的错误码</param>
    /// <param name="errMsg">接口返回的错误信息</param>
    /// <param name="url">请求的 url，用于异常信息</param>
    /// <param name="isRetry">是否已经重试过</param>
    private static void HandleErrCode(int errCode, string? errMsg, string url, bool isRetry)
    {
        if (errCode == 0) return;

        if (errCode == QingflowTokenExpiredCode)
        {
            if (isRetry)
                throw new Exception($"调用轻流接口失败，Token刷新后仍无效，url:{url}");
            return;
        }

        throw new Exception($"调用轻流接口失败,url:{url}，{errCode}:{errMsg}");
    }

    public async Task<QResult<AppInfoList>?> GetAppListAsync()
    {
        var dic = GetHeaders();
        try
        {
            dic["accessToken"] = Options.SuperAdminToken;
            var url = $"{Options.Domain}/app";
            var result = await url.HttpGetAsync<QResult<AppInfoList>>(dic, HttpTimeoutSeconds);
            if (result != null && result.ErrCode == 0 && result.Result != null && result.Result.AppList != null)
            {
                Options.QingflowApps.Clear();
                foreach (var item in result.Result.AppList)
                {
                    Options.QingflowApps.Add(new QingflowAppOptions()
                    {
                        AppId = item.AppKey,
                        AppName = item.AppName
                    });
                }
                return result;
            }
            else
                throw new Exception($"获取应用列表失败，Code:{result?.ErrCode ?? 9999}");

        }
        catch (Exception ex)
        {
            Logger.Error("获取轻流应用列表出现异常", ex);
            throw;
        }
    }

    #endregion

    #region 应用数据

    /// <summary>
    /// 获得应用数据列表,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/aynfx2uqhkarv33c,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wz17n68r0ayoa5sk?singleDoc#%20%E3%80%8A%E8%8E%B7%E5%8F%96%E5%BA%94%E7%94%A8%E6%95%B0%E6%8D%AE%E6%8E%A5%E5%8F%A3%E8%AF%A6%E8%A7%A3%E3%80%8B
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="pageNum"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<AppData>>> GetAppDataListAsync(GetAppDataListInput input, bool isRetry = false)
    {
        QResult<QPagedList<AppData>>? result = null;
        do
        {
            var url = $"{Options.Domain}/app/{input.AppId}/apply/filter";
            var data = await url.HttpPostAsync<QResult<QPagedList<AppData>>>(input, GetHeaders(), HttpTimeoutSeconds);
            if (data == null)
            {
                throw new Exception($"调用轻流接口失败，url:{url},{data?.ErrCode}:{data?.ErrMsg}");
            }
            else
            {
                if (data.ErrCode != 0)
                {
                    HandleErrCode(data.ErrCode, data.ErrMsg, url, isRetry);
                    if (data.ErrCode == QingflowTokenExpiredCode)
                    {
                        GetAccessToken(true);
                        return await GetAppDataListAsync(input, isRetry: true);
                    }
                }
                if (data.Result != null && data.Result.Result != null)
                {
                    input.PageIndex += 1;
                    if (result == null)
                    {
                        result = data;
                        if (result.Result.Result.Count >= data.Result.ResultAmount) break;
                    }
                    else
                    {
                        result.Result.Result.AddRange(data.Result.Result);
                        if (result.Result.Result.Count >= data.Result.ResultAmount) break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        while (input.IsAll);
        return result ?? new QResult<QPagedList<AppData>>();
    }

    /// <summary>
    /// 获得应用数据列表详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<TitleValueCollection>?> GetTitleValueCollectionListAsync(GetAppDataListInput input)
    {
        var appData = await GetAppDataListAsync(input);
        if (appData == null || appData.ErrCode != 0 || appData.Result == null || appData.Result.Result == null || appData.Result.Result.Count < 1)
        {
            return null;
        }
        List<TitleValueCollection> result = [];
        foreach (var item in appData.Result.Result)
        {
            var tvc = item.FieldInfos.Select(q => new TitleValue(q.QueTitle, q.Values?.FirstOrDefault()?.Value ?? "")).ToList().ConvertToTitleValueCollection();
            result.Add(tvc);
        }
        return result;
    }

    /// <summary>
    /// 根据应用数据ID获取应用数据详细
    /// </summary>
    /// <param name="appDataId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<QResult<AppData>> GetAppDataByIdAsync(long appDataId, bool isRetry = false)
    {
        var url = $"{Options.Domain}/apply/{appDataId}";
        var result = await url.HttpGetAsync<QResult<AppData>>(GetHeaders(), HttpTimeoutSeconds);
        if (result == null)
        {
            throw new Exception($"调用轻流接口失败,url:{url}，{result?.ErrCode}:{result?.ErrMsg}");
        }
        else
        {
            HandleErrCode(result.ErrCode, result.ErrMsg, url, isRetry);
            if (result.ErrCode == QingflowTokenExpiredCode)
            {
                GetAccessToken(true);
                return await GetAppDataByIdAsync(appDataId, isRetry: true);
            }
            return result;
        }
    }

    /// <summary>
    /// 根据应用数据ID获取应用数据详细
    /// </summary>
    /// <param name="appDataId"></param>
    /// <returns></returns>
    public async Task<TitleValueCollection?> GetTitleValueCollectionByIdAsync(long appDataId)
    {
        var appData = await GetAppDataByIdAsync(appDataId);
        if (appData == null || appData.ErrCode != 0 || appData.Result == null)
        {
            return null;
        }
        return appData.Result.FieldInfos.Select(q => new TitleValue(q.QueTitle, q.Values?.FirstOrDefault()?.Value ?? "")).ToList().ConvertToTitleValueCollection();
    }

    /// <summary>
    /// 根据应用数据ID获取应用数据详细
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="appDataId"></param>
    /// <returns></returns>
    public async Task<T?> GetModelByIdAsync<T>(long appDataId)
        where T : class, IAppData, new()
    {
        var data = await GetAppDataByIdAsync(appDataId);
        if (data.Result == null) return default;
        var t = data.Result.ConvertTo<T>();
        if (t == null) return default;
        t.AppDataId = data.Result.AppDataId;
        return t;
    }

    /// <summary>
    /// 获得应用数据列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<T>>?> GetPagedModelListAsync<T>(GetAppDataListInput input)
         where T : class, IAppData, new()
    {
        var data = await GetAppDataListAsync(input);
        QResult<QPagedList<T>> result = new()
        {
            ErrCode = data.ErrCode,
            ErrMsg = data.ErrMsg
        };
        if (data.Result != null && data.Result.Result != null && data.Result.Result.Count > 0)
        {
            result.Result = new QPagedList<T>()
            {
                PageAmount = data.Result.PageAmount,
                PageNum = data.Result.PageNum,
                PageSize = data.Result.PageSize,
                ResultAmount = data.Result.ResultAmount
            };
            result.Result.Result = data.Result.Result.ConvertToList<T>();
        }
        ;
        return result;

    }

    /// <summary>
    /// 获取按日期范围的应用数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<AppData>>> GetLastestAppDataAsync(GetLastestAppDataInput input)
    {
        return await OpenApiClient.Instance.GetAppDataListAsync(new Qingflow.Models.GetAppDataListInput()
        {
            AppId = input.AppId,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            Queries = new List<AppDataQuery>()
             {
                 new AppDataQuery()
                 {
                      QueId = input.DateQueId,
                      MinValue = input.FromDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                      MaxValue = input.ToDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                 }
             },
            IsAll = input.IsAll
        });
    }

    /// <summary>
    /// 获取按日期范围的应用数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<T>>?> GetLastestModelAsync<T>(GetLastestAppDataInput input)
        where T : class, IAppData, new()
    {
        return await OpenApiClient.Instance.GetPagedModelListAsync<T>(new Qingflow.Models.GetAppDataListInput()
        {
            AppId = input.AppId,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            Queries = new List<AppDataQuery>()
            {
                 new AppDataQuery()
                 {
                      QueId = input.DateQueId,
                      MinValue = input.FromDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                      MaxValue = input.ToDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                 }
            },
            IsAll = input.IsAll
        });
    }

    /// <summary>
    /// 删除应用数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> DeleteAppDataAsync(GetAppDataListInput input, bool isRetry = false)
    {
        var url = $"{Options.Domain}/app/{input.AppId}/apply";
        var result = await url.HttpDeleteAsync<QResult<RequestInfo>>(input, GetHeaders(), HttpTimeoutSeconds);
        if (result == null)
        {
            throw new Exception($"调用轻流接口失败，url:{url},{result?.ErrCode}:{result?.ErrMsg}");
        }
        else
        {
            if (result.ErrCode != 0)
            {
                HandleErrCode(result.ErrCode, result.ErrMsg, url, isRetry);
                if (result.ErrCode == QingflowTokenExpiredCode)
                {
                    GetAccessToken(true);
                    return await DeleteAppDataAsync(input, isRetry: true);
                }
            }
            return result.Result.RequestId;
        }
    }

    /// <summary>
    /// 删除应用数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<string> DeleteAppDataAsync(DeleteAppDataInput input)
    {
        var request = new GetAppDataListInput()
        {
            AppId = input.AppId,
            PageIndex = 1,
            PageSize = 1,
            AppDataIds = [input.AppDataId]
        };
        return await DeleteAppDataAsync(request);
    }

    /// <summary>
    /// 从轻流中获取某数据相关的表格数据列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<List<T>> GetModelListForTabletAsync<T>(GetTableDataListInput input, Func<FiledInfo, bool>? where = null) where T : class, new()
    {
        List<T> result = [];
        var data = await GetAppDataListAsync(input);
        if (data?.Result?.Result == null) return result;
        foreach (var item in data.Result.Result)
        {
            if (where != null)
            {
                if (!item.FieldInfos.Any(where)) continue;
            }
            var tableValues = item.FieldInfos.Where(q => q.QueTitle == input.QueTitleOfTable).Select(q => q.TableValues).ToList();
            if (tableValues == null || tableValues.Count < 1) continue;

            foreach (var tableValue in tableValues)
            {
                if (tableValue == null || tableValue.Count < 1) continue;
                var first = tableValue.FirstOrDefault();
                if (first == null) continue;
                var t = first.ConvertTo<T>();
                if (t != null)
                {
                    result.Add(t);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 从轻流中获取数据和数据相关的表格数据列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<Dictionary<T1, List<T2>>> GetModelListWithTableDataAsync<T1, T2>(GetTableDataListInput input, Func<FiledInfo, bool>? where = null)
        where T1 : class, new()
        where T2 : class, new()
    {
        Dictionary<T1, List<T2>> result = [];
        var data = await GetAppDataListAsync(input);
        if (data?.Result?.Result == null) return result;
        foreach (var item in data.Result.Result)
        {
            if (where != null)
            {
                if (!item.FieldInfos.Any(where)) continue;
            }

            var collection = item.FieldInfos.Select(q => new TitleValue(q.QueTitle, q.Values?.FirstOrDefault()?.Value ?? "")).ToList().ConvertToTitleValueCollection();
            if (collection == null) continue;

            var dataResult = collection.ConvertTo<T1>();
            if (dataResult == null) continue;

            List<T2> tableResult = [];
            result.TryAdd(dataResult, tableResult);

            var tableValues = item.FieldInfos.Where(q => q.QueTitle == input.QueTitleOfTable).Select(q => q.TableValues).ToList();
            if (tableValues == null || tableValues.Count < 1) continue;

            foreach (var tableValue in tableValues)
            {
                if (tableValue == null || tableValue.Count < 1) continue;
                var first = tableValue.FirstOrDefault();
                if (first == null) continue;
                var t = first.ConvertTo<T2>();
                if (t != null)
                {
                    tableResult.Add(t);
                }
            }
            result[dataResult] = tableResult;
        }
        return result;
    }

    /// <summary>
    /// 获取患者注册信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<AppData>>> GetPatientRegistInfosAsync(GetPatientRegistInfoInput input)
    {
        var data = new GetAppDataListInput
        {
            AppId = input.AppId,
            PageIndex = 1,
            PageSize = 100,
            Queries = new List<AppDataQuery>()
            {
                new AppDataQuery()
                {
                    QueId=1,
                    SearchKey=input.Applicant
                }
            }
        };
        return await GetAppDataListAsync(data);
    }

    /// <summary>
    /// 获取患者注册信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<T>>?> GetPatientRegistInfosAsync<T>(GetPatientRegistInfoInput input)
        where T : class, IAppData, new()
    {
        var data = new GetAppDataListInput
        {
            AppId = input.AppId,
            PageIndex = 1,
            PageSize = 100,
            Queries = new List<AppDataQuery>()
            {
                new AppDataQuery()
                {
                    QueId=1,
                    SearchKey=input.Applicant
                }
            }
        };
        return await GetPagedModelListAsync<T>(data);
    }

    /// <summary>
    /// 根据患者身份证获取患者相关应用的信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<AppData>>> GetAppDataByIdCardAsync(GetAppDataByIdCardInput input)
    {
        var dic = new Dictionary<string, string>();
        var data = new GetAppDataListInput
        {
            AppId = input.AppId,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize,
            //QueryKey = input.PatientIdCard
            Queries = new List<AppDataQuery>()
        };
        data.Queries.Add(new AppDataQuery()
        {
            QueId = input.IdCardQueId,
            SearchKey = input.PatientIdCard
        });
        return await GetAppDataListAsync(data);
    }

    /// <summary>
    /// 根据患者身份证获取应用数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<T>?> GetModelByIdCardAsync<T>(GetAppDataByIdCardInput input)
        where T : class, IAppData, new()
    {
        var data = new GetAppDataByIdCardInput
        {
            AppId = input.AppId,
            PatientIdCard = input.PatientIdCard,
            IdCardQueId = input.IdCardQueId,
            PageIndex = input.PageIndex,
            PageSize = input.PageSize
        };
        var resultData = await GetAppDataByIdCardAsync(data);
        if (resultData.Result != null && resultData.Result.Result != null && resultData.Result.Result.Any())
        {
            return resultData.Result.Result.ConvertToList<T>();
        }
        return default;
    }

    #endregion

    #region 流程数据

    /// <summary>
    /// 获取某条数据的流程的列表,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/hp9ggumdrxdmk541
    /// </summary>
    /// <param name="dataId"></param>
    /// <returns></returns>
    public async Task<QResult<AuditFlowInfo>> GetAuditFlowListAsync(string dataId, bool isRetry = false)
    {
        var url = $"{Options.Domain}/apply/{dataId}/auditRecord";
        var result = await url.HttpGetAsync<QResult<AuditFlowInfo>>(GetHeaders(), HttpTimeoutSeconds);
        if (result == null)
        {
            throw new Exception($"调用轻流接口失败，url:{url},{result?.ErrCode}:{result?.ErrMsg}");
        }
        else
        {
            if (result.ErrCode != 0)
            {
                if (result.ErrCode == QingflowTokenExpiredCode)
                HandleErrCode(result.ErrCode, result.ErrMsg, url, isRetry);
                if (result.ErrCode == QingflowTokenExpiredCode)
                {
                    GetAccessToken(true);
                    return await GetAuditFlowListAsync(dataId, isRetry: true);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 获取某条数据的流程的最新状态
    /// </summary>
    /// <param name="dataId"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public async Task<LatestAuditFlow?> GetLatestAuditFlowAsync(string dataId, string nodeName = "")
    {
        var data = await GetAuditFlowListAsync(dataId);
        if (data.Result != null && data.Result.ApplyStatus != EnumFlowType.Default)
        {
            var result = new LatestAuditFlow()
            {
                ApplyStatus = data.Result.ApplyStatus
            };
            if (data.Result.AuditRecords != null && data.Result.AuditRecords.Count > 0)
            {
                AuditRecord? applyRecord = null;
                if (data.Result.AuditRecords.Count == 1)
                {
                    applyRecord = data.Result.AuditRecords[0];
                }
                else
                {
                    if (nodeName.IsNullOrEmpty())
                    {
                        var last = data.Result.AuditRecords.LastOrDefault();
                        if (last != null)
                            applyRecord = last;
                    }

                    else
                    {
                        var last = data.Result.AuditRecords.Where(x => x.AuditNodeName.IsNotNullOrEmpty() && x.AuditNodeName.Contains(nodeName)).OrderByDescending(x => x.AuditTime).LastOrDefault();
                        if (last != null)
                            applyRecord = last;
                    }
                }
                if (applyRecord == null) return null;
                result.AuditNodeId = applyRecord.AuditNodeId ?? 0;
                result.AuditNodeName = applyRecord.AuditNodeName ?? "";
                if (applyRecord.AuditTime.HasValue)
                    result.AuditTime = applyRecord.AuditTime.Value.FromUnixTimeStamp();
                return result;
            }
        }
        return default;
    }

    #endregion

    #region 轻流用户

    /// <summary>
    /// 获取用户信息，
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/firpyap0k0fgyk7w
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<QResult<QUser>> GetUserAsync(string userId, bool isRetry = false)
    {
        var url = $"{Options.Domain}/user/{userId}";
        var result = await url.HttpGetAsync<QResult<QUser>>(GetHeaders(), HttpTimeoutSeconds);
        if (result == null)
        {
            throw new Exception($"调用轻流接口失败,url:{url}，{result?.ErrCode}:{result?.ErrMsg}");
        }
        else
        {
            if (result.ErrCode != 0)
            {
                HandleErrCode(result.ErrCode, result.ErrMsg, url, isRetry);
                if (result.ErrCode == QingflowTokenExpiredCode)
                {
                    GetAccessToken(true);
                    return await GetUserAsync(userId, isRetry: true);
                }
            }
            return result;
        }
    }

    #endregion

    #region 报表

    /// <summary>
    /// 获取报表数据,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wyokz56ootk13hpm
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<QResult<QPagedList<AppData>>> GetChartDataAsync(GetChartDataListInput input, bool isRetry = false)
    {
        var url = $"{Options.Domain}/chart/{input.ChartKey}/apply/filter";
        var result = await url.HttpPostAsync<QResult<QPagedList<AppData>>>(input, GetHeaders(), HttpTimeoutSeconds);
        if (result == null)
        {
            throw new Exception($"调用轻流接口失败,url:{url}，{result?.ErrCode}:{result?.ErrMsg}");
        }
        else
        {
            if (result.ErrCode != 0)
            {
                HandleErrCode(result.ErrCode, result.ErrMsg, url, isRetry);
                if (result.ErrCode == QingflowTokenExpiredCode)
                {
                    GetAccessToken(true);
                    return await GetChartDataAsync(input, isRetry: true);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 获取报表数据,
    /// https://exiao.yuque.com/ixwxsb/cqfg2y/wyokz56ootk13hpm
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<List<AppData>> GetChartDataByDatetimeAsync(ChartDataByDatetimeInput input)
    {
        var list = new List<AppData>();
        do
        {
            var data = await GetChartDataAsync(new GetChartDataListInput()
            {
                ChartKey = input.ChartKey,
                Filter = new ChartDataListInputFilter()
                {
                    PageNum = input.PageNum,
                    PageSize = input.PageSize,
                    Type = 13,
                    QueryKey = input.QueryKey ?? ""
                },
                AccurateQuery = new List<ChartDataListInputAccurateQuery>()
                    {
                        new ChartDataListInputAccurateQuery()
                        {
                            QueId = input.QueId,
                            SearchKey = $"{input.StartTime.ToString("yyyy-MM-dd")}~{input.EndTime.ToString("yyyy-MM-dd")}"
                        }
                    }
            });
            if (data == null) break;
            if (data.ErrCode > 0)
            {
                throw new Exception($"调用轻流接口失败，{data.ErrCode}:{data.ErrMsg}");
            }
            if (data.Result != null && data.Result.Result != null && data.Result.Result.Count > 0)
            {
                list.AddRange(data.Result.Result);
                if (list.Count >= data.Result.ResultAmount) break;
                input.PageNum += 1;
            }
            else
            {
                break;
            }
        }
        while (input.IsAll);

        return list;
    }
    #endregion
}
