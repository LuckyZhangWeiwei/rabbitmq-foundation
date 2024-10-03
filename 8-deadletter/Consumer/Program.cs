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

// 消费主队列的消息
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine(" [x] main consumer Received '{0}'", message);

    // 拒绝消息，模拟消息无法处理的情况，将其发送到死信队列
    channel.BasicReject(ea.DeliveryTag, false); // false 表示不重新入队
};

// 消费消息
channel.BasicConsume(queue: "main_queue", autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
