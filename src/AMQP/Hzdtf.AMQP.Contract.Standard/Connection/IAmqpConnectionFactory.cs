using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Standard.Connection
{
    /// <summary>
    /// AMQP连接工厂接口
    /// @ 黄振东
    /// </summary>
    public interface IAmqpConnectionFactory : IConnectionFactory<IAmqpConnection, AmqpConnectionInfo, AmqpConnectionWrapInfo>
    {
    }
}
