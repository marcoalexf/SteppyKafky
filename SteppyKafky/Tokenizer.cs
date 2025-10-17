using System.Text;

namespace SteppyKafky;

public static class Tokenizer
{
    public static List<Token> Tokenize(string text)
    {
        text ??= string.Empty;
        var tokens = new List<Token>();
        int pos = 0;

        char Peek() => pos < text.Length ? text[pos] : '\0';
        char Advance() => pos < text.Length ? text[pos++] : '\0';
        bool IsAtEnd() => pos >= text.Length;

        void SkipWhitespace()
        {
            while (!IsAtEnd() && char.IsWhiteSpace(Peek())) Advance();
        }

        string ReadWhile(Func<char, bool> predicate)
        {
            var sb = new StringBuilder();
            while (!IsAtEnd() && predicate(Peek())) sb.Append(Advance());
            return sb.ToString();
        }

        string ReadQuotedString()
        {
            var quote = Advance(); // consume opening quote
            var sb = new StringBuilder();
            while (!IsAtEnd())
            {
                var c = Advance();
                if (c == '\\')
                {
                    if (IsAtEnd()) break;
                    var next = Advance();
                    sb.Append(next);
                    continue;
                }
                if (c == quote) break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        char PeekNextNonWhitespaceChar()
        {
            var idx = pos;
            while (idx < text.Length && char.IsWhiteSpace(text[idx])) idx++;
            return idx < text.Length ? text[idx] : '\0';
        }

        while (true)
        {
            SkipWhitespace();
            if (IsAtEnd())
            {
                tokens.Add(new Token(TokenType.EOF, string.Empty, pos));
                break;
            }

            var ch = Peek();
            var start = pos;

            if (ch == '(')
            {
                Advance();
                tokens.Add(new Token(TokenType.ParenthesisOpen, "(", start));
                continue;
            }
            if (ch == ')')
            {
                Advance();
                tokens.Add(new Token(TokenType.ParenthesisClose, ")", start));
                continue;
            }
            if (ch == ',')
            {
                Advance();
                tokens.Add(new Token(TokenType.Comma, ",", start));
                continue;
            }
            if (ch == '=')
            {
                Advance();
                tokens.Add(new Token(TokenType.Equals, "=", start));
                continue;
            }

            if (ch == '"' || ch == '\'')
            {
                var str = ReadQuotedString();
                tokens.Add(new Token(TokenType.PropertyValue, str, start));
                continue;
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                var word = ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                if (string.Equals(word, "AND", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new Token(TokenType.And, word, start));
                    continue;
                }
                if (string.Equals(word, "OR", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new Token(TokenType.Or, word, start));
                    continue;
                }
                if (string.Equals(word, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    tokens.Add(new Token(TokenType.Null, word, start));
                    continue;
                }

                var nextNonWs = PeekNextNonWhitespaceChar();
                if (nextNonWs == '=')
                {
                    tokens.Add(new Token(TokenType.PropertyName, word, start));
                }
                else
                {
                    tokens.Add(new Token(TokenType.PropertyValue, word, start));
                }

                continue;
            }

            if (char.IsDigit(ch))
            {
                var num = ReadWhile(char.IsDigit);
                tokens.Add(new Token(TokenType.PropertyValue, num, start));
                continue;
            }

            // Unknown single character
            Advance();
            tokens.Add(new Token(TokenType.Unknown, ch.ToString(), start));
        }

        return tokens;
    }
}

