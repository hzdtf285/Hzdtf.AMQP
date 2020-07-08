using Hzdtf.Utility.Standard.Utils;
using MessagePack;
using Newtonsoft.Json;
using System;

namespace Hzdtf.AMQP.Model.Standard.Config
{
    /// <summary>
    /// AMQP配置信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class AmqpConfigInfo
    {
        /// <summary>
        /// Amqp
        /// </summary>
        [JsonProperty("amqp")]
        [Key("amqp")]
        public AmqpInfo[] Amqp { get; set; }
    }

    /// <summary>
    /// AMQP信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class AmqpInfo
    {
        /// <summary>
        /// 主机ID
        /// </summary>
        [JsonProperty("hostId")]
        [Key("hostId")]
        public string HostId { get; set; }

        /// <summary>
        /// 连接是否加密
        /// </summary>
        [JsonProperty("connectionEncrypt")]
        [Key("connectionEncrypt")]
        public bool ConnectionEncrypt { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        [JsonProperty("connectionString")]
        [Key("connectionString")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// 交换机数组
        /// </summary>
        [JsonProperty("exchanges")]
        [Key("exchanges")]
        public ExchangeInfo[] Exchanges { get; set; }
    }

    /// <summary>
    /// 交换机信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class ExchangeInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        [Key("name")]
        public string Name { get; set; }

        /// <summary>
        /// 持久化，默认为是
        /// </summary>
        [JsonProperty("persistent")]
        [Key("persistent")]
        public bool Persistent { get; set; } = true;

        /// <summary>
        /// 类型（默认直通)
        /// </summary>
        [JsonProperty("type")]
        [Key("type")]
        public string Type { get; set; } = AmqpDefineUtil.DIRECT_EXCHANGE_NAME;

        /// <summary>
        /// 队列数组
        /// </summary>
        [JsonProperty("queues")]
        [Key("queues")]
        public QueueInfo[] Queues { get; set; }
    }

    /// <summary>
    /// 队列信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class QueueInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        [Key("name")]
        public string Name { get; set; }

        /// <summary>
        /// 服务质量数
        /// </summary>
        [JsonProperty("qos")]
        [Key("qos")]
        public ushort? Qos { get; set; }

        /// <summary>
        /// 路由键集合
        /// </summary>
        [JsonProperty("routingKeys")]
        [Key("routingKeys")]
        public string[] RoutingKeys { get; set; }

        /// <summary>
        /// 自动删除队列
        /// </summary>
        [JsonProperty("autoDelQueue")]
        [Key("autoDelQueue")]
        public bool AutoDelQueue { get; set; }

        /// <summary>
        /// RPC客户端程序集数组
        /// </summary>
        [JsonProperty("rpcClientAssemblys")]
        [Key("rpcClientAssemblys")]
        public RpcClientAssemblyInfo[] RpcClientAssemblys { get; set; }

        /// <summary>
        /// 异常处理
        /// </summary>
        [JsonProperty("exceptionHandle")]
        [Key("exceptionHandle")]
        public ExceptionHandleInfo ExceptionHandle { get; set; }

        /// <summary>
        /// 获取路由键
        /// </summary>
        /// <param name="routingKey">路由键</param>
        /// <returns>路由键</returns>
        public string GetRoutingKey(string routingKey = null)
        {
            if (RoutingKeys.IsNullOrLength0())
            {
                return routingKey;
            }

            return string.IsNullOrWhiteSpace(routingKey) ? RoutingKeys[0] : routingKey;
        }
    }

    /// <summary>
    /// RPC客户端程序集信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class RpcClientAssemblyInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        [Key("name")]
        public string Name { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [JsonProperty("dataType")]
        [Key("dataType")]
        public string DataType { get; set; }
    }

    /// <summary>
    /// 异常处理信息
    /// </summary>
    [MessagePackObject]
    public class ExceptionHandleInfo
    {
        /// <summary>
        /// 服务名
        /// </summary>
        [JsonProperty("serviceName")]
        [Key("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 发布消费者信息数组
        /// </summary>
        [JsonProperty("publishConsumers")]
        [Key("publishConsumers")]
        public PublishConsumerInfo[] PublishConsumers { get; set; }
    }

    /// <summary>
    /// 发布消费者信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class PublishConsumerInfo
    {
        /// <summary>
        /// 主机ID
        /// </summary>
        [JsonProperty("hostId")]
        [Key("hostId")]
        public string HostId { get; set; }

        /// <summary>
        /// 交换机
        /// </summary>
        [JsonProperty("exchange")]
        [Key("exchange")]
        public string Exchange { get; set; }

        /// <summary>
        /// 路由键
        /// </summary>
        [JsonProperty("routingKey")]
        [Key("routingKey")]
        public string RoutingKey { get; set; }
    }

}
