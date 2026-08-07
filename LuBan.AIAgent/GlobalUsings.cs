/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： GlobalUsings
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：GlobalUsings
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：GlobalUsings
*
*****************************************************************************/
global using LuBan.AIAgent.Abstractions;
global using LuBan.AIAgent.Configuration;
global using LuBan.AIAgent.Core;
global using LuBan.AIAgent.Infrastructure;
global using LuBan.AIAgent.LocalMemory;
global using LuBan.AIAgent.MCP;
global using LuBan.AIAgent.MCP.BuiltIn;
global using LuBan.AIAgent.Plugins;
global using LuBan.AIAgent.Rules;
global using LuBan.AIAgent.Rules.BuiltIn;
global using LuBan.AIAgent.Sessions;
global using LuBan.AIAgent.Skills;
global using LuBan.AIAgent.Skills.BuiltIn;
global using LuBan.AIAgent.Utils.Text;
global using LuBan.Common;
global using LuBan.DI;

global using Microsoft.Agents.AI;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Playwright;

global using System.Collections.Concurrent;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
