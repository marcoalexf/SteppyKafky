using System.Text;
using Confluent.Kafka;

namespace SteppyKafky;

public static class MessageMatcher
{
    // Returns true when the consumed message content contains all required key/value pairs
    public static bool Matches(ConsumeResult<Ignore, string> result, Dictionary<string, string> required)
    {
        if (required == null || required.Count == 0) return true;

        var body = result?.Message?.Value ?? string.Empty;
        return MatchesBody(body, required);
    }

    // Test-friendly overload: check a raw body string only
    public static bool Matches(string body, Dictionary<string, string> required)
    {
        if (required == null || required.Count == 0) return true;
        return MatchesBody(body ?? string.Empty, required);
    }

    private static bool MatchesBody(string body, Dictionary<string, string> required)
    {
        // If body is empty and there are required key/value pairs, it cannot match
        if (string.IsNullOrEmpty(body)) return required.Count == 0;

        foreach (var kv in required)
        {
            var key = kv.Key ?? string.Empty;
            var val = kv.Value ?? string.Empty;
            if (string.IsNullOrEmpty(key)) continue; // ignore empty keys

            var matched = false;

            // Check common textual patterns in the body (case-insensitive)
            // key=value, key:value, "key":"value", 'key':'value', also JSON-like "key":value
            if (body.IndexOf($"{key}={val}", StringComparison.OrdinalIgnoreCase) >= 0) matched = true;
            else if (body.IndexOf($"\"{key}\" : {val}", StringComparison.OrdinalIgnoreCase) >= 0) matched = true;
            else if (body.IndexOf($"'{key}':'{val}'", StringComparison.OrdinalIgnoreCase) >= 0) matched = true;
            else if (body.IndexOf($"\"{key}\":{val}", StringComparison.OrdinalIgnoreCase) >= 0) matched = true; // numeric/unquoted JSON value

            if (!matched) return false;
        }

        return true;
    }
}
