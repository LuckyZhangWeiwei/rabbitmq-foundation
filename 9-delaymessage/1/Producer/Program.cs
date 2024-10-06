//https://blog.csdn.net/MDZZ666/article/details/120811287

using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

Dictionary<string, object> queueArgs = new Dictionary<string, object>()
{
    { "x-dead-letter-exchange", "exchange.business.test" },
    { "x-dead-letter-routing-key", "businessRoutingkey" }
};

//延时的交换机和队列绑定 ------------------
channel.ExchangeDeclare(
    exchange: "exchange.business.dlx",
    type: "direct",
    durable: true,
    autoDelete: false,
    arguments: null
);

channel.QueueDeclare(
    queue: "queue.business.dlx",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: queueArgs
);

channel.QueueBind(queue: "queue.business.dlx", exchange: "exchange.business.dlx", routingKey: "");

// ---------------------------------

//业务的交换机和队列绑定 (真正的死信队列)----------------
channel.ExchangeDeclare(
    exchange: "exchange.business.test",
    type: "direct",
    durable: true,
    autoDelete: false,
    arguments: null
);

channel.QueueDeclare(
    queue: "queue.business.test",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: queueArgs
);

channel.QueueBind(
    queue: "queue.business.test",
    exchange: "exchange.business.test",
    routingKey: "businessRoutingkey",
    arguments: null
);

// -----------------------------------
Console.WriteLine("生产者开始发送消息");

while (true)
{
    string message = Console.ReadLine();
    var body = Encoding.UTF8.GetBytes(message!);

    // 设置消息持久化
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;
    properties.Expiration = "5000";

    //发送一条延时5秒的消息
    channel.BasicPublish(
        exchange: "exchange.business.dlx",
        routingKey: "",
        basicProperties: properties,
        body: body
    );
}
