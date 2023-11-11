using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FormulaAirlline.API.Services
{
    public class MessageProducer : IMessageProducer
    {
        public void SendingMessage<T>(T message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 14859,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                "bookings",
                durable: true,
                exclusive: false, //默认是false，如果为true， 只能发送一条消息？
                autoDelete: false,
                arguments: null
            );

            var jsonStr = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(jsonStr);

            channel.BasicPublish("", "bookings", body: body);
        }
    }
}
