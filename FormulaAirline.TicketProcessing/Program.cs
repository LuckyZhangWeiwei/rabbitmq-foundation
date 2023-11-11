// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("welcome to the ticketing service");

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
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    var body = args.Body.ToArray();

    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"new ticket processing is initiation: {message}");
};

channel.BasicConsume("bookings", true, consumer);

//Console.ReadLine();
