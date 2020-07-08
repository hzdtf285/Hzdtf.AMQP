using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.AMQP.Impl.Standard;
using Hzdtf.AMQP.Impl.Standard.Connection;
using Hzdtf.Platform.Config.Contract.Standard.Config.App;
using Hzdtf.Utility.Standard.Attr;
using Hzdtf.Utility.Standard.Safety;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.RabbitV2.Impl.Standard.Connection
{
    /// <summary>
    /// Rabbit连接工厂
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class RabbitConnectionFactory : AmqpConnectionFactoryBase
    {
        /// <summary>
        /// AMQP配置读取
        /// </summary>
        private readonly IAmqpConfigReader amqpConfigReader;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="symmetricalEncryption">加密</param>
        /// <param name="appConfig">应用配置</param>
        /// <param name="amqpConfigReader">配置读取</param>
        public RabbitConnectionFactory(ISymmetricalEncryption symmetricalEncryption = null, IAppConfiguration appConfig = null, IAmqpConfigReader amqpConfigReader = null)
            : base(symmetricalEncryption, appConfig)
        {
            this.amqpConfigReader = AmqpUtil.GetConfigReader(amqpConfigReader);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns>连接</returns>
        public override IAmqpConnection Create() => new RabbitConnection(amqpConfigReader, symmetricalEncryption);
    }
}
