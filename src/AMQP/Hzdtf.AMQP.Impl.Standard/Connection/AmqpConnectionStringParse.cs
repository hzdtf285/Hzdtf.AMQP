using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Connection.Connection
{
    /// <summary>
    /// AMQP连接字符串解析器
    /// @ 黄振东
    /// </summary>
    public class AmqpConnectionStringParse : ConnectionStringParseBase<AmqpConnectionInfo>, IConnectionStringParse<AmqpConnectionInfo>
    {
        #region 重写父类的方法

        /// <summary>
        /// 设置独特的值
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        /// <param name="name">名称</param>
        /// <param name="value">值</param>
        protected override void SetOnlyHaveValue(AmqpConnectionInfo connectionInfo, string name, string value)
        {
            switch (name)
            {
                case "virtualPath":
                    connectionInfo.VirtualPath = value;

                    break;

                case "autoRecovery":
                    connectionInfo.AutoRecovery = Convert.ToBoolean(value);

                    break;

                case "heartbeat":
                    connectionInfo.Heartbeat = Convert.ToUInt16(value);

                    break;
            }
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        protected override void SetDefaultValue(AmqpConnectionInfo connectionInfo)
        {
            if (string.IsNullOrWhiteSpace(connectionInfo.VirtualPath))
            {
                connectionInfo.VirtualPath = AmqpDefineUtil.DEFAULT_VIRTUAL_PATH;
            }
        }

        /// <summary>
        /// 验证独特的参数集合，如果不通过则抛出对应异常
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        protected override void ValidateOnlyHaveParams(AmqpConnectionInfo connectionInfo)
        {
            ValidateUtil.ValidateNullOrEmpty(connectionInfo.VirtualPath, "虚拟路径");
        }

        /// <summary>
        /// 创建连接信息
        /// </summary>
        /// <returns>连接信息</returns>
        protected override AmqpConnectionInfo CreateConnectionInfo()
        {
            return new AmqpConnectionInfo();
        }

        #endregion
    }
}
