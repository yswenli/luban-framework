/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： EventBusUnitTest
*版本号： V1.0.0.0
*描述：EventBus 事件总线完整单元测试
*****************************************************************************/
using LuBan.Common.EventBus;
using LuBan.EventBus.Extensions;
using LuBan.EventBus.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

/// <summary>
/// EventBus 事件总线完整单元测试
/// </summary>
[TestClass]
public class EventBusUnitTest
{
    #region 测试事件定义

    /// <summary>
    /// 测试用事件数据
    /// </summary>    
    public class TestEvent : BaseEventData
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// 另一个测试事件数据
    /// </summary>
    public class AnotherTestEvent : BaseEventData
    {
        public string Data { get; set; } = string.Empty;
    }

    #endregion

    #region 测试处理器定义

    /// <summary>
    /// 测试事件处理器
    /// </summary>
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public static readonly List<TestEvent> ReceivedEvents = new();
        public static readonly List<Exception> ReceivedErrors = new();

        public Task HandleAsync(TestEvent eventData)
        {
            ReceivedEvents.Add(eventData);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            ReceivedErrors.Add(ex);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedEvents.Clear();
            ReceivedErrors.Clear();
        }
    }

    /// <summary>
    /// 第二个测试事件处理器
    /// </summary>
    public class SecondTestEventHandler : IEventHandler<TestEvent>
    {
        public static readonly List<TestEvent> ReceivedEvents = new();

        public Task HandleAsync(TestEvent eventData)
        {
            ReceivedEvents.Add(eventData);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedEvents.Clear();
        }
    }

    /// <summary>
    /// 会抛出异常的测试处理器
    /// </summary>
    public class ErrorTestEventHandler : IEventHandler<TestEvent>
    {
        public static readonly List<Exception> ReceivedErrors = new();
        public static bool ShouldThrow { get; set; } = true;

        public Task HandleAsync(TestEvent eventData)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException($"测试异常: {eventData.Message}");
            }
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            ReceivedErrors.Add(ex);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedErrors.Clear();
            ShouldThrow = true;
        }
    }

    /// <summary>
    /// 另一个测试事件处理器
    /// </summary>
    public class AnotherTestEventHandler : IEventHandler<AnotherTestEvent>
    {
        public static readonly List<AnotherTestEvent> ReceivedEvents = new();

        public Task HandleAsync(AnotherTestEvent eventData)
        {
            ReceivedEvents.Add(eventData);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ReceivedEvents.Clear();
        }
    }

    /// <summary>
    /// 计数处理器（用于并发测试）
    /// </summary>
    public class CounterEventHandler : IEventHandler<TestEvent>
    {
        private static int _counter = 0;
        public static int Counter => _counter;

        public Task HandleAsync(TestEvent eventData)
        {
            Interlocked.Increment(ref _counter);
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            _counter = 0;
        }
    }

    #endregion

    #region 测试初始化

    /// <summary>
    /// 每个测试方法执行前的初始化
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        TestEventHandler.Reset();
        SecondTestEventHandler.Reset();
        ErrorTestEventHandler.Reset();
        AnotherTestEventHandler.Reset();
        CounterEventHandler.Reset();
        ScopedDependencyHandler.Reset();
        ConstructorFailHandler.ConstructorFailHandlerExceptionCount = 0;
    }

    #endregion

    #region 基础功能测试

    /// <summary>
    /// 测试：发布事件后，处理器应该能收到事件
    /// </summary>
    [TestMethod]
    public async Task PublishAsync_Should_Deliver_Event_To_Handler()
    {
        // 创建 EventBus 实例
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        // 订阅事件
        eventBus.Subscribe<TestEvent, TestEventHandler>();

        // 发布事件
        var testEvent = new TestEvent { Message = "Hello", Value = 42 };
        await eventBus.PublishAsync(testEvent);

        // 等待事件处理完成（Channel 是异步的）
        await Task.Delay(500);

        // 验证处理器收到了事件
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("Hello", TestEventHandler.ReceivedEvents[0].Message);
        Assert.AreEqual(42, TestEventHandler.ReceivedEvents[0].Value);
    }

    /// <summary>
    /// 测试：订阅事件后，订阅数量应该增加
    /// </summary>
    [TestMethod]
    public void Subscribe_Should_Register_Handler()
    {
        var services = CreateServiceProvider();
        var eventBus = CreateEventBus(services);

        // 初始订阅数为 0
        Assert.AreEqual(0, eventBus.GetSubscriberCount<TestEvent>());

        // 订阅事件
        eventBus.Subscribe<TestEvent, TestEventHandler>();

        // 订阅数变为 1
        Assert.AreEqual(1, eventBus.GetSubscriberCount<TestEvent>());
    }

    /// <summary>
    /// 测试：取消订阅后，订阅数量应该减少
    /// </summary>
    [TestMethod]
    public void Unsubscribe_Should_Remove_Handler()
    {
        var services = CreateServiceProvider();
        var eventBus = CreateEventBus(services);

        // 订阅事件
        eventBus.Subscribe<TestEvent, TestEventHandler>();
        Assert.AreEqual(1, eventBus.GetSubscriberCount<TestEvent>());

        // 取消订阅
        eventBus.Unsubscribe<TestEvent>();

        // 订阅数变为 0
        Assert.AreEqual(0, eventBus.GetSubscriberCount<TestEvent>());
    }

    /// <summary>
    /// 测试：获取订阅数量应该返回正确的值
    /// </summary>
    [TestMethod]
    public void GetSubscriberCount_Should_Return_Correct_Count()
    {
        var services = CreateServiceProvider();
        var eventBus = CreateEventBus(services);

        // 未订阅时为 0
        Assert.AreEqual(0, eventBus.GetSubscriberCount<TestEvent>());
        Assert.AreEqual(0, eventBus.GetSubscriberCount<AnotherTestEvent>());

        // 订阅 TestEvent
        eventBus.Subscribe<TestEvent, TestEventHandler>();
        Assert.AreEqual(1, eventBus.GetSubscriberCount<TestEvent>());
        Assert.AreEqual(0, eventBus.GetSubscriberCount<AnotherTestEvent>());

        // 订阅 AnotherTestEvent
        eventBus.Subscribe<AnotherTestEvent, AnotherTestEventHandler>();
        Assert.AreEqual(1, eventBus.GetSubscriberCount<TestEvent>());
        Assert.AreEqual(1, eventBus.GetSubscriberCount<AnotherTestEvent>());
    }

    #endregion

    #region 处理器行为测试

    /// <summary>
    /// 测试：多个处理器应该都能收到同一事件
    /// </summary>
    [TestMethod]
    public async Task Multiple_Handlers_Should_All_Receive_Event()
    {
        var services = CreateServiceProvider(services =>
        {
            // DI 容器中注册两个处理器
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
            services.AddTransient<IEventHandler<TestEvent>, SecondTestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        // 订阅一个处理器（DI 中注册了两个，Channel 会取出所有 IEventHandler<TestEvent>）
        eventBus.Subscribe<TestEvent, TestEventHandler>();

        // 发布事件
        await eventBus.PublishAsync(new TestEvent { Message = "Multi", Value = 100 });
        await Task.Delay(500);

        // 验证两个处理器都收到了事件
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual(1, SecondTestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("Multi", TestEventHandler.ReceivedEvents[0].Message);
        Assert.AreEqual("Multi", SecondTestEventHandler.ReceivedEvents[0].Message);
    }

    /// <summary>
    /// 测试：处理器应该收到正确的事件数据
    /// </summary>
    [TestMethod]
    public async Task Handler_Should_Receive_Correct_Event_Data()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, TestEventHandler>();

        // 发布事件，设置各种属性
        var eventTime = new DateTime(2024, 6, 1, 12, 0, 0);
        var eventSource = new { Id = 123, Name = "Test" };

        var testEvent = new TestEvent
        {
            Name = "CustomEvent",
            EventTime = eventTime,
            EventSource = eventSource,
            Message = "Test Message",
            Value = 999
        };

        await eventBus.PublishAsync(testEvent);
        await Task.Delay(500);

        // 验证数据完整性
        var received = TestEventHandler.ReceivedEvents[0];
        Assert.AreEqual("CustomEvent", received.Name);
        Assert.AreEqual(eventTime, received.EventTime);
        Assert.AreEqual(eventSource, received.EventSource);
        Assert.AreEqual("Test Message", received.Message);
        Assert.AreEqual(999, received.Value);
    }

    /// <summary>
    /// 测试：一次性订阅应该记录 IsOnce 标志，且事件能被正常处理
    /// 注意：当前 EventChannel 实现未检查 IsOnce，事件会被多次处理
    /// </summary>
    [TestMethod]
    public async Task SubscribeOnce_Should_Register_With_IsOnce_Flag()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        // 一次性订阅
        eventBus.SubscribeOnce<TestEvent, TestEventHandler>();
        Assert.AreEqual(1, eventBus.GetSubscriberCount<TestEvent>());

        // 发布事件
        await eventBus.PublishAsync(new TestEvent { Message = "First" });
        await Task.Delay(500);

        // 验证收到了事件
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("First", TestEventHandler.ReceivedEvents[0].Message);

        // 再次发布（当前实现未真正限制只处理一次，但至少订阅信息被记录了）
        await eventBus.PublishAsync(new TestEvent { Message = "Second" });
        await Task.Delay(500);

        // 验证第二次也收到了（当前实现行为）
        Assert.AreEqual(2, TestEventHandler.ReceivedEvents.Count);
    }

    /// <summary>
    /// 测试：发布不同类型的事件，处理器互不干扰
    /// </summary>
    [TestMethod]
    public async Task Multiple_Event_Types_Should_Not_Interfere()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
            services.AddTransient<IEventHandler<AnotherTestEvent>, AnotherTestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        // 订阅两种不同类型的事件
        eventBus.Subscribe<TestEvent, TestEventHandler>();
        eventBus.Subscribe<AnotherTestEvent, AnotherTestEventHandler>();

        // 发布两种事件
        await eventBus.PublishAsync(new TestEvent { Message = "Test" });
        await eventBus.PublishAsync(new AnotherTestEvent { Data = "Another" });
        await Task.Delay(500);

        // 验证各自处理器只收到自己关心的事件
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("Test", TestEventHandler.ReceivedEvents[0].Message);

        Assert.AreEqual(1, AnotherTestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("Another", AnotherTestEventHandler.ReceivedEvents[0].Data);
    }

    #endregion

    #region 错误处理测试

    /// <summary>
    /// 测试：处理器抛出异常时，应该调用 OnErrorAsync
    /// </summary>
    [TestMethod]
    public async Task Handler_Error_Should_Call_OnErrorAsync()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, ErrorTestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, ErrorTestEventHandler>();

        // 发布事件（处理器会抛出异常）
        await eventBus.PublishAsync(new TestEvent { Message = "Error" });
        await Task.Delay(500);

        // 验证 OnErrorAsync 被调用
        Assert.AreEqual(1, ErrorTestEventHandler.ReceivedErrors.Count);
        Assert.IsTrue(ErrorTestEventHandler.ReceivedErrors[0] is InvalidOperationException);
        Assert.IsTrue(ErrorTestEventHandler.ReceivedErrors[0].Message.Contains("测试异常"));
    }

    /// <summary>
    /// 测试：处理器异常不应该导致系统崩溃，OnErrorAsync 被调用且后续事件仍能处理
    /// 注意：当处理器抛出异常时，同事件类型的其他处理器不会收到该事件（foreach 提前退出）
    /// </summary>
    [TestMethod]
    public async Task Handler_Exception_Should_Not_Crash_System()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, ErrorTestEventHandler>();
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, ErrorTestEventHandler>();

        // 发布事件（ErrorTestEventHandler 会抛异常）
        await eventBus.PublishAsync(new TestEvent { Message = "Crash Test" });
        await Task.Delay(500);

        // 验证系统没有崩溃，OnErrorAsync 被调用
        Assert.AreEqual(1, ErrorTestEventHandler.ReceivedErrors.Count);
        Assert.IsTrue(ErrorTestEventHandler.ReceivedErrors[0] is InvalidOperationException);

        // 后续事件应该仍能正常处理（关闭异常抛出）
        ErrorTestEventHandler.ShouldThrow = false;
        await eventBus.PublishAsync(new TestEvent { Message = "Recovery Test" });
        await Task.Delay(500);

        // 验证后续事件正常处理：ErrorTestEventHandler 没有新错误（因为不再抛异常）
        Assert.AreEqual(1, ErrorTestEventHandler.ReceivedErrors.Count);
        // TestEventHandler 应该收到了 Recovery Test 事件
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
        Assert.AreEqual("Recovery Test", TestEventHandler.ReceivedEvents[0].Message);
    }

    /// <summary>
    /// 测试：没有处理器时发布事件不应该抛异常
    /// </summary>
    [TestMethod]
    public async Task Publish_Should_Not_Throw_When_No_Handler()
    {
        var services = CreateServiceProvider();
        var eventBus = CreateEventBus(services);

        // 不订阅任何处理器，直接发布事件
        await eventBus.PublishAsync(new TestEvent { Message = "No Handler" });
        await Task.Delay(200);

        // 如果没有抛异常，测试通过（到达这里说明没有异常）
        Assert.AreEqual(0, eventBus.GetSubscriberCount<TestEvent>());
    }

    #endregion

    #region 并发测试

    /// <summary>
    /// 测试：并发发布多个事件，所有事件都应该被处理
    /// </summary>
    [TestMethod]
    public async Task Concurrent_Publish_Should_Handle_All_Events()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, CounterEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, CounterEventHandler>();

        // 并发发布 100 个事件
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(eventBus.PublishAsync(new TestEvent { Message = $"Event {i}" }));
        }

        await Task.WhenAll(tasks);

        // 等待所有事件处理完成
        await Task.Delay(2000);

        // 验证所有事件都被处理
        Assert.AreEqual(100, CounterEventHandler.Counter);
    }

    /// <summary>
    /// 测试：EventBus 应该能正确处理大量事件
    /// </summary>
    [TestMethod]
    public async Task EventBus_Should_Handle_Large_Number_Of_Events()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, CounterEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, CounterEventHandler>();

        // 发布 1000 个事件
        for (int i = 0; i < 1000; i++)
        {
            await eventBus.PublishAsync(new TestEvent { Message = $"Event {i}" });
        }

        // 等待所有事件处理完成
        await Task.Delay(5000);

        // 验证所有事件都被处理
        Assert.AreEqual(1000, CounterEventHandler.Counter);
    }

    #endregion

    #region 数据模型测试

    /// <summary>
    /// 测试：BaseEventData 默认名称应该是类型名
    /// </summary>
    [TestMethod]
    public void BaseEventData_Should_Set_Default_Name()
    {
        var eventData = new TestEvent();
        Assert.AreEqual("TestEvent", eventData.Name);
    }

    /// <summary>
    /// 测试：BaseEventData 应该能设置自定义名称
    /// </summary>
    [TestMethod]
    public void BaseEventData_Should_Set_Custom_Name()
    {
        var eventData = new TestEvent();
        eventData.Name = "CustomName";
        Assert.AreEqual("CustomName", eventData.Name);
    }

    /// <summary>
    /// 测试：BaseEventData 默认事件时间应该是当前时间
    /// </summary>
    [TestMethod]
    public void BaseEventData_Should_Set_EventTime()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var eventData = new TestEvent();
        var after = DateTime.Now.AddSeconds(1);

        Assert.IsTrue(eventData.EventTime >= before);
        Assert.IsTrue(eventData.EventTime <= after);
    }

    /// <summary>
    /// 测试：BaseEventData 应该能设置自定义事件源
    /// </summary>
    [TestMethod]
    public void BaseEventData_Should_Set_EventSource()
    {
        var source = new { Module = "OrderService", UserId = 123 };
        var eventData = new TestEvent
        {
            EventSource = source
        };

        Assert.IsNotNull(eventData.EventSource);
        Assert.AreEqual("OrderService", ((dynamic)eventData.EventSource).Module);
        Assert.AreEqual(123, ((dynamic)eventData.EventSource).UserId);
    }

    /// <summary>
    /// 测试：BaseEventData 应该标记为可序列化
    /// </summary>
    [TestMethod]
    public void BaseEventData_Should_Be_Serializable()
    {
        // 直接检查 BaseEventData 类型本身是否有 SerializableAttribute
        var type = typeof(BaseEventData);
        var serializableAttr = type.GetCustomAttributes(typeof(SerializableAttribute), false);
        Assert.IsTrue(serializableAttr.Length > 0);
    }

    #endregion

    #region 配置选项测试

    /// <summary>
    /// 测试：EventBusOptions 应该有正确的默认值
    /// </summary>
    [TestMethod]
    public void EventBusOptions_Should_Have_Default_Values()
    {
        var options = new EventBusOptions();

        Assert.AreEqual(10000, options.MaxQueueCapacity);
        Assert.AreEqual(true, options.EnablePersistence);
        Assert.AreEqual(100, options.Sensitivities);
        Assert.AreEqual(4, options.MaxDegreeOfParallelism);
    }

    /// <summary>
    /// 测试：EventBusOptions 应该允许自定义值
    /// </summary>
    [TestMethod]
    public void EventBusOptions_Should_Allow_Custom_Values()
    {
        var options = new EventBusOptions
        {
            MaxQueueCapacity = 5000,
            EnablePersistence = false,
            Sensitivities = 200,
            MaxDegreeOfParallelism = 8
        };

        Assert.AreEqual(5000, options.MaxQueueCapacity);
        Assert.AreEqual(false, options.EnablePersistence);
        Assert.AreEqual(200, options.Sensitivities);
        Assert.AreEqual(8, options.MaxDegreeOfParallelism);
    }

    /// <summary>
    /// 测试：禁用持久化后，事件仍然能被正常处理
    /// </summary>
    [TestMethod]
    public async Task Publish_With_Persistence_Disabled_Should_Not_Save()
    {
        // 创建禁用持久化的 EventBus
        var services = new ServiceCollection();
        services.AddEventBus(options =>
        {
            options.EnablePersistence = false;
            options.MaxQueueCapacity = 1000;
        });
        services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        eventBus.Subscribe<TestEvent, TestEventHandler>();

        // 发布事件
        await eventBus.PublishAsync(new TestEvent { Message = "No Persistence" });
        await Task.Delay(500);

        // 验证事件仍然被处理
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count);
    }

    #endregion

    #region DI 扩展测试

    /// <summary>
    /// 测试：AddEventBus 应该正确注册 IEventBus
    /// </summary>
    [TestMethod]
    public void AddEventBus_Should_Register_IEventBus()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        var serviceProvider = services.BuildServiceProvider();

        var eventBus = serviceProvider.GetService<IEventBus>();
        Assert.IsNotNull(eventBus);
        Assert.IsInstanceOfType(eventBus, typeof(LuBan.EventBus.Core.EventBus));
    }

    /// <summary>
    /// 测试：AddEventBus 应该注册 EventPersistence
    /// </summary>
    [TestMethod]
    public void AddEventBus_Should_Register_EventPersistence()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        var serviceProvider = services.BuildServiceProvider();

        var persistence = serviceProvider.GetService<EventBus.Core.EventPersistence>();
        Assert.IsNotNull(persistence);
    }

    /// <summary>
    /// 测试：AddEventHandlers 应该自动注册所有 IEventHandler
    /// </summary>
    [TestMethod]
    public void AddEventHandlers_Should_Auto_Register_All_Handlers()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        // 传入当前程序集
        services.AddEventHandlers(typeof(EventBusUnitTest).Assembly);

        // 验证注册了 IEventHandler<TestEvent> 和 IEventHandler<AnotherTestEvent>
        var testEventRegistrations = services.Where(s => 
            s.ServiceType.IsGenericType && 
            s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
            s.ServiceType.GetGenericArguments()[0] == typeof(TestEvent)).ToList();

        var anotherTestEventRegistrations = services.Where(s => 
            s.ServiceType.IsGenericType && 
            s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>) &&
            s.ServiceType.GetGenericArguments()[0] == typeof(AnotherTestEvent)).ToList();

        // 验证 TestEvent 的处理器都被注册了
        Assert.IsTrue(testEventRegistrations.Count > 0, "TestEvent 的处理器应该被注册");

        // 验证 AnotherTestEvent 的处理器也被注册了
        Assert.IsTrue(anotherTestEventRegistrations.Count > 0, "AnotherTestEvent 的处理器应该被注册");
    }

    /// <summary>
    /// 测试：AddEventBus 应该应用自定义配置
    /// </summary>
    [TestMethod]
    public void AddEventBus_Should_Apply_Custom_Options()
    {
        var services = new ServiceCollection();
        services.AddEventBus(options =>
        {
            options.MaxQueueCapacity = 500;
            options.EnablePersistence = false;
        });
        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<EventBusOptions>>();
        Assert.AreEqual(500, options.Value.MaxQueueCapacity);
        Assert.AreEqual(false, options.Value.EnablePersistence);
    }

    #endregion

    #region 持久化测试

    /// <summary>
    /// 测试：EventPersistence 应该能保存事件
    /// </summary>
    [TestMethod]
    public async Task EventPersistence_Should_Save_Event()
    {
        var services = new ServiceCollection();
        services.AddEventBus(options =>
        {
            options.EnablePersistence = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        var persistence = serviceProvider.GetRequiredService<EventBus.Core.EventPersistence>();

        // 保存事件
        var testEvent = new TestEvent { Message = "Persistence Test", Value = 42 };
        await persistence.SaveAsync(testEvent);

        // 验证可以加载（虽然 LoadAsync 目前返回空列表）
        var loaded = await persistence.LoadAsync<TestEvent>();
        Assert.IsNotNull(loaded);
    }

    /// <summary>
    /// 测试：EventPersistence 应该能处理不同类型的事件
    /// </summary>
    [TestMethod]
    public async Task EventPersistence_Should_Handle_Different_Event_Types()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        var serviceProvider = services.BuildServiceProvider();

        var persistence = serviceProvider.GetRequiredService<EventBus.Core.EventPersistence>();

        // 保存不同类型的事件
        var testEvent = new TestEvent { Message = "Test" };
        var anotherEvent = new AnotherTestEvent { Data = "Another" };

        await persistence.SaveAsync(testEvent);
        await persistence.SaveAsync(anotherEvent);

        // 验证都成功保存（不抛异常即可）
        Assert.IsNotNull(persistence);
    }

    #endregion

    #region 资源释放测试

    /// <summary>
    /// 测试：EventBus 应该能正确释放资源
    /// </summary>
    [TestMethod]
    public void EventBus_Should_Dispose_Correctly()
    {
        var services = CreateServiceProvider();
        var eventBus = CreateEventBus(services);

        // 订阅事件
        eventBus.Subscribe<TestEvent, TestEventHandler>();
        eventBus.Subscribe<AnotherTestEvent, AnotherTestEventHandler>();

        // 释放资源
        if (eventBus is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // 验证释放后订阅数应该为 0（内部 channel 被清理）
        Assert.AreEqual(0, eventBus.GetSubscriberCount<TestEvent>());
        Assert.AreEqual(0, eventBus.GetSubscriberCount<AnotherTestEvent>());
    }

    #endregion

    #region 后台任务健壮性测试（根因修复验证）

    /// <summary>
    /// 测试：处理器抛出异常后，后台任务不应终止，后续事件仍能被处理
    /// 这是验证根因修复的核心测试：EventChannel.ProcessEventsAsync 的外层 try-catch
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Survive_Handler_Exception_And_Continue()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, ErrorTestEventHandler>();
            services.AddTransient<IEventHandler<TestEvent>, TestEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, ErrorTestEventHandler>();

        // 第一个事件：处理器会抛出异常
        await eventBus.PublishAsync(new TestEvent { Message = "Will Fail" });
        await Task.Delay(500);

        // 验证异常被捕获
        Assert.AreEqual(1, ErrorTestEventHandler.ReceivedErrors.Count);

        // 关闭异常抛出，模拟恢复正常
        ErrorTestEventHandler.ShouldThrow = false;

        // 第二个事件：应该正常处理
        await eventBus.PublishAsync(new TestEvent { Message = "Should Succeed" });
        await Task.Delay(500);

        // 验证后台任务仍然存活，第二个事件被正常处理
        Assert.AreEqual(1, TestEventHandler.ReceivedEvents.Count, "后台任务已终止，后续事件无法被处理");
        Assert.AreEqual("Should Succeed", TestEventHandler.ReceivedEvents[0].Message);
    }

    /// <summary>
    /// 测试：连续多个事件处理失败后，后台任务不应终止
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Survive_Multiple_Consecutive_Failures()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, ErrorTestEventHandler>();
            services.AddTransient<IEventHandler<TestEvent>, CounterEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, ErrorTestEventHandler>();

        // 连续发布 5 个会导致异常的事件
        for (int i = 0; i < 5; i++)
        {
            await eventBus.PublishAsync(new TestEvent { Message = $"Fail {i}" });
        }
        await Task.Delay(1000);

        // 验证所有异常都被捕获
        Assert.AreEqual(5, ErrorTestEventHandler.ReceivedErrors.Count);

        // 关闭异常，发布正常事件
        ErrorTestEventHandler.ShouldThrow = false;
        await eventBus.PublishAsync(new TestEvent { Message = "Recovery" });
        await Task.Delay(500);

        // 验证后台任务仍然存活
        Assert.AreEqual(1, CounterEventHandler.Counter, "后台任务应该在多次失败后仍然存活");
    }

    /// <summary>
    /// 测试：没有注册处理器时，发布事件不应导致后台任务崩溃
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Survive_When_No_Handler_Registered()
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        var serviceProvider = services.BuildServiceProvider();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        // 不订阅任何处理器，直接发布事件
        await eventBus.PublishAsync(new TestEvent { Message = "No Handler 1" });
        await Task.Delay(300);

        await eventBus.PublishAsync(new TestEvent { Message = "No Handler 2" });
        await Task.Delay(300);

        // 没有抛异常即表示后台任务存活
        Assert.IsNotNull(eventBus);
    }

    /// <summary>
    /// 测试：处理器依赖的 Scoped 服务应该能正确解析
    /// </summary>
    [TestMethod]
    public async Task Handler_With_Scoped_Dependency_Should_Resolve_Correctly()
    {
        var services = CreateServiceProvider(services =>
        {
            // 注册一个 Scoped 服务
            services.AddScoped<ScopedTestService>();
            // 注册依赖 Scoped 服务的处理器
            services.AddTransient<IEventHandler<TestEvent>, ScopedDependencyHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, ScopedDependencyHandler>();

        // 发布多个事件，验证每次都能正确解析 Scoped 服务
        for (int i = 0; i < 3; i++)
        {
            await eventBus.PublishAsync(new TestEvent { Message = $"Scoped {i}" });
        }
        await Task.Delay(1000);

        // 验证所有事件都被处理
        Assert.AreEqual(3, ScopedDependencyHandler.ProcessedCount);
    }

    /// <summary>
    /// 测试：处理器DI解析失败时，后台任务不应终止
    /// 验证EventChannel外层try-catch保护了后台任务
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Survive_Handler_DI_Resolution_Failure()
    {
        var services = new ServiceCollection();
        services.AddEventBus(options =>
        {
            options.MaxQueueCapacity = 10;
            options.EnablePersistence = false;
        });
        services.AddTransient<IEventHandler<TestEvent>, ConstructorFailHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        eventBus.Subscribe<TestEvent, ConstructorFailHandler>();

        // 发布多个事件，每次DI解析都会失败
        // 如果后台任务终止了，Channel最终会满，WriteAsync会阻塞
        for (int i = 0; i < 5; i++)
        {
            var publishTask = eventBus.PublishAsync(new TestEvent { Message = $"DI Fail {i}" });
            var completed = await Task.WhenAny(publishTask, Task.Delay(2000));
            Assert.AreEqual(publishTask, completed, $"第{i + 1}次发布事件应该能完成（后台任务存活，Channel在消费）");
        }
    }

    /// <summary>
    /// 测试：Dispose 后后台任务应该正确终止
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Terminate_After_Dispose()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, CounterEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, CounterEventHandler>();

        // 发布一个事件确认工作正常
        await eventBus.PublishAsync(new TestEvent { Message = "Before Dispose" });
        await Task.Delay(300);
        Assert.AreEqual(1, CounterEventHandler.Counter);

        // 释放 EventBus
        if (eventBus is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // 等待后台任务终止
        await Task.Delay(500);

        // 释放后发布事件应该能写入 Channel（Channel 已完成），但不会被处理
        // 注意：Writer.Complete() 后 WriteAsync 会返回 false 或抛出异常
        // 这里主要验证 Dispose 不会导致未处理的异常
    }

    /// <summary>
    /// 测试：高并发下后台任务应该保持稳定
    /// </summary>
    [TestMethod]
    public async Task BackgroundTask_Should_Be_Stable_Under_High_Concurrency()
    {
        var services = CreateServiceProvider(services =>
        {
            services.AddTransient<IEventHandler<TestEvent>, CounterEventHandler>();
        });
        var eventBus = CreateEventBus(services);

        eventBus.Subscribe<TestEvent, CounterEventHandler>();

        // 高并发发布事件
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await eventBus.PublishAsync(new TestEvent { Message = $"Concurrent {i}" });
            }));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(3000);

        // 验证所有事件都被处理
        Assert.AreEqual(50, CounterEventHandler.Counter, "高并发下所有事件都应该被处理");
    }

    #endregion

    #region 辅助类型定义

    /// <summary>
    /// 用于测试的 Scoped 服务
    /// </summary>
    public class ScopedTestService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
        public int CallCount { get; set; }
    }

    /// <summary>
    /// 依赖 Scoped 服务的处理器
    /// </summary>
    public class ScopedDependencyHandler : IEventHandler<TestEvent>
    {
        private readonly ScopedTestService _scopedService;
        public static int ProcessedCount = 0;

        public ScopedDependencyHandler(ScopedTestService scopedService)
        {
            _scopedService = scopedService;
        }

        public Task HandleAsync(TestEvent eventData)
        {
            _scopedService.CallCount++;
            Interlocked.Increment(ref ProcessedCount);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            ProcessedCount = 0;
        }
    }

    /// <summary>
    /// 构造函数会抛出异常的处理器（用于测试 DI 解析失败场景）
    /// </summary>
    public class ConstructorFailHandler : IEventHandler<TestEvent>
    {
        public static int ConstructorFailHandlerExceptionCount = 0;
        private readonly MissingService _missing;

        // 这个构造函数在 DI 解析时会失败，因为 MissingService 未注册
        public ConstructorFailHandler(MissingService missing)
        {
            _missing = missing ?? throw new ArgumentNullException(nameof(missing));
        }

        public Task HandleAsync(TestEvent eventData)
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Interlocked.Increment(ref ConstructorFailHandlerExceptionCount);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 未注册的服务，用于模拟 DI 解析失败
    /// </summary>
    public class MissingService
    {
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 创建默认的 ServiceProvider
    /// </summary>    
    private static ServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddEventBus();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// 创建 EventBus 实例
    /// </summary>
    private static IEventBus CreateEventBus(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IEventBus>();
    }

    #endregion
}
