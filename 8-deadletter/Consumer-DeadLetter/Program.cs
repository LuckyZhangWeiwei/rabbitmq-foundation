using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

// 定义死信队列，并绑定到死信交换器
channel.QueueDeclare(
    queue: "dlx_queue",
    durable: true,
    exclusive: false,
    autoDelete: true,
    arguments: null
);

channel.QueueBind(queue: "dlx_queue", exchange: "dlx_exchange", routingKey: "");

// 消费死信队列中的消息
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(" [x] DLX Received '{0}'", message);
};

channel.BasicConsume(queue: "dlx_queue", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
