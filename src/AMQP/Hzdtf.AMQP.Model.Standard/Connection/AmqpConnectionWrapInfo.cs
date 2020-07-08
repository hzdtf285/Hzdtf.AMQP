using Hzdtf.Utility.Standard.Connection;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Connection
{
    /// <summary>
    /// AMQP连接包装信息，优先级是：HostId>DefaultConnection>ConnectionInfo>ConnectionString>ConnectionStringAppConfigName
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class AmqpConnectionWrapInfo : ConnectionWrapInfo<AmqpConnectionInfo>
    {
        /// <summary>
        /// 虚拟路径
        /// </summary>
        [JsonProperty("hostId")]
        [MessagePack.Key("hostId")]
        public string HostId
        {
            get;
            set;
        }
    }
}
