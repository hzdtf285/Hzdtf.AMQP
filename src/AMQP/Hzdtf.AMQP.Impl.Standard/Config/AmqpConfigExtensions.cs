using Hzdtf.AMQP.Impl.Standard.Connection;
using Hzdtf.AMQP.Model.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.Utility.Standard.Connection;
using Hzdtf.Utility.Standard.Safety;
using Hzdtf.Utility.Standard.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hzdtf.AMQP.Contract.Standard.Config
{
    /// <summary>
    /// AMQP配置扩展类
    /// @ 黄振东
    /// </summary>
    public static class AmqpConfigExtensions
    {
        /// <summary>
        /// 转换为AMQP连接信息
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="hostId">主机ID</param>
        /// <param name="connectionStringParse">连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>AMQP连接信息</returns>
        public static AmqpConnectionInfo ToAmqpConnectionInfo(this AmqpConfigInfo amqpConfig, string hostId, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse = null, ISymmetricalEncryption symmetricalEncryption = null)
        {
            if (amqpConfig == null || string.IsNullOrWhiteSpace(hostId) || amqpConfig.Amqp.IsNullOrLength0())
            {
                return null;
            }
            if (connectionStringParse == null)
            {
                connectionStringParse = AmqpConnectionUtil.DefaultConnectionStringParse;
            }

            var amqp = amqpConfig.Amqp.Where(p => p.HostId == hostId).FirstOrDefault();
            if (amqp == null)
            {
                return null;
            }

            string connString = null;
            if (amqp.ConnectionEncrypt)
            {
                symmetricalEncryption = SymmetricalEncryptionUtil.GetSymmetricalEncryption(symmetricalEncryption);
                connString = symmetricalEncryption.Decrypt(amqp.ConnectionString);
            }
            else
            {
                connString = amqp.ConnectionString;
            }

            return connectionStringParse.Parse(connString);
        }

        /// <summary>
        /// 默认取第1个转换为AMQP连接信息
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="connectionStringParse">连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>AMQP连接信息</returns>
        public static AmqpConnectionInfo ToFristAmqpConnectionInfo(this AmqpConfigInfo amqpConfig, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse = null, ISymmetricalEncryption symmetricalEncryption = null)
        {
            if (amqpConfig == null || amqpConfig.Amqp.IsNullOrLength0())
            {
                return null;
            }
            if (connectionStringParse == null)
            {
                connectionStringParse = AmqpConnectionUtil.DefaultConnectionStringParse;
            }

            var amqp = amqpConfig.Amqp[0];
            if (amqp == null)
            {
                return null;
            }

            string connString = null;
            if (amqp.ConnectionEncrypt)
            {
                symmetricalEncryption = SymmetricalEncryptionUtil.GetSymmetricalEncryption(symmetricalEncryption);
                connString = symmetricalEncryption.Decrypt(amqp.ConnectionString);
            }
            else
            {
                connString = amqp.ConnectionString;
            }

            return connectionStringParse.Parse(connString);
        }

        /// <summary>
        /// 默认取第1个转换为连接字符串
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>连接字符串</returns>
        public static string ToFristConnectionString(this AmqpConfigInfo amqpConfig, ISymmetricalEncryption symmetricalEncryption = null)
        {
            if (amqpConfig == null || amqpConfig.Amqp.IsNullOrLength0())
            {
                return null;
            }

            var amqp = amqpConfig.Amqp[0];
            if (amqp == null)
            {
                return null;
            }

            string connString = null;
            if (amqp.ConnectionEncrypt)
            {
                symmetricalEncryption = SymmetricalEncryptionUtil.GetSymmetricalEncryption(symmetricalEncryption);
                connString = symmetricalEncryption.Decrypt(amqp.ConnectionString);
            }
            else
            {
                connString = amqp.ConnectionString;
            }

            return AmqpConfigUtil.GetConnectionString(connString);
        }

        /// <summary>
        /// 将AMQP连接转换为交换机信息数组
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="amqpConnection">AMQP连接</param>
        /// <param name="connectionStringParse">连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>交换机信息数组</returns>
        public static ExchangeInfo[] ToExchanges(this AmqpConfigInfo amqpConfig, AmqpConnectionInfo amqpConnection, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse = null, ISymmetricalEncryption symmetricalEncryption = null)
        {
            string hostId;

            return ToExchanges(amqpConfig, amqpConnection, out hostId, connectionStringParse, symmetricalEncryption);
        }

        /// <summary>
        /// 将AMQP连接转换为交换机信息数组
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="amqpConnection">AMQP连接</param>
        /// <param name="hostId">主机ID</param>
        /// <param name="connectionStringParse">连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>交换机信息数组</returns>
        public static ExchangeInfo[] ToExchanges(this AmqpConfigInfo amqpConfig, AmqpConnectionInfo amqpConnection, out string hostId, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse = null, ISymmetricalEncryption symmetricalEncryption = null)
        {
            hostId = null;
            if (amqpConfig == null || amqpConfig.Amqp.IsNullOrLength0() || amqpConnection == null || string.IsNullOrWhiteSpace(amqpConnection.VirtualPath))
            {
                return null;
            }
            if (connectionStringParse == null)
            {
                connectionStringParse = AmqpConnectionUtil.DefaultConnectionStringParse;
            }

            symmetricalEncryption = SymmetricalEncryptionUtil.GetSymmetricalEncryption(symmetricalEncryption);

            foreach (var r in amqpConfig.Amqp)
            {
                if (string.IsNullOrWhiteSpace(r.ConnectionString))
                {
                    continue;
                }

                string connString = r.ConnectionEncrypt ? symmetricalEncryption.Decrypt(r.ConnectionString) : r.ConnectionString;
                var conn = connectionStringParse.Parse(connString);
                if (conn.Host.Equals(amqpConnection.Host) && conn.Port == amqpConnection.Port && conn.VirtualPath.Equals(amqpConnection.VirtualPath))
                {
                    hostId = r.HostId;

                    return r.Exchanges;
                }
            }

            return null;
        }

        /// <summary>
        /// 将AMQP连接转换为主机ID
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="amqpConnection">AMQP连接</param>
        /// <param name="connectionStringParse">连接字符串解析</param>
        /// <param name="symmetricalEncryption">加密</param>
        /// <returns>主机ID</returns>
        public static string ToHostId(this AmqpConfigInfo amqpConfig, AmqpConnectionInfo amqpConnection, IConnectionStringParse<AmqpConnectionInfo> connectionStringParse = null, ISymmetricalEncryption symmetricalEncryption = null)
        {
            string hostId;

            ToExchanges(amqpConfig, amqpConnection, out hostId, connectionStringParse, symmetricalEncryption);

            return hostId;
        }

        /// <summary>
        /// 将主机ID转换为交换机信息数组
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="hostId">主机ID</param>
        /// <returns>交换机信息数组</returns>
        public static ExchangeInfo[] ToExchanges(this AmqpConfigInfo amqpConfig, string hostId)
        {
            if (amqpConfig == null || amqpConfig.Amqp.IsNullOrLength0() || string.IsNullOrWhiteSpace(hostId))
            {
                return null;
            }

            var r = amqpConfig.Amqp.Where(p => p.HostId == hostId).FirstOrDefault();

            return r != null ? r.Exchanges : null;
        }

        /// <summary>
        /// 将RPC客户端程序集转换为RPC客户端程序集队列信息
        /// </summary>
        /// <param name="amqpConfig">AMQP配置信息</param>
        /// <param name="rpcClientAssembly">RPC客户端程序集</param>
        /// <returns>RPC客户端程序集队列信息</returns>
        public static RpcClientAssemblyQueueInfo ToRpcClientAssemblyQueue(this AmqpConfigInfo amqpConfig, string rpcClientAssembly)
        {
            if (amqpConfig == null || amqpConfig.Amqp.IsNullOrLength0() || string.IsNullOrWhiteSpace(rpcClientAssembly))
            {
                return null;
            }

            foreach (var a in amqpConfig.Amqp)
            {
                if (a.Exchanges.IsNullOrLength0())
                {
                    continue;
                }

                foreach (var e in a.Exchanges)
                {
                    if (e.Queues.IsNullOrLength0())
                    {
                        continue;
                    }

                    foreach (var q in e.Queues)
                    {
                        if (q.RpcClientAssemblys.IsNullOrLength0())
                        {
                            continue;
                        }

                        var assemblys = q.RpcClientAssemblys.Where(p => p.Name == rpcClientAssembly).FirstOrDefault();
                        if (assemblys == null)
                        {
                            continue;
                        }

                        return new RpcClientAssemblyQueueInfo()
                        {
                            HostId = a.HostId,
                            ExchangeName = e.Name,
                            QueueName = q.Name,
                            RpcClientAssembly = assemblys
                        };
                    }
                }
            }

            return null;
        }
    }
}
