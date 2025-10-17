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
var parsedQuery = QueryParser.Parse(query);
foreach (var kvp in parsedQuery)
{
    Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
}

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

            // skip empty messages
            if (consumeResult?.Message == null)
            {
                continue;
            }

            // If the message doesn't match all required key/value pairs from parsedQuery, skip it
            if (!MessageMatcher.Matches(consumeResult, parsedQuery))
            {
                Console.WriteLine("Message does not match filter; skipping.");
                continue;
            }

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
