using Hzdtf.AMQP.Impl.Standard.Connection.Connection;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Connection
{
    /// <summary>
    /// AMQP连接辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpConnectionUtil
    {
        /// <summary>
        /// 默认连接字符串解析
        /// </summary>
        public readonly static IConnectionStringParse<AmqpConnectionInfo> DefaultConnectionStringParse = new AmqpConnectionStringParse();
    }
}
