using Hzdtf.AMQP.Contract.Standard.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.AspNet.Core
{
    /// <summary>
    /// Rabbit连接打开配置选项
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionOpenOptions : RabbitConnectionFactoryOptions
    {
        /// <summary>
        /// 主机ID
        /// </summary>
        public string HostId
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Rabbit连接打开自定义配置选项
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionOpenCustomerOptions : RabbitConnectionOpenOptions
    {
        /// <summary>
        /// 配置读取
        /// </summary>
        public IAmqpConfigReader ConfigReader
        {
            get;
            set;
        }
    }
}
