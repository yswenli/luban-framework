/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow
*文件名： GlobalUsing
*版本号： V1.0.0.0
*唯一标识：f6669d1c-c1ab-4ef7-82a8-0e716d6e5dcf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:53:17
*描述：全局 using 引用声明，统一管理审批流项目的命名空间引用。
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:53:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：全局 using 引用声明，统一管理审批流项目的命名空间引用。
*
*****************************************************************************/
global using LuBan.ApprovalFlow.Abstractions;
global using LuBan.ApprovalFlow.Consts;
global using LuBan.ApprovalFlow.Core;
global using LuBan.ApprovalFlow.Core.Handlers;
global using LuBan.ApprovalFlow.Libs;
global using LuBan.ApprovalFlow.Models;
global using LuBan.Common;
global using LuBan.Common.EventBus;
global using LuBan.EventBus;
global using LuBan.EventBus.Core;
global using LuBan.EventBus.Models;
global using LuBan.Orm.Models;
global using LuBan.Threading;
global using LuBan.Threading.Models;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using SqlSugar;

global using System;
global using System.Collections.Concurrent;
global using System.ComponentModel.DataAnnotations;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.Json.Serialization;
