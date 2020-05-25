using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCore3_RebbitMqReceive.IRepositories
{
    public interface IRabbitMq
    {
        bool QueueBind(string queuename, string exchange, string routekey);
        bool ExchangeSend(string exchange, string routingKey, string msg);
        void QueueReceive(string queuename);
    }
}
