using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Config
{
    /// <summary>
    /// AMQP队列信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class AmqpQueueInfo
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
        /// 虚拟路径
        /// </summary>
        [JsonProperty("virtualPath")]
        [Key("virtualPath")]
        public string VirtualPath { get; set; } = AmqpDefineUtil.DEFAULT_VIRTUAL_PATH;

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
        /// 队列
        /// </summary>
        [JsonProperty("queue")]
        [Key("queue")]
        public QueueInfo Queue { get; set; }

        /// <summary>
        /// 异常处理
        /// </summary>
        [JsonProperty("exceptionHandle")]
        [Key("exceptionHandle")]
        public ExceptionHandleInfo ExceptionHandle { get; set; }

        /// <summary>
        /// 根据交换机信息和队列信息创建AMQP队列
        /// </summary>
        /// <param name="hostId">主机ID</param>
        /// <param name="exchange">交换机信息</param>
        /// <param name="queue">队列信息</param>
        /// <returns>AMQP队列</returns>
        public static AmqpQueueInfo CreateAmqpQueue(string hostId, ExchangeInfo exchange, QueueInfo queue)
        {
            var result =  new AmqpQueueInfo()
            {
                HostId = hostId,
                ExchangeName = exchange.Name,
                Persistent = exchange.Persistent,
                Queue = queue,
                Type = exchange.Type
            };
            if (queue != null)
            {
                result.ExceptionHandle = queue.ExceptionHandle;
            }

            return result;
        }
    }
}
