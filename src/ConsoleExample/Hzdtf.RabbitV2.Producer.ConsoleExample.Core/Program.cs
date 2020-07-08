using Hzdtf.RabbitV2.Impl.Standard.Connection;
using System;

namespace Hzdtf.RabbitV2.Producer.ConsoleExample.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("host1");
            var producer = conn.CreateProducer("TestExchange");
            Console.WriteLine("请输入要生产的路由键:");
            var key = Console.ReadLine();
            Console.WriteLine("请输入要发送的消息:");
            while (true)
            {
                var msg = Console.ReadLine();
                if ("exit".Equals(msg))
                {
                    break;
                }

                producer.Publish(msg, key);
            }
        }
    }
}
