# Hzdtf.Consul.AMQP
基于AMQP协议作为接口层，以RabbitMQ作为封装对象，通过amqp.json统一配置，使用者更方便，语言：C#。主要功能有：消费，生产，RPC通讯，业务处理异常自动发生异常队列，与consul配置中心集成，使amqp.json可动态更改配置。

本框架.NET Core 3.1.5 以上。下载源码用Visual Studio 2019打开。
工程以Standard或Std结尾是标准库，以Framework或Frm结尾为Framework库，以Core结尾为Core库。
初始编译时会耗些时间，因为要从nuget下载包。
本库依赖类库是：
1、Hzdrtf.Utility
2、Hzdrtf.Utility.AspNet.Core
3、Hzdtf.Logger
4、Hzdtf.Platform
...
类库统一放在src/Library里。（依赖库源码都可在Hzdtf.Foundation.Framework和Hzdtf.Consul.Extensions里找到）

框架的核心都在amqp.json文件里，请参考各个样例项目。下面主要介绍功能点：
1、消费者服务：
在StartUp.ConfigureServices里添加以下代码：
var conn = services.AddRabbitConnectionAndOpenConsulConfigCenter("host1"); // 此句是结合consul配置中心，如果不需要配置中心，则改为：var conn = services.AddRabbitConnectionAndOpen("host1");
var consumer = conn.CreateConsumer("TestExchange", "TestQueue1"); // 作为消费者服务，需要输入监听的交换机和队列
consumer.Subscribe((string msg) =>
{
	Console.WriteLine("接收到消息：" + msg);
	
	// throw new Exception("测试异常"); // 如果在这里抛出异常，该消费服务又有在配置文件配置发送异常队列，则自动会发送到异常队列里

	return true; // 返回是否处理成功；如果为false，默认会把此消息转发到下一个消费者上
});

2、生产者：
（1）、在StartUp.ConfigureServices里添加以下代码：
services.AddRabbitConnectionFactoryConsulConfigCenter(); // 如果要将amqp.json加入到consul配置中心 ，则执行此句，如果不需要配置中心，则执行：services.AddRabbitConnectionFactory();

（2）、在控制器里通过构造函数将IAmqpConnectionFactory连接工厂注入进来，如下代码：
private readonly IAmqpConnectionFactory factory;

public WeatherForecastController(ILogger<WeatherForecastController> logger, IAmqpConnectionFactory factory)
{
	_logger = logger;
	this.factory = factory;
}

（3）、在Action里执行需要发送消息：
using (var conn = factory.Create()) // 此处是利用工厂创建连接，为了演示所以简例。正式做法是，应该把连接缓存起来
{
	conn.OpenByHostId("host1");
	using (var producer = conn.CreateProducer("TestExchange")) // 输入要发布目的交换机
	{
		producer.Publish("这是一个测试数据", "TestKey1");
	}
}

3、RPC监听服务：
（1）、程序启动时，执行如下代码：
/// <summary>
/// 业务调用RPC服务端
/// </summary>
private static void BusinessCallRpcServer()
{
	// 创建Rabbit连接和Rpc服务端
	var conn = new RabbitConnection();
	conn.OpenByHostId("host1");
	var rpcServer = conn.CreateRpcServer("RpcExchange", "RpcQueue");

	// 创建Rpc监听服务
	var listen = new RpcServerListen();

	// 设置接口映射配置文件
	var mapImplCache = new InterfaceMapImplCache();
	mapImplCache.Set(new DictionaryJson("Config/interfaceAssemblyMapImplAssemblyConfig.json"));

	// 将映射赋值到监听中
	listen.InterfaceMapImpl = mapImplCache;

	// 将RPC服务设置到监听中
	listen.RpcServer = rpcServer;
	listen.BytesSerialization = new MessagePackBytesSerialization();

	// 注册错误事件
	listen.ReceivingError += Listen_ReceivingError;

	// 开始监听
	Console.WriteLine("监听Rpc服务的请求：");
	listen.ListenAsync();
}

/// <summary>
/// 监听接收出错
/// </summary>
/// <param name="arg1">错误消息</param>
/// <param name="arg2">异常</param>
private static void Listen_ReceivingError(string arg1, Exception arg2)
{
	Console.WriteLine(arg1 + "," + arg2 ?? arg2.ToString());
}

（2）、添加业务接口层与实现层的映射关系：
在Config\interfaceAssemblyMapImplAssemblyConfig.json添加，样例如下：
{
  "Hzdtf.BusinessDemo.Contract.Standard": "Hzdtf.BusinessDemo.Impl.Standard"
}

Hzdtf.BusinessDemo.Contract.Standard：业务接口层，只定义接口
Hzdtf.BusinessDemo.Impl.Standard：业务实现层，是实现上面的接口

当RPC客户端调用时，会传入接口层的信息，服务端就是通过这个配置找到实现类去执行的。

4、RPC客户端：
 样例代码如下：
var proxy = new RabbitRpcDispatchProxyClient(); // RabbitMQ RPC动态代理库
var service = proxy.Create<IPersonService>(); // 创建接口层的动态代理类
var re = service.Get(10); // 执行远程调用，就像调用本地方法一样简单方便
Console.WriteLine("Get:" + re);
var re1 = service.Query();
Console.WriteLine("Query:" + re1);


具体使用请查看各个项目样例
