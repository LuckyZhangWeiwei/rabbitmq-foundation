using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Consumer
{
    public static void Main(string[] args)
    {
        // 创建与生产者相同的连接工厂
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        // 创建连接
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // 假设你能获取到生产者动态生成的 queueName
        // 这里可以直接指定从生产者返回的 queueName
        var queueName = "cdab77dd-6b8a-49ec-87ec-cbf2c75a58a1"; // 注意：你需要从生产者获取这个队列名

        // 声明队列（如果消费者和生产者不在同一个生命周期内，你可以再声明一次队列）
        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        //设置prefetchCount : 1来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时，不再分配任务。
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        Console.WriteLine($" [*] Waiting for messages from queue: {queueName}");

        // 创建消费者对象
        var consumer = new EventingBasicConsumer(channel);

        // 消息处理事件
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
            Thread.Sleep(200);
            //channel.BasicReject(deliveryTag: ea.DeliveryTag, false);
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            //channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        };

        // 消费队列中的消息
        //channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
