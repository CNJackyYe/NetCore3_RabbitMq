using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using System.Reflection;
using NetCore3_RebbitMqReceive.Utilities;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NetCore3_RebbitMqReceive.IRepositories;
using NetCore3_RebbitMqReceive.Repositories;

namespace NetCore3_RebbitMqReceive
{
    public class Startup
    {
        public readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IRabbitMq,RabbitMq>();

            QueueReceive("MqReceive_Receive");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void QueueReceive(string queuename)
        {
            var _factory = new ConnectionFactory() { HostName = Configuration.GetSection("RabbitMqSetting:HostName").Value, UserName = Configuration.GetSection("RabbitMqSetting:UserName").Value, Password = Configuration.GetSection("RabbitMqSetting:Password").Value, VirtualHost = Configuration.GetSection("RabbitMqSetting:VirtualHost").Value };

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
