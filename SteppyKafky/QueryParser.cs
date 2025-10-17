namespace SteppyKafky;

public static class QueryParser
{
    private static List<string> Tokenize(string query)
    {
        // Simple tokenizer splitting by spaces
        return query.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
    }

    public static Dictionary<string, string> Parse(string query)
    {
        var tokens = Tokenize(query ?? string.Empty);
        return Parse(tokens);
    }

    public static Dictionary<string, string> Parse(List<string> tokenizedQuery)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in tokenizedQuery)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var item = raw.Trim();

            // split on '=' into exactly two parts: key and value
            var parts = item.Split(new[] { '=' }, 2);
            if (parts.Length != 2) continue; // invalid entry

            var key = parts[0].Trim();
            var val = parts[1].Trim();

            // strip surrounding quotes if present
            if (val.Length >= 2 && ((val[0] == '"' && val[^1] == '"') || (val[0] == '\'' && val[^1] == '\'')))
            {
                val = val.Substring(1, val.Length - 2);
            }

            // store (last occurrence wins)
            if (!string.IsNullOrEmpty(key)) result[key] = val;
        }

        return result;
    }
}