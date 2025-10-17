namespace SteppyKafky;

public sealed record Token(TokenType Type, string Value, int Position)
{
    public override string ToString() => $"{Type}('{Value}')@{Position}";
}

