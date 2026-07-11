/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.UnitTestProject
*文件名： RagFlowUnitTest
*版本号： V1.0.0.0
*唯一标识：d20bbac3-0680-4410-bb1b-28311c641ec1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/11 14:31:17
*描述：ragflow单元测试
*
*=================================================
*修改标记
*修改时间：2025/6/11 14:31:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ragflow单元测试
*
*****************************************************************************/
using LuBan.AIFlow;
using LuBan.AIFlow.Core;
using LuBan.AIFlow.Models;
using LuBan.AIFlow.Models.ChatAssistant;

namespace LuBan.UnitTestProject
{
    /// <summary>
    /// ragflow单元测试
    /// </summary>
    [TestClass]
    public class RagFlowUnitTest
    {
        /// <summary>
        /// 测试RagFlow
        /// </summary>
        [TestMethod]
        public void TestRagFlow()
        {
            var builder = new AIClientBuilder(new AIOptions()
            {
                AIType = EnumAIType.RagFlow,
                ApiKey = "LuBanFramework",
                BaseUrl = "LuBanFramework"
            });

            var client = builder.Build() as RagFlowAIClient;

            TestRagFlowAsync(client).GetAwaiter().GetResult();

        }

        async Task TestRagFlowAsync(RagFlowAIClient client)
        {
            // 创建一个新的聊天助手
            var assistant = await client.CreateChatAssistantAsync(new CreateChatAssistantRequest()
            {
                Avatar = "https://www.ragflow.cn/favicon.ico",
                Name = "RagFlow测试",
                Description = "RagFlow测试",
                DataSetIds = []
            });

            // 创建一个新的聊天会话
            var session = await client.CreateSessionAsync(assistant.Id, new AIFlow.Models.Session.CreateSessionRequest()
            {
                Name = "RagFlow测试会话"
            });

            // 与聊天助手交谈
            var talk = await client.ChatWithAssistantAsync(assistant.Id, "hello", session.Id);



        }


    }
}
