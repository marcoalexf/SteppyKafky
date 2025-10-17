using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using SteppyKafky;

// Build configuration from appsettings.json
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Bind the "Kafka" section into a strongly-typed record
var kafka = config.GetSection("Kafka").Get<KafkaOptions>();

if (kafka is null)
{
    Console.WriteLine("Kafka configuration section is missing.");
    return;
}

// Example usage
Console.WriteLine($"Kafka bootstrap: {kafka.Consumer.BootstrapServers}");


// Tokenize and print the Query from appsettings.json
var query = config.GetValue<string>("Query") ?? string.Empty;
Console.WriteLine("Tokenized Query:");
foreach (var t in Tokenizer.Tokenize(query)) Console.WriteLine("  " + t);

// Simple Kafka consumer
var consumerConfig = new ConsumerConfig
{
    BootstrapServers = kafka.Consumer.BootstrapServers,
    GroupId = kafka.Consumer.GroupId,
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.Plain,
    SaslUsername = kafka.Consumer.SaslUsername,
    SaslPassword = kafka.Consumer.SaslPassword,
    AutoOffsetReset = AutoOffsetReset.Earliest,
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
consumer.Subscribe(kafka.Consumer.Topic);

Console.WriteLine("Starting string consumer loop. Press Ctrl-C to exit.");
try
{
    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            var consumeResult = consumer.Consume(cts.Token);
            ConsoleMessageRenderer.Render(consumeResult);
            Console.WriteLine();
            Console.WriteLine("Press ENTER to consume the next message (or Ctrl-C to exit)...");
            _ = Console.ReadLine();
        }
        catch (ConsumeException ex)
        {
            Console.WriteLine($"Consume error: {ex.Error.Reason}");
            // skip this message and continue consuming
            continue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error while consuming: {ex.Message}");
            continue;
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Closing consumer.");
}
finally
{
    consumer.Close();
}
