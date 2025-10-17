namespace SteppyKafky;

public sealed record KafkaOptions
{
    public required ConsumerOptions Consumer { get; init; }
    public required SchemaRegistryOptions SchemaRegistry { get; init; }
}

public sealed record ConsumerOptions
{
    public string? BootstrapServers { get; init; }
    public string? GroupId { get; init; }
    public string? SaslUsername { get; init; }
    public string? SaslPassword { get; init; }
    public string? Topic { get; init; }
    public string? ConsumerGroupOffsetUtcTimestamp { get; init; }
}

public sealed record SchemaRegistryOptions
{
    public string? Url { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}
