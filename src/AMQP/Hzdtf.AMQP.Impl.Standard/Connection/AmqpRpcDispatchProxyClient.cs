using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Proxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Standard.Connection
{
    /// <summary>
    /// AMQP RPC动态代理客户端
    /// @ 黄振东
    /// </summary>
    public class AmqpRpcDispatchProxyClient : RpcDispatchProxyClient<AmqpRpcDispatchProxyClient>
    {
        /// <summary>
        /// 创建默认Rpc客户端方法
        /// </summary>
        /// <returns>Rpc客户端方法</returns>
        protected override IRpcClientMethod CreateRpcClientMethod() => null;
    }
}
