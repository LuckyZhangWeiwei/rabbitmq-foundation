//https://www.rabbitmq.com/tutorials/tutorial-four-dotnet

using System.Text;
using RabbitMQ.Client;

//1.创建基于本地的连接工厂
var factory = new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
};

//2. 建立连接
using var connection = factory.CreateConnection();

//3. 创建频道
using var channel = connection.CreateModel();

var queueName = "7492CF66-0A10-EA1B-B2F5-8E1D9F016EAF";

var logLevel = (args.Length > 0) ? args[0] : "info";

channel.ExchangeDeclare(exchange: "direct_logs", type: "direct", durable: true);

channel.QueueDeclare(
    queue: logLevel + "-" + queueName,
    durable: true, // 持久化队列
    exclusive: false,
    autoDelete: true,
    arguments: null
);

Console.WriteLine("loglevel:" + logLevel);

// 绑定队列到 direct 交换器
channel.QueueBind(queue: logLevel + "-" + queueName, exchange: "direct_logs", routingKey: logLevel);

//将消息标记为持久性 - 将IBasicProperties.SetPersistent设置为true
var properties = channel.CreateBasicProperties();
properties.Persistent = true;

var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello RabbitMQ!";

var body = Encoding.UTF8.GetBytes(message);

//发布到指定exchange，direct类型一定要指定routingKey
channel.BasicPublish(
    exchange: "direct_logs",
    routingKey: logLevel,
    basicProperties: null,
    body: body
);

Console.WriteLine(" [x] Sent '{0}':'{1}'", logLevel, message);
