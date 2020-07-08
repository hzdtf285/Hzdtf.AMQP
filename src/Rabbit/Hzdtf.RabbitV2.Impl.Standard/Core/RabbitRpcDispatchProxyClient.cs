using Hzdtf.RabbitV2.Impl.Standard.Connection;
using Hzdtf.Utility.Standard.Attr;
using Hzdtf.Utility.Standard.ProcessCall;
using Hzdtf.Utility.Standard.Proxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hzdtf.RabbitV2.Impl.Standard.Core
{
    /// <summary>
    /// Rabbit RPC动态代理客户端
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class RabbitRpcDispatchProxyClient : RpcDispatchProxyClient<RabbitRpcDispatchProxyClient>
    {
        /// <summary>
        /// 创建默认Rpc客户端方法
        /// </summary>
        /// <returns>Rpc客户端方法</returns>
        protected override IRpcClientMethod CreateRpcClientMethod() => new RabbitRpcClientMethod();
    }
}
