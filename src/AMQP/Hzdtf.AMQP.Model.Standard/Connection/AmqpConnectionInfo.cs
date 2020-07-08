using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Utility.Standard.Connection;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Connection
{
    /// <summary>
    /// AMQP连接信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class AmqpConnectionInfo : ConnectionInfo
    {
        /// <summary>
        /// 虚拟路径
        /// </summary>
        [JsonProperty("virtualPath")]
        [MessagePack.Key("virtualPath")]
        public string VirtualPath
        {
            get;
            set;
        } = AmqpDefineUtil.DEFAULT_VIRTUAL_PATH;

        /// <summary>
        /// 心跳包间隔时间（单位：秒），默认为60秒
        /// </summary>
        [JsonProperty("heartbeat")]
        [MessagePack.Key("heartbeat")]
        public ushort Heartbeat
        {
            get;
            set;
        } = 60;

        /// <summary>
        /// 自动恢复连接，默认开启
        /// </summary>
        [JsonProperty("autoRecovery")]
        [MessagePack.Key("autoRecovery")]
        public bool AutoRecovery
        {
            get;
            set;
        } = true;
    }
}
