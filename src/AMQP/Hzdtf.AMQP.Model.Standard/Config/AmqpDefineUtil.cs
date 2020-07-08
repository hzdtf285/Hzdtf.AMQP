using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Config
{
    /// <summary>
    /// AMQP定义辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpDefineUtil
    {
        /// <summary>
        /// 直通交换机名称
        /// </summary>
        public const string DIRECT_EXCHANGE_NAME = "direct";

        /// <summary>
        /// 广播交换机名称
        /// </summary>
        public const string FANOUT_EXCHANGE_NAME = "fanout";

        /// <summary>
        /// 主题交换机名称
        /// </summary>
        public const string TOPIC_EXCHANGE_NAME = "topic";

        /// <summary>
        /// 默认直通交换机
        /// </summary>
        public const string DEFAULT_DIRECT_EXCHANGE = "amq.direct";

        /// <summary>
        /// 默认广播交换机
        /// </summary>
        public const string DEFAULT_FANOUT_EXCHANGE = "amq.fanout";

        /// <summary>
        /// 默认主题交换机
        /// </summary>
        public const string DEFAULT_TOPIC_EXCHANGE = "amq.topic";

        /// <summary>
        /// 默认虚拟路径
        /// </summary>
        public const string DEFAULT_VIRTUAL_PATH = "/";
    }
}
