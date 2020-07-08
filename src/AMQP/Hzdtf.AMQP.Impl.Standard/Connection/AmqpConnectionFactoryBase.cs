using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Platform.Config.Contract.Standard.Config.App;
using Hzdtf.Platform.Contract.Standard.Config.Connection;
using Hzdtf.Utility.Standard.Safety;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Connection
{
    /// <summary>
    /// AMQP连接工厂基类
    /// @ 黄振东
    /// </summary>
    public abstract class AmqpConnectionFactoryBase : ConnectionAppConfigFactoryBase<IAmqpConnection, AmqpConnectionInfo, AmqpConnectionWrapInfo>, IAmqpConnectionFactory
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="symmetricalEncryption">加密</param>
        /// <param name="appConfig">应用配置</param>
        public AmqpConnectionFactoryBase(ISymmetricalEncryption symmetricalEncryption = null, IAppConfiguration appConfig = null)
            : base(symmetricalEncryption, appConfig)
        {
        }

        /// <summary>
        /// 创建并打开
        /// </summary>
        /// <param name="connectionWrap">连接包装信息</param>
        /// <returns>连接</returns>
        public override IAmqpConnection CreateAndOpen(AmqpConnectionWrapInfo connectionWrap = null)
        {
            if (string.IsNullOrWhiteSpace(connectionWrap.HostId))
            {
                return base.CreateAndOpen(connectionWrap);
            }
            else
            {
                var conn = Create();
                conn.OpenByHostId(connectionWrap.HostId);

                return conn;
            }
        }
    }
}
