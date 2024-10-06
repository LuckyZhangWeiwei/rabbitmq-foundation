//https: //blog.csdn.net/MDZZ666/article/details/120811287

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

Dictionary<string, object> exchangeArgs = new Dictionary<string, object>()
{
    { "x-delayed-type", "direct" }
};

//指定x-delayed-message 类型的交换机，并且添加x-delayed-type属性
channel.ExchangeDeclare(
    exchange: "plug.delay.exchange",
    type: "x-delayed-message",
    durable: true,
    autoDelete: false,
    arguments: exchangeArgs
);

channel.QueueDeclare(
    queue: "plug.delay.queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

channel.QueueBind(
    queue: "plug.delay.queue",
    exchange: "plug.delay.exchange",
    routingKey: "plugdelay"
);

var properties = channel.CreateBasicProperties();
Dictionary<string, object> headers = new Dictionary<string, object>() { { "x-delay", "5000" } };
properties.Persistent = true;
properties.Headers = headers;

Console.WriteLine("生产者开始发送消息");

while (true)
{
    string message = Console.ReadLine();
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish("plug.delay.exchange", "plugdelay", properties, body);
}
