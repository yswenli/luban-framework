/****************************************************************************
*Copyright (c) 2023 yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.EventBus
*文件名： GlobalUsing
*版本号： V2.0.0.0
*唯一标识：c07c43a6-23fe-4b50-9052-d6bb860e15e4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：全局 using 引用声明
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：重构为使用新的命名空间
*
*****************************************************************************/
global using System.Collections.Concurrent;
global using System.Threading.Channels;

global using LuBan.Common;
global using LuBan.Common.EventBus;
global using LuBan.EventBus.Core;
global using LuBan.EventBus.Models;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
