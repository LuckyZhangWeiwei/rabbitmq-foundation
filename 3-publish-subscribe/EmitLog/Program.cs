// https://www.rabbitmq.com/tutorials/tutorial-three-dotnet

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

// 生成随机队列名称
var queueName = "7492CF66-0A10-EA1B-B2F5-8E1D9F016EAD"; //channel.QueueDeclare().QueueName;

//使用fanout exchange type，指定exchange名称
channel.ExchangeDeclare(exchange: "logs", type: "fanout", durable: true);

channel.QueueDeclare(
    queue: queueName,
    durable: true, // 持久化队列
    exclusive: false,
    autoDelete: true,
    arguments: null
);

// 绑定队列到 fanout 交换器
channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");

//将消息标记为持久性 - 将IBasicProperties.SetPersistent设置为true
var properties = channel.CreateBasicProperties();
properties.Persistent = true;

var message = GetMessage(args);
var body = Encoding.UTF8.GetBytes(message);

//发布到指定exchange，fanout类型无需指定routingKey
channel.BasicPublish(exchange: "logs", routingKey: "", basicProperties: properties, body: body);

Console.WriteLine(" [x] Sent {0}", message);

string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello RabbitMQ!");
}
