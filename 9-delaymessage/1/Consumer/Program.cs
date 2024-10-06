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

EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    var message = Encoding.UTF8.GetString(args.Body.ToArray());
    //打印消费的消息
    Console.WriteLine(message);
    channel.BasicAck(args.DeliveryTag, false);
};

//消费queue.business.test队列的消息
channel.BasicConsume(queue: "queue.business.test", autoAck: false, consumer: consumer);

Console.ReadKey();
channel.Dispose();
connection.Close();
