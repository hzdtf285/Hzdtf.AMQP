using Hzdtf.AMQP.AspNet.Core;
using Hzdtf.AMQP.Contract.Standard.Connection;
using Hzdtf.AMQP.Impl.Standard;
using Hzdtf.AMQP.Impl.Standard.Config;
using Hzdtf.AMQP.Model.Standard.Connection;
using Hzdtf.RabbitV2.Impl.Standard.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hzdtf.RabbitV2.AspNet.Core
{
    /// <summary>
    /// Rabbit扩展类
    /// @ 黄振东
    /// </summary>
    public static class RabbitExtensions
    {
        #region 连接工厂

        /// <summary>
        /// 添加Rabbit连接工厂
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <returns>AMQP连接工厂</returns>
        public static IAmqpConnectionFactory AddRabbitConnectionFactory(this IServiceCollection services, Action<RabbitConnectionFactoryOptions> options = null)
        {
            return services.AddCustomerRabbitConnectionFactory((RabbitConnectionFactoryCustomerOptions op) =>
            {
                op.ConfigReader = new AmqpConfigCache();
            });
        }

        /// <summary>
        /// 为微软配置添加Rabbit连接工厂
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <param name="beforeConfigurationBuilder">配置生成前回调</param>
        /// <returns>AMQP连接工厂</returns>
        public static IAmqpConnectionFactory AddRabbitConnectionFactoryForConfigurate(this IServiceCollection services, Action<RabbitConnectionFactoryOptions> options = null, Action<IConfigurationBuilder, string, object> beforeConfigurationBuilder = null)
        {
            return services.AddCustomerRabbitConnectionFactory((RabbitConnectionFactoryCustomerOptions op) =>
            {
                op.ConfigReader = new AmqpConfigConfiguration(beforeConfigurationBuilder: beforeConfigurationBuilder);
            });
        }

        /// <summary>
        /// 添加自定义Rabbit连接工厂
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <returns>AMQP连接工厂</returns>
        public static IAmqpConnectionFactory AddCustomerRabbitConnectionFactory(this IServiceCollection services, Action<RabbitConnectionFactoryCustomerOptions> options = null)
        {
            var config = new RabbitConnectionFactoryCustomerOptions();
            if (options != null)
            {
                options(config);
            }
            if (config.ConfigReader == null)
            {
                config.ConfigReader = new AmqpConfigCache();
            }
            AmqpUtil.GlobalConfigReader = config.ConfigReader;

            var factory = new RabbitConnectionFactory(config.SymmetricalEncryption, config.AppConfig, config.ConfigReader);
            services.AddSingleton<IAmqpConnectionFactory>(factory);

            return factory;
        }

        #endregion

        #region 连接

        /// <summary>
        /// 添加Rabbit连接并打开
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="hostId">主机ID</param>
        /// <returns>AMQP连接</returns>
        public static IAmqpConnection AddRabbitConnectionAndOpen(this IServiceCollection services, string hostId)
        {
            return services.AddCustomerRabbitConnectionAndOpen(op =>
            {
                op.HostId = hostId;
            });
        }

        /// <summary>
        /// 为微软配置添加Rabbit连接并打开
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="hostId">主机ID</param>
        /// <param name="beforeConfigurationBuilder">配置生成前回调</param>
        /// <returns>AMQP连接</returns>
        public static IAmqpConnection AddRabbitConnectionAndOpenForConfigurate(this IServiceCollection services, string hostId, Action<IConfigurationBuilder, string, object> beforeConfigurationBuilder = null)
        {
            return services.AddCustomerRabbitConnectionAndOpen((RabbitConnectionOpenCustomerOptions op) =>
            {
                op.HostId = hostId;
                op.ConfigReader = new AmqpConfigConfiguration(beforeConfigurationBuilder: beforeConfigurationBuilder);
            });
        }

        /// <summary>
        /// 为微软配置添加Rabbit连接并打开
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <param name="beforeConfigurationBuilder">配置生成前回调</param>
        /// <returns>AMQP连接</returns>
        public static IAmqpConnection AddRabbitConnectionAndOpenForConfigurate(this IServiceCollection services, Action<RabbitConnectionOpenOptions> options, Action<IConfigurationBuilder, string, object> beforeConfigurationBuilder = null)
        {
            var config = new RabbitConnectionOpenOptions();
            if (options != null)
            {
                options(config);
            }
            return services.AddCustomerRabbitConnectionAndOpen((RabbitConnectionOpenCustomerOptions op) =>
            {
                op.HostId = config.HostId;
                op.ConfigReader = new AmqpConfigConfiguration(beforeConfigurationBuilder: beforeConfigurationBuilder);
            });
        }

        /// <summary>
        /// 添加自定义Rabbit连接并打开
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <returns>AMQP连接</returns>
        public static IAmqpConnection AddCustomerRabbitConnectionAndOpen(this IServiceCollection services, Action<RabbitConnectionOpenCustomerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("配置回调不能为null");
            }
            var config = new RabbitConnectionOpenCustomerOptions();
            options(config);
            if (string.IsNullOrWhiteSpace(config.HostId))
            {
                throw new ArgumentException("主机ID不能为空");
            }
            if (config.ConfigReader == null)
            {
                config.ConfigReader = new AmqpConfigCache();
            }
            AmqpUtil.GlobalConfigReader = config.ConfigReader;

            var factory = services.AddCustomerRabbitConnectionFactory((op) =>
            {
                op.AppConfig = config.AppConfig;
                op.ConfigReader = config.ConfigReader;
                op.SymmetricalEncryption = config.SymmetricalEncryption;
            });

            var conn = factory.CreateAndOpen(new AmqpConnectionWrapInfo()
            {
                HostId = config.HostId
            });
            services.AddSingleton<IAmqpConnection>(conn);

            return conn;
        }

        #endregion
    }
}
