using Microsoft.Extensions.Configuration;
using NetCore3_RebbitMqReceive.IRepositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore3_RebbitMqReceive.Repositories
{
    public class RabbitMq : IRabbitMq
    {
        private readonly IConfiguration Configuration;
        private readonly ConnectionFactory _factory;
        public RabbitMq(IConfiguration configuration)
        {
            Configuration = configuration;
            _factory = new ConnectionFactory() { HostName = Configuration.GetSection("RabbitMqSetting:HostName").Value, UserName = Configuration.GetSection("RabbitMqSetting:UserName").Value, Password = Configuration.GetSection("RabbitMqSetting:Password").Value, VirtualHost = Configuration.GetSection("RabbitMqSetting:VirtualHost").Value };
        }

        public bool QueueBind(string queuename, string exchange, string routekey)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queuename,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                    channel.QueueBind(queuename, exchange, routekey);
                    Console.WriteLine($"{queuename} declared. ");
                    return true;
                }
            }
        }


        public bool ExchangeSend(string exchange, string routingKey, string msg)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange, ExchangeType.Topic, true, false);

                    string message = $"{msg}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: exchange,
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine($"Sent {exchange} {msg}");
                    return true;
                }
            }
        }

        public void QueueReceive(string queuename)
        {
            var connection = _factory.CreateConnection();

            var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };
            channel.BasicConsume(queue: queuename,
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
