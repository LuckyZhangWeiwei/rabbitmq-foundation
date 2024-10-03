//1.创建基于本地的连接工厂
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

//2. 建立连接
using var connection = factory.CreateConnection();

//3. 创建频道
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "direct_logs", type: "direct", durable: true);

var queueName = "7492CF66-0A10-EA1B-B2F5-8E1D9F016EAF";

//设置prefetchCount : 1来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时，不再分配任务。
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

if (args.Length < 1)
{
    Console.Error.WriteLine("Use one of parameters: [info] [warning] [error]");
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.Exit(1);
    return;
}

string queuename = "";

foreach (var logLevel in args)
{
    queuename = logLevel + "-" + queueName;

    channel.QueueBind(
        queue: logLevel + "-" + queueName,
        exchange: "direct_logs",
        routingKey: logLevel
    );
}

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);

    channel.BasicAck(ea.DeliveryTag, false);
};

channel.BasicConsume(queue: queuename, autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
