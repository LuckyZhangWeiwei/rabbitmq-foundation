using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory connectionFactory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};

//创建连接
var connection = connectionFactory.CreateConnection();

var channel = connection.CreateModel();

EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

consumer.Received += (obj, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine(message);
    channel.BasicAck(ea.DeliveryTag, false);
};

channel.BasicConsume("plug.delay.queue", false, consumer);

Console.ReadKey();
channel.Dispose();
connection.Close();
