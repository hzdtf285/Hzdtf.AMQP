using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hzdtf.AMQP.Impl.Standard.Config
{
    /// <summary>
    /// AMQP配置缓存
    /// @ 黄振东
    /// </summary>
    public class AmqpConfigCache : IAmqpConfigReader
    {
        /// <summary>
        /// 原生AMQP配置读取
        /// </summary>
        private readonly IAmqpConfigReader protoAmqpConfigReader;

        /// <summary>
        /// AMQP配置信息
        /// </summary>
        private static AmqpConfigInfo configInfo;

        /// <summary>
        /// 同步配置信息
        /// </summary>
        private static readonly object syncConfigInfo = new object();

        /// <summary>
        /// 构造方法
        /// </summary>
        public AmqpConfigCache()
            : this(new AmqpConfigJson())
        { }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="protoAmqpConfigReader">原生AMQP配置读取</param>
        public AmqpConfigCache(IAmqpConfigReader protoAmqpConfigReader)
        {
            this.protoAmqpConfigReader = protoAmqpConfigReader;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns>数据</returns>
        public AmqpConfigInfo Reader()
        {
            if (configInfo == null)
            {
                lock (syncConfigInfo)
                {
                    configInfo = protoAmqpConfigReader.Reader();
                }
            }

            return configInfo;
        }
    }
}
