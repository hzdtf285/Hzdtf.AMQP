using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Impl.Standard.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard
{
    /// <summary>
    /// AMQP辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpUtil
    {
        /// <summary>
        /// 全局配置读取，默认为AmqpConfigCache
        /// </summary>
        public static IAmqpConfigReader GlobalConfigReader = new AmqpConfigCache();

        /// <summary>
        /// 获取配置读取，如果传入为空，则取全局配置读取
        /// </summary>
        /// <param name="configReader">配置读取</param>
        /// <returns>配置读取</returns>
        public static IAmqpConfigReader GetConfigReader(IAmqpConfigReader configReader = null)
        {
            return configReader == null ? GlobalConfigReader : configReader;
        }
    }
}
