/****************************************************************************
*Copyright (c) 2023 RiverLand All Rights Reserved.
*CLR版本： .net8
*机器名称：WALLE
*公司名称：Walle
*命名空间：Services.ApiServices
*文件名： SysConfigService
*版本号： V1.0.0.0
*唯一标识：23d7ffb3-85e2-4a87-9d1a-176513583827
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/7 10:54:23
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/7 10:54:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统参数配置服务
*
*****************************************************************************/
using WebApplication1.Models.Entities;

namespace Services.ApiServices;

/// <summary>
/// 系统参数配置服务
/// </summary>
public class ConfigService : BaseService<ConfigService>
{
    private DbRepository<DbConfig> _sysConfigRep => new();


    /// <summary>
    /// 获取参数配置分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<DbConfig>> GetPageListAsync(ConfigPageInput input)
    {
        return await _sysConfigRep
            .WhereIF(input.Name.IsNotNullOrEmpty(), u => u.Name.Contains(input.Name))
            .WhereIF(input.Code.IsNotNullOrEmpty(), u => u.Code != null && u.Code.Contains(input.Code ?? ""))
            .WhereIF(input.GroupCode.IsNotNullOrEmpty(), u => u.GroupCode != null && u.GroupCode.Equals(input.GroupCode))
            .ToPagedListAsync(input);
    }



    /// <summary>
    /// 获取参数配置列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbConfig>> GetListAsync()
    {
        return await _sysConfigRep.GetListAsync();
    }

    /// <summary>
    /// 增加参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> AddConfigAsync(AddConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => u.Name == input.Name || u.Code == input.Code);
        if (isExist)
            throw FriendlyError.Ex(EnumErrorCode.D9000);

        return await _sysConfigRep.InsertAsync(input.Adapt<DbConfig>());
    }


    /// <summary>
    /// 更新参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateConfigAsync(UpdateConfigInput input)
    {
        var isExist = await _sysConfigRep.IsAnyAsync(u => (u.Name == input.Name || u.Code == input.Code) && u.Id != input.Id);
        if (isExist)
            throw FriendlyError.Ex(EnumErrorCode.db1000);

        var config = input.Adapt<DbConfig>();
        if (config == null || config.Code.IsNullOrEmpty()) return false;
        await _sysConfigRep.AsUpdateable(config).IgnoreColumns(true).ExecuteCommandAsync();
        return CacheService.Instance.Remove(config.Code) > 0;
    }


    /// <summary>
    /// 删除参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteConfigAsync(DeleteConfigInput input)
    {
        var config = await _sysConfigRep.FirstAsync(u => u.Id == input.Id);
        if (config == null || config.SysFlag == EnumYesNo.Y) // 禁止删除系统参数
            throw FriendlyError.Ex(EnumErrorCode.D9001);
        if (await _sysConfigRep.DeleteAsync(config))
        {
            if (config.Code.IsNotNullOrEmpty())
                return CacheService.Instance.Remove(config.Code) > 0;
        }
        return false;
    }


    /// <summary>
    /// 批量删除参数配置
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<bool> BatchDeleteConfigAsync(List<long> ids)
    {
        foreach (var id in ids)
        {
            var config = await _sysConfigRep.FirstAsync(u => u.Id == id);
            if (config.SysFlag == EnumYesNo.Y) // 禁止删除系统参数
                continue;
            await _sysConfigRep.DeleteAsync(config);
            if (config.Code.IsNotNullOrEmpty())
                CacheService.Instance.Remove(config.Code);
        }
        return true;
    }

    /// <summary>
    /// 获取参数配置详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<DbConfig> GetDetailAsync([FromQuery] ConfigInput input)
    {
        return await _sysConfigRep.FirstAsync(u => u.Id == input.Id);
    }


    /// <summary>
    /// 添加或更新配置项
    /// </summary>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> SetConfigValueAsync(string code, string value)
    {
        //设置前先删除
        _ = await _sysConfigRep.DeleteAsync(u => u.Code == code);
        return await _sysConfigRep.InsertAsync(new DbConfig()
        {
            Code = code,
            GroupCode = "Default",
            Value = value,
            Name = "快捷配置项",
            SysFlag = EnumYesNo.N,
            Remark = "配置项",
            OrderNo = 0
        });
    }


    /// <summary>
    /// 添加或更新配置项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="code"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public async Task<bool> SetValueAsync<T>(string code, T t) where T : class, new()
    {
        if (t == null) return false;
        var json = SerializeUtil.Serialize(t);
        if (json.IsNotNullOrEmpty())
            return await SetConfigValueAsync(code, json);
        return false;
    }


    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task<string?> GetConfigValueAsync(string code, int timeout = 0)
    {
        if (timeout > 0)
        {
            var datetime = DateTime.Now.AddSeconds(-timeout);
            var config = await _sysConfigRep.FirstAsync(u => u.Code == code && u.IsDelete == false && u.CreateTime > datetime);
            if (config == null || config.Id < 1)
            {
                //过期的删除
                await _sysConfigRep.DeleteAsync(u => u.Code == code);
                return string.Empty;
            }
            return config.Value;
        }
        else
        {
            var config = await _sysConfigRep.FirstAsync(u => u.Code == code && u.IsDelete == false);
            if (config == null || config.Id < 1)
            {
                //逻辑删的也删除
                await _sysConfigRep.DeleteAsync(u => u.Code == code);
                return string.Empty;
            }
            return config.Value;
        }
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>

    public string? GetConfigValue(string code, int timeout = 0)
    {
        if (timeout > 0)
        {
            var datetime = DateTime.Now.AddSeconds(-timeout);
            var config = _sysConfigRep.First(u => u.Code == code && u.IsDelete == false && u.CreateTime > datetime);
            if (config == null || config.Id < 1)
            {
                //过期的删除
                _sysConfigRep.Delete(u => u.Code == code);
                return string.Empty;
            }
            return config.Value;
        }
        else
        {
            var config = _sysConfigRep.First(u => u.Code == code && u.IsDelete == false);
            if (config == null || config.Id < 1)
            {
                //逻辑删的也删除
                _sysConfigRep.Delete(u => u.Code == code);
                return string.Empty;
            }
            return config.Value;
        }
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <typeparam name="T">基础类型</typeparam>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<T?> GetConfigValueAsync<T>(string code) where T : struct
    {
        var value = CacheService.Instance.Get<string>(code);
        if (string.IsNullOrEmpty(value))
        {
            var config = await _sysConfigRep.FirstAsync(u => u.Code == code);
            value = config != null ? config.Value : default;
            CacheService.Instance.Set(code, value);
        }
        if (string.IsNullOrWhiteSpace(value)) return default;
        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<T?> GetValueAsync<T>(string code) where T : class, new()
    {
        var json = await GetConfigValueAsync(code);
        if (json.IsNotNullOrEmpty())
        {
            return SerializeUtil.Deserialize<T>(json);
        }
        return default;
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="code"></param>
    /// <returns></returns>
    public T? GetValue<T>(string code) where T : class, new()
    {
        var json = GetConfigValue(code);
        if (json.IsNotNullOrEmpty())
        {
            return SerializeUtil.Deserialize<T>(json);
        }
        return default;
    }


    /// <summary>
    /// 获取分组列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<string?>> GetGroupListAsync()
    {
        return await _sysConfigRep.Where(q => q.IsDelete == false).GroupBy(u => u.GroupCode).Select(u => u.GroupCode).ToListAsync();
    }


    /// <summary>
    /// 获取 Token 过期时间（秒）
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetTokenExpireAsync()
    {
        var tokenExpireStr = await GetConfigValueAsync(CommonConst.SysTokenExpireCode);
        _ = int.TryParse(tokenExpireStr, out var tokenExpire);
        return tokenExpire == 0 ? 86400 : tokenExpire;
    }


    /// <summary>
    /// 获取 RefreshToken 过期时间（分钟）
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetRefreshTokenExpireAsync()
    {
        var refreshTokenExpireStr = await GetConfigValueAsync(CommonConst.SysRefreshTokenExpireCode);
        _ = int.TryParse(refreshTokenExpireStr, out var refreshTokenExpire);
        return refreshTokenExpire == 0 ? 525600 : refreshTokenExpire;
    }


    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> SaveAsync(AddConfigInput input)
    {
        var result = await _sysConfigRep.FirstAsync(q => q.IsDelete == false && q.Code == input.Code);
        if (result == null)
        {
            return await _sysConfigRep.InsertAsync(input.Adapt<DbConfig>());
        }
        else
        {
            result.Name = input.Name;
            result.Value = input.Value;
            result.Remark = input.Remark;
            result.GroupCode = input.GroupCode;
            return await _sysConfigRep.UpdateAsync((q) => result, q => q.Code == input.Code);
        }
    }

    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <param name="code"></param>
    /// <param name="set"></param>
    /// <returns></returns>
    public async Task<DbConfig> GetOrSetAsync(string code, Func<AddConfigInput> set, TimeSpan timeOut)
    {
        var result = await _sysConfigRep.FirstAsync(q => q.IsDelete == false && q.Code == code);
        if (result == null)
        {
            var entity = set.Invoke().Adapt<DbConfig>();
            await _sysConfigRep.InsertAsync(entity);
            result = await _sysConfigRep.FirstAsync(q => q.IsDelete == false && q.Code == code);
        }
        else
        {
            if (result.CreateTime.Add(timeOut) < DateTime.Now)
            {
                await _sysConfigRep.DeleteAsync(result);
                return await GetOrSetAsync(code, set, timeOut);
            }
        }
        return result;
    }


}
