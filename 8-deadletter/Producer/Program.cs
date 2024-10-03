//https://chatgpt.com/share/66fe6c96-0c54-8013-9665-e508ef1ec0be

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

// 定义死信交换器
channel.ExchangeDeclare(exchange: "dlx_exchange", type: "fanout", durable: true);

// 定义主交换器 (fanout类型)
channel.ExchangeDeclare(exchange: "main_exchange", type: "fanout", durable: true);

// 定义主队列，并指定它的死信交换器
var arguments = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dlx_exchange" } // 死信交换器的配置
};

channel.QueueDeclare(
    queue: "main_queue",
    durable: true,
    exclusive: false,
    autoDelete: true,
    arguments: arguments
);

// 将主队列绑定到主交换器
channel.QueueBind(queue: "main_queue", exchange: "main_exchange", routingKey: "");

// 定义消息
var message = "Message for DLX testing!";
var body = Encoding.UTF8.GetBytes(message);

// 设置消息持久化
var properties = channel.CreateBasicProperties();
properties.Persistent = true;

// 发布消息到主交换器
channel.BasicPublish(exchange: "main_exchange", routingKey: "", basicProperties: null, body: body);

Console.WriteLine(" [x] Sent '{0}'", message);
