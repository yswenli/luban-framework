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
*命名空间：LuBan.XTestProject
*文件名： UserInteractionUnitTest
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：用户交互服务与提问登记簿单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Tools.Interaction;

using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class UserInteractionServiceUnitTest
{
    private static IReadOnlyList<UserQuestion> TwoQuestions() =>
    [
        new("选哪个方案？",
            [new("A"), new("B")],
            "方案"),
        new("是否启用？",
            [new("是"), new("否")])
    ];

    [TestMethod]
    public void IsAvailable_FalseWithoutCallback()
    {
        var sut = new UserInteractionService(new ToolConfirmationContext());

        Assert.IsFalse(sut.IsAvailable);
    }

    [TestMethod]
    public async Task AskAsync_NoCallback_ReturnsUnavailable()
    {
        var sut = new UserInteractionService(new ToolConfirmationContext());

        var result = await sut.AskAsync(TwoQuestions(), CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Unavailable, result.Outcome);
        Assert.IsNull(result.Answers);
    }

    [TestMethod]
    public async Task AskAsync_EmptyQuestions_ReturnsUnavailable()
    {
        var context = new ToolConfirmationContext
        {
            AskQuestions = (_, _) => throw new AssertFailedException("空问题不应调用回调")
        };
        var sut = new UserInteractionService(context);

        var result = await sut.AskAsync([], CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Unavailable, result.Outcome);
    }

    [TestMethod]
    public async Task AskAsync_InvokesCallback_AndReleasesGate()
    {
        var calls = 0;
        var context = new ToolConfirmationContext
        {
            AskQuestions = (questions, _) =>
            {
                calls++;
                var answers = questions
                    .Select(q => new UserQuestionAnswer(q.Question, [q.Options[0].Label]))
                    .ToList();
                return Task.FromResult(UserQuestionResult.Answered(answers));
            }
        };
        var sut = new UserInteractionService(context);

        var first = await sut.AskAsync(TwoQuestions(), CancellationToken.None);
        var second = await sut.AskAsync(TwoQuestions(), CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Answered, first.Outcome);
        Assert.AreEqual(2, first.Answers!.Count);
        Assert.AreEqual(2, calls);
        Assert.AreEqual(UserQuestionOutcome.Answered, second.Outcome);
        Assert.AreEqual(1, context.UiGate.CurrentCount, "串行门必须在本轮交互结束后释放");
    }

    [TestMethod]
    public async Task AskAsync_ContextAlreadyCancelled_ReturnsCancelledWithoutCallback()
    {
        var called = false;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var context = new ToolConfirmationContext
        {
            CancellationToken = cts.Token,
            AskQuestions = (_, _) =>
            {
                called = true;
                return Task.FromResult(UserQuestionResult.Cancelled());
            }
        };
        var sut = new UserInteractionService(context);

        var result = await sut.AskAsync(TwoQuestions(), CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public async Task AskAsync_CallbackThrowsCancellation_ReturnsCancelled()
    {
        var context = new ToolConfirmationContext
        {
            AskQuestions = (_, _) => throw new OperationCanceledException()
        };
        var sut = new UserInteractionService(context);

        var result = await sut.AskAsync(TwoQuestions(), CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
        Assert.AreEqual(1, context.UiGate.CurrentCount);
    }

    [TestMethod]
    public async Task AskAsync_WaitsForGateHeldByConfirmation()
    {
        var invoked = false;
        var context = new ToolConfirmationContext
        {
            AskQuestions = (_, _) =>
            {
                invoked = true;
                return Task.FromResult(UserQuestionResult.Cancelled());
            }
        };
        var sut = new UserInteractionService(context);

        // 模拟工具确认正占用串行门
        Assert.IsTrue(await context.UiGate.WaitAsync(TimeSpan.FromSeconds(1)));

        var task = sut.AskAsync(TwoQuestions(), CancellationToken.None);
        await Task.Delay(80);
        Assert.IsFalse(task.IsCompleted, "串行门未释放前提问不得开始");
        Assert.IsFalse(invoked);

        context.UiGate.Release();
        var result = await task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsTrue(invoked);
        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
        Assert.AreEqual(1, context.UiGate.CurrentCount);
    }
}

[TestClass]
public class DefaultUserQuestionBrokerUnitTest
{
    private static DefaultUserQuestionBroker Build(int timeoutSeconds = 60, TimeSpan? overrideTimeout = null)
        => new(Options.Create(new LuBanAgentOptions
        {
            Confirmation = new ToolConfirmationOptions { TimeoutSeconds = timeoutSeconds }
        }), overrideTimeout);

    private static IReadOnlyList<UserQuestion> Questions() =>
    [
        new("选哪个？", [new("A"), new("B")]),
        new("是否继续？", [new("是"), new("否")])
    ];

    private static IReadOnlyList<UserQuestionAnswer> Answers() =>
    [
        new("选哪个？", ["A"]),
        new("是否继续？", ["否"])
    ];

    [TestMethod]
    public async Task Resolve_ReturnsAnswers()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        var result = await sut.WaitAsync(pending, CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Answered, result.Outcome);
        Assert.AreEqual(2, result.Answers!.Count);
        Assert.AreEqual("A", result.Answers[0].Labels[0]);
    }

    [TestMethod]
    public void Resolve_WrongSession_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("other", pending.QuestionId, Answers()));
        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
    }

    [TestMethod]
    public async Task Resolve_MissingQuestion_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId, [new("选哪个？", ["A"])]));
        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.AreEqual(UserQuestionOutcome.Answered, (await sut.WaitAsync(pending, CancellationToken.None)).Outcome);
    }

    [TestMethod]
    public async Task Resolve_UnknownQuestion_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", ["A"]),
            new("臆造的问题？", ["是"])
        ]));
        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.AreEqual(UserQuestionOutcome.Answered, (await sut.WaitAsync(pending, CancellationToken.None)).Outcome);
    }

    [TestMethod]
    public async Task Resolve_DuplicateQuestion_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", ["A"]),
            new("选哪个？", ["B"])
        ]));
        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.AreEqual(UserQuestionOutcome.Answered, (await sut.WaitAsync(pending, CancellationToken.None)).Outcome);
    }

    [TestMethod]
    public void Resolve_DuplicateRegisteredQuestions_IsRejected()
    {
        var sut = Build();
        // 绕过解析器的去重校验直接登记：正文重复时无法回链，应拒收而不是抛异常
        var pending = sut.Register("sid",
        [
            new("选哪个？", [new("A"), new("B")]),
            new("选哪个？", [new("是"), new("否")])
        ]);

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId, [new("选哪个？", ["A"])]));
    }

    [TestMethod]
    public async Task Resolve_LabelNotInOptions_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", ["MySQL"]),
            new("是否继续？", ["否"])
        ]));
        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.AreEqual(UserQuestionOutcome.Answered, (await sut.WaitAsync(pending, CancellationToken.None)).Outcome);
    }

    [TestMethod]
    public async Task Resolve_FreeTextOnly_IsAccepted()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", [], "自建 MySQL 8"),
            new("是否继续？", [], "先等等")
        ]));
        var result = await sut.WaitAsync(pending, CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Answered, result.Outcome);
        Assert.AreEqual(0, result.Answers![0].Labels.Count);
        Assert.AreEqual("自建 MySQL 8", result.Answers[0].FreeText);
    }

    [TestMethod]
    public void Resolve_EmptyAnswer_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", []),
            new("是否继续？", ["否"])
        ]));
    }

    [TestMethod]
    public void Resolve_SingleSelectMultipleLabels_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId,
        [
            new("选哪个？", ["A", "B"]),
            new("是否继续？", ["否"])
        ]));
    }

    [TestMethod]
    public async Task Resolve_MultiSelectLabels_AreAccepted()
    {
        var sut = Build();
        var pending = sut.Register("sid",
            [new("需要哪些能力？", [new("A"), new("B"), new("C")], MultiSelect: true)]);

        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, [new("需要哪些能力？", ["A", "C"])]));
        var result = await sut.WaitAsync(pending, CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Answered, result.Outcome);
        CollectionAssert.AreEqual(new[] { "A", "C" }, result.Answers![0].Labels.ToArray());
    }

    [TestMethod]
    public async Task Resolve_Twice_SecondReturnsFalse()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsTrue(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId, Answers()));
        Assert.AreEqual(UserQuestionOutcome.Answered, (await sut.WaitAsync(pending, CancellationToken.None)).Outcome);
    }

    [TestMethod]
    public async Task Cancel_ReturnsCancelled()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsTrue(sut.TryCancel("sid", pending.QuestionId));
        var result = await sut.WaitAsync(pending, CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
        Assert.IsFalse(sut.TryCancel("sid", pending.QuestionId));
    }

    [TestMethod]
    public void Cancel_UnknownOrWrongSession_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());

        Assert.IsFalse(sut.TryCancel("sid", "not-exists"));
        Assert.IsFalse(sut.TryCancel("other", pending.QuestionId));
        Assert.IsTrue(sut.TryCancel("sid", pending.QuestionId));
    }

    [TestMethod]
    public void Timeout_PrefersInteractionSetting()
    {
        var sut = new DefaultUserQuestionBroker(Options.Create(new LuBanAgentOptions
        {
            Confirmation = new ToolConfirmationOptions { TimeoutSeconds = 60 },
            Interaction = new InteractionOptions { TimeoutSeconds = 300 }
        }));

        Assert.AreEqual(300, sut.TimeoutSeconds);
    }

    [TestMethod]
    public void Timeout_FallsBackToConfirmationWhenInteractionUnset()
    {
        var sut = Build(timeoutSeconds: 90);

        Assert.AreEqual(90, sut.TimeoutSeconds);
    }

    [TestMethod]
    public async Task Timeout_ReturnsCancelled_AndEntryIsGone()
    {
        var sut = Build(overrideTimeout: TimeSpan.FromMilliseconds(80));
        var pending = sut.Register("sid", Questions());

        var result = await sut.WaitAsync(pending, CancellationToken.None);

        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
        Assert.IsTrue(result.Message?.Contains("超时") == true);
        Assert.IsFalse(sut.TryResolve("sid", pending.QuestionId, Answers()));
    }

    [TestMethod]
    public async Task Cancellation_ReturnsCancelled()
    {
        var sut = Build();
        var pending = sut.Register("sid", Questions());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await sut.WaitAsync(pending, cts.Token);

        Assert.AreEqual(UserQuestionOutcome.Cancelled, result.Outcome);
    }

    [TestMethod]
    public void TimeoutSeconds_ComesFromOptions()
    {
        Assert.AreEqual(42, Build(42).TimeoutSeconds);
    }
}

/// <summary>
/// <c>questionsJson</c> 解析与约束校验：问题正文与同题选项文案都必须唯一，
/// 否则宿主按正文/文案回链时会出现无法作答或选中串位。
/// </summary>
[TestClass]
public class UserQuestionParserUnitTest
{
    private const string ValidJson = """
        {"questions":[
          {"question":"选哪个方案？","header":"方案","options":[{"label":"A"},{"label":"B"}]},
          {"question":"是否启用？","options":[{"label":"是"},{"label":"否"}]}
        ]}
        """;

    [TestMethod]
    public void Parse_ValidJson_ReturnsQuestions()
    {
        Assert.IsTrue(UserQuestionParser.TryParse(ValidJson, out var questions, out var error), error);
        Assert.AreEqual(2, questions.Count);
        Assert.AreEqual("方案", questions[0].Header);
        Assert.AreEqual(2, questions[1].Options.Count);
    }

    [TestMethod]
    public void Parse_DuplicateQuestionText_IsRejected()
    {
        const string json = """
            [{"question":"选哪个？","options":[{"label":"A"},{"label":"B"}]},
             {"question":"选哪个？","options":[{"label":"C"},{"label":"D"}]}]
            """;

        Assert.IsFalse(UserQuestionParser.TryParse(json, out _, out var error));
        Assert.IsNotNull(error);
        StringAssert.Contains(error!, "重复");
    }

    [TestMethod]
    public void Parse_DuplicateOptionLabel_IsRejected()
    {
        const string json = """
            [{"question":"选哪个？","options":[{"label":"A"},{"label":"A"}]}]
            """;

        Assert.IsFalse(UserQuestionParser.TryParse(json, out _, out var error));
        Assert.IsNotNull(error);
        StringAssert.Contains(error!, "重复");
    }

    [TestMethod]
    public void Parse_TrimmedDuplicateQuestionText_IsRejected()
    {
        const string json = """
            [{"question":"选哪个？ ","options":[{"label":"A"},{"label":"B"}]},
             {"question":" 选哪个？","options":[{"label":"C"},{"label":"D"}]}]
            """;

        Assert.IsFalse(UserQuestionParser.TryParse(json, out _, out var error), "正文按 Trim 后比较，空格差异不算区分");
    }
}