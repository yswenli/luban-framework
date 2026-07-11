/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core
*文件名： GloabUsing
*版本号： V1.0.0.0
*唯一标识：32e11c24-1e0d-471b-bbcd-d279bdfb2114
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/1 15:19:58
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/1 15:19:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
global using LuBan.ApprovalFlow;
global using LuBan.CloudStorage;
global using LuBan.Common;
global using LuBan.Common.AntiReplayAttacks;
global using LuBan.Common.Consts;
global using LuBan.Common.Data;
global using LuBan.Common.IO;
global using LuBan.Common.IPToRegion;
global using LuBan.Common.Models;
global using LuBan.DI;
global using LuBan.EventBus.Extensions;
global using LuBan.LogLib;
global using LuBan.Orm;
global using LuBan.Orm.Attributes;
global using LuBan.Orm.Entities;
global using LuBan.Orm.Enums;
global using LuBan.Orm.Interfaces;
global using LuBan.Orm.Models;
global using LuBan.Redis;
global using LuBan.Redis.Core;
global using LuBan.Reporting.Extensions;
global using LuBan.Service;
global using LuBan.Service.Core;
global using LuBan.Service.Models;
global using LuBan.VideoKit;
global using LuBan.Web.Core;
global using LuBan.Web.Core.AspNetCore;
global using LuBan.Web.Core.AspNetCore.Extentions;
global using LuBan.Web.Core.AspNetCore.SignalR;
global using LuBan.Web.Core.Attributes;
global using LuBan.Web.Core.Auth;
global using LuBan.Web.Core.Database;
global using LuBan.Web.Core.Jwt;
global using LuBan.Web.Core.Models;
global using LuBan.Web.Core.Swagger;
global using LuBan.Web.Core.Swagger.Doc;
global using LuBan.Web.Core.Utils;

global using Encrypt.Library;
global using Encrypt.Library.Core.Extensions;

global using JWT.Net;

global using Mapster;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authorization.Policy;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Html;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.Extensions;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.AspNetCore.Localization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ActionConstraints;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Microsoft.AspNetCore.Mvc.ApplicationModels;
global using Microsoft.AspNetCore.Mvc.Controllers;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using Microsoft.AspNetCore.Mvc.Rendering;
global using Microsoft.AspNetCore.Mvc.Routing;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Primitives;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.Net.Http.Headers;
global using Microsoft.OpenApi;
global using Microsoft.OpenApi.Any;
global using Microsoft.OpenApi.Models;

global using Nacos.V2;
global using Nacos.V2.DependencyInjection;

global using SqlSugar;

global using Swashbuckle.AspNetCore.SwaggerGen;

global using System.Collections.Concurrent;
global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Data;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Linq.Dynamic;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Runtime.Serialization;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Text.Unicode;

global using Yitter.IdGenerator;
