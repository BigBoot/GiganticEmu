namespace GiganticEmu.Shared;

public record ReportPostRequest
{
    public MatchResult? Result { get; init; } = null;
    public string? ParseError { get; init; } = null;
    public string Server { get; init; } = default!;
};
