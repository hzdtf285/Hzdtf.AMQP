using Hzdtf.AMQP.Model.Standard.BusinessException;
using Hzdtf.RabbitV2.Impl.Standard.Connection;
using System;

namespace Hzdtf.RabbitV2.BusinessExceptionHandle.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("这里是接收MQ业务异常处理：");

            var conn = new RabbitConnection();
            conn.OpenByHostId("exhost");
            var consumer = conn.CreateConsumer("ExChange", "ExQueue");
            consumer.Subscribe((BusinessExceptionInfo busEx) =>
            {
                Console.WriteLine("接收到异常数据：" + busEx);

                return true;
            });

            Console.Read();
        }
    }
}
