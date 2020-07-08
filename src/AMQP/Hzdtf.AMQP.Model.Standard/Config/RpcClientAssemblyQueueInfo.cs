using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Config
{
    /// <summary>
    /// RPC客户端程序集队列信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class RpcClientAssemblyQueueInfo
    {
        /// <summary>
        /// 主机ID
        /// </summary>
        [JsonProperty("hostId")]
        [Key("hostId")]
        public string HostId { get; set; }

        /// <summary>
        /// 交换机名
        /// </summary>
        [JsonProperty("exchangeName")]
        [Key("exchangeName")]
        public string ExchangeName { get; set; }

        /// <summary>
        /// 队列名
        /// </summary>
        [JsonProperty("queueName")]
        [Key("queueName")]
        public string QueueName { get; set; }

        /// <summary>
        /// RPC客户端程序集
        /// </summary>
        [JsonProperty("rpcClientAssembly")]
        [Key("rpcClientAssembly")]
        public RpcClientAssemblyInfo RpcClientAssembly { get; set; }
    }
}
