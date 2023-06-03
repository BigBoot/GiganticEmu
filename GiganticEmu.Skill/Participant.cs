using System;

namespace GiganticEmu.Skill;

public class Participant
{
    public long Id { get; init; }

    public string? Name { get; init; } = "Unknown";

    public string? MatchToken { get; init; } = null;

    public double Rating { get; init; } = 1500.0;

    public double RatingDeviation { get; init; } = 350;

    public double Volatility { get; init; } = 0.06;
}