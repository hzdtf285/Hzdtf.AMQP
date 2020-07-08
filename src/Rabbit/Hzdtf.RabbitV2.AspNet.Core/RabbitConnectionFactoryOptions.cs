using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.Configuration.Impl.Config.Core.App;
using Hzdtf.Platform.Config.Contract.Standard.Config.App;
using Hzdtf.Utility.Standard.Safety;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.AspNet.Core
{
    /// <summary>
    /// Rabbit连接工厂配置选项
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionFactoryOptions
    {
        /// <summary>
        /// 加密
        /// </summary>
        public ISymmetricalEncryption SymmetricalEncryption
        {
            get;
            set;
        } = SymmetricalEncryptionUtil.DefaultSymmetricalEncryption;

        /// <summary>
        /// 配置
        /// </summary>
        public IAppConfiguration AppConfig
        {
            get;
            set;
        } = new AppConfiguration();
    }

    /// <summary>
    /// Rabbit连接工厂自定义选项配置
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionFactoryCustomerOptions : RabbitConnectionFactoryOptions
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
