using Hzdtf.AMQP.Contract.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Utility.Standard.Attr;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Config
{
    /// <summary>
    /// AMQP配置JSON文件
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class AmqpConfigJson : IAmqpConfigReader
    {
        /// <summary>
        /// 配置JSON文件
        /// </summary>
        private readonly string configJsonFile;

        /// <summary>
        /// 构造方法，默认读取amqp.json
        /// </summary>
        public AmqpConfigJson()
            : this("amqp.json")
        { }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="configJsonFile">配置JSON文件</param>
        public AmqpConfigJson(string configJsonFile) 
        {
            this.configJsonFile = configJsonFile;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns>数据</returns>
        public AmqpConfigInfo Reader()
        {
            return JsonUtil.DeserializeFromFile<AmqpConfigInfo>(configJsonFile);
        }
    }
}
