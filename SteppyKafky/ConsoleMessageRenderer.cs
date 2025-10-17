using Confluent.Kafka;
using Spectre.Console;

namespace SteppyKafky;

public static class ConsoleMessageRenderer
{
    // Renders a simple table with message metadata and content
    public static void Render(ConsumeResult<Ignore, string> result)
    {
        AnsiConsole.Clear();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[yellow]Kafka Message[/]")
            .AddColumn(new TableColumn("Field").NoWrap())
            .AddColumn(new TableColumn("Value"))
            .Centered();

        var topic = result.Topic;
        var partition = result.Partition.Value.ToString();
        var offset = result.Offset.Value.ToString();
        var timestamp = result.Message?.Timestamp.UtcDateTime;
        var value = result.Message?.Value ?? string.Empty;

        table.AddRow("Topic", Markup.Escape(topic));
        table.AddRow("Partition", partition);
        table.AddRow("Offset", offset);
        if (timestamp.HasValue)
        {
            table.AddRow("Timestamp (UTC)", Markup.Escape(timestamp.Value.ToString("O")));
        }

        // Render headers (keys only) if any
        var headers = result.Message?.Headers;
        if (headers is { Count: > 0 })
        {
            var headerKeys = string.Join(", ", headers.Select(h => h.Key));
            table.AddRow("Headers", Markup.Escape(headerKeys));
        }

        // Message value (escaped to avoid Spectre markup interpretation)
        table.AddRow("Value", Markup.Escape(value));

        AnsiConsole.Write(table);
    }
}
