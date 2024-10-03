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
var channel = connection.CreateModel();

const string message = "hello world";

var body = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(
    exchange: string.Empty,
    routingKey: "hello",
    basicProperties: null,
    body: body
);
Console.WriteLine($" [x] Sent {message}");

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
