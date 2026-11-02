/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： GlobalUsings
*版本号： V1.0.0.0
*唯一标识：af627f18-3e46-44d4-9da6-6eb1b5bf4a05
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:04:01
*描述：项目级全局 using 声明，集中管理常用命名空间。
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:04:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：项目级全局 using 声明，集中管理常用命名空间。
*
*****************************************************************************/
global using LuBan.CloudStorage.Aliyun;
global using LuBan.CloudStorage.MinIO;
global using LuBan.CloudStorage.MsAzure;
global using LuBan.Common;
global using LuBan.Common.IO;
global using LuBan.DI;
global using LuBan.Orm;
global using LuBan.Orm.Entities;
global using LuBan.Threading;

global using Encrypt.Library;

global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Security.Cryptography;
global using System.Text.RegularExpressions;

global using Yitter.IdGenerator;

// 由代码文件上移的命名空间（2026-10-28 规范化）
global using Aliyun.OSS;
global using Aliyun.OSS.Common;
global using Azure;
global using Azure.Core.Pipeline;
global using Azure.Storage.Blobs;
global using Azure.Storage.Sas;
global using LuBan.Common.Data;
global using LuBan.Common.Errors;
global using LuBan.VideoKit;
global using Minio;
global using Minio.DataModel.Args;
global using Minio.DataModel.Encryption;
global using System.Buffers;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Reflection;
