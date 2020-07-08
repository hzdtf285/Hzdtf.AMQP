using Hzdtf.Consul.Extensions.AspNet.Core;
using Hzdtf.RabbitV2.AspNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hzdtf.RabbitV2.Consul.AspNet.Core
{
    /// <summary>
    /// Rabbit Consull连接选项配置
    /// @ 黄振东
    /// </summary>
    public class RabbitConsulConnectionOptions : RabbitConnectionFactoryOptions
    {
        /// <summary>
        /// Consul配置文件
        /// </summary>
        public string ConsulConfigFile
        {
            get;
            set;
        } = "Config/consulConfig.json";
    }
}
