using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.Utility.Standard.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Standard.Config
{
    /// <summary>
    /// AMQP配置读取接口
    /// @ 黄振东
    /// </summary>
    public interface IAmqpConfigReader : IReader<AmqpConfigInfo>
    {
    }
}
