/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.DI
*文件名： GlobalUsings
*版本号： V1.0.0.0
*唯一标识：3dbeb0b6-958a-4041-8f92-35a3f5b5da8f
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：项目级全局 using 声明，集中上移各代码文件的公共命名空间引用
*
*=================================================
*修改标记
*修改时间：2026/10/28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：项目级全局 using 声明，集中上移各代码文件的公共命名空间引用
*
*****************************************************************************/

global using LuBan.Common;
global using LuBan.DI;
global using LuBan.DI.Attrs;
global using LuBan.DI.Core;
global using LuBan.DI.Interfaces;
global using LuBan.DI.Models;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using System.ComponentModel;
global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;
global using System.Reflection.Emit;
global using System.Runtime.ExceptionServices;

// 由代码文件上移的命名空间（2026-10-28 规范化）
global using System.Collections.Concurrent;
