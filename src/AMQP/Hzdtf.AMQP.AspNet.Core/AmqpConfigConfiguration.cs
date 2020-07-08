using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Utility.AspNet.Core.Config;
using Microsoft.Extensions.Configuration;
using System;

namespace Hzdtf.AMQP.AspNet.Core
{
    /// <summary>
    /// AMQP配置来自微软配置对象里
    /// @ 黄振东
    /// </summary>
    public class AmqpConfigConfiguration : JsonFileMicrosoftConfigurationBase<AmqpConfigInfo>, IAmqpConfigReader
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="jsonFile">json文件</param>
        /// <param name="beforeConfigurationBuilder">配置生成前回调</param>
        public AmqpConfigConfiguration(string jsonFile = "amqp.json", Action<IConfigurationBuilder, string, object> beforeConfigurationBuilder = null)
            : base(jsonFile, beforeConfigurationBuilder)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="options">配置</param>
        /// <param name="beforeConfigurationBuilder">配置生成前回调</param>
        public AmqpConfigConfiguration(AmqpConfigInfo options, Action<IConfigurationBuilder, string, object> beforeConfigurationBuilder = null)
            : base(options, beforeConfigurationBuilder)
        {
        }
    }
}
