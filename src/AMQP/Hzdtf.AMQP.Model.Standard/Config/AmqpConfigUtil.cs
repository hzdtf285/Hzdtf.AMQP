using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Model.Standard.Config
{
    /// <summary>
    /// AMQP配置辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpConfigUtil
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="basicConnectionString">基本连接字符串</param>
        /// <param name="virtualPath">虚拟路径</param>
        /// <returns>连接字符串</returns>
        public static string GetConnectionString(string basicConnectionString, string virtualPath = AmqpDefineUtil.DEFAULT_VIRTUAL_PATH)
        {
            if (basicConnectionString.Contains("virtualPath="))
            {
                return basicConnectionString;
            }

            string split = basicConnectionString.EndsWith(";") ? null : ";";

            return $"{basicConnectionString}{split}virtualPath={virtualPath}";
        }
    }
}
