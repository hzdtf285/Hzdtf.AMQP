using Hzdtf.AMQP.Model.Standard.BusinessException;
using Hzdtf.RabbitV2.Impl.Standard.Connection;
using System;

namespace Hzdtf.RabbitV2.ExceptionHandleExample.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("exhost");
            var consumer = conn.CreateConsumer("ExChange", "ExQueue");

            Console.WriteLine("监听业务异常信息：");

            consumer.Subscribe((BusinessExceptionInfo msg) =>
            {
                Console.WriteLine(msg.ToString());

                return true;
            });

            Console.Read();
        }
    }
}
