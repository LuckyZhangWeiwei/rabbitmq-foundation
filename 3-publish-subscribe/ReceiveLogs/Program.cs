using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory()
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

//申明exchange
channel.ExchangeDeclare(exchange: "logs", type: "fanout", durable: true);

//设置prefetchCount : 1来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时，不再分配任务。
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

//申明随机队列名称
var queuename = "7492CF66-0A10-EA1B-B2F5-8E1D9F016EAD";

//绑定队列到指定exchange,使用默认路由
channel.QueueBind(queue: queuename, exchange: "logs", routingKey: "");
Console.WriteLine("[*] Waitting for logs.");

//申明consumer
var consumer = new EventingBasicConsumer(channel);

//绑定消息接收后的事件委托
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine("[x] {0}", message);

    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: queuename, autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
