// https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet


//https://www.rabbitmq.com/docs/confirms

//https://github.com/sheng-jie/RabbitMQ

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

const int MESSAGE_COUNT = 50000;

//PublishMessagesIndividually();

//PublishMessagesInBatch();

await HandlePublishConfirmsAsynchronously();

static void PublishMessagesIndividually()
{
    using var connection = CreateConnection();
    using var channel = connection.CreateModel();

    // declare a server-named queue
    //var queueName = channel.QueueDeclare().QueueName;
    var queueName = "cdab77dd-6b8a-49ec-87ec-cbf2c75a58a1";
    channel.ConfirmSelect();

    var startTime = Stopwatch.GetTimestamp();

    for (int i = 0; i < MESSAGE_COUNT; i++)
    {
        var body = Encoding.UTF8.GetBytes(i.ToString());
        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: body
        );
        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
    }

    var endTime = Stopwatch.GetTimestamp();

    Console.WriteLine(
        $"Published {MESSAGE_COUNT:N0} messages individually in {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms"
    );
}

static void PublishMessagesInBatch()
{
    using var connection = CreateConnection();
    using var channel = connection.CreateModel();

    // declare a server-named queue
    //var queueName = channel.QueueDeclare().QueueName;
    var queueName = "cdab77dd-6b8a-49ec-87ec-cbf2c75a58a1";
    channel.ConfirmSelect();

    var batchSize = 100;
    var outstandingMessageCount = 0;

    var startTime = Stopwatch.GetTimestamp();

    for (int i = 0; i < MESSAGE_COUNT; i++)
    {
        var body = Encoding.UTF8.GetBytes(i.ToString());
        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: body
        );
        outstandingMessageCount++;

        if (outstandingMessageCount == batchSize)
        {
            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
            outstandingMessageCount = 0;
        }
    }

    if (outstandingMessageCount > 0)
        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

    var endTime = Stopwatch.GetTimestamp();

    Console.WriteLine(
        $"Published {MESSAGE_COUNT:N0} messages in batch in {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms"
    );
}

static async Task HandlePublishConfirmsAsynchronously()
{
    using var connection = CreateConnection();
    using var channel = connection.CreateModel();

    // declare a server-named queue
    //var queueName = channel.QueueDeclare().QueueName;
    var queueName = "cdab77dd-6b8a-49ec-87ec-cbf2c75a58a1";
    channel.ConfirmSelect();

    channel.QueueDeclare(
        queue: queueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    //将消息标记为持久性 - 将IBasicProperties.SetPersistent设置为true
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;

    var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

    void CleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
    {
        if (multiple)
        {
            var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
            foreach (var entry in confirmed)
                outstandingConfirms.TryRemove(entry.Key, out _);
        }
        else
            outstandingConfirms.TryRemove(sequenceNumber, out _);
    }

    channel.BasicAcks += (sender, ea) => CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
    channel.BasicNacks += (sender, ea) =>
    {
        outstandingConfirms.TryGetValue(ea.DeliveryTag, out string? body);
        Console.WriteLine(
            $"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}"
        );
        CleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
    };

    var startTime = Stopwatch.GetTimestamp();

    for (int i = 0; i < MESSAGE_COUNT; i++)
    {
        var body = i.ToString();

        var publishNo = channel.NextPublishSeqNo;
        //Console.WriteLine($"publishNo: {publishNo}");

        outstandingConfirms.TryAdd(publishNo, i.ToString());

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: Encoding.UTF8.GetBytes(body)
        );
    }
    if (!await WaitUntil(60, () => outstandingConfirms.IsEmpty))
        throw new Exception(
            $"All messages could not be confirmed in 60 seconds - outstandingConfirmsCount:{outstandingConfirms.Count}"
        );

    var endTime = Stopwatch.GetTimestamp();
    Console.WriteLine(
        $"Published {MESSAGE_COUNT:N0} messages and handled confirm asynchronously {Stopwatch.GetElapsedTime(startTime, endTime).TotalMilliseconds:N0} ms - outstandingConfirmsCount:{outstandingConfirms.Count}"
    );
}

static IConnection CreateConnection()
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/"
    };
    return factory.CreateConnection();
}

static async ValueTask<bool> WaitUntil(int numberOfSeconds, Func<bool> condition)
{
    int waited = 0;
    while (!condition() && waited < numberOfSeconds * 10)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        waited += 100;
    }

    return condition();
}
