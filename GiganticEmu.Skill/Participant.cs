using System;

namespace GiganticEmu.Skill;

public class Participant
{
    public static double DEFAULT_RATING = 1500.0;
    public static double DEFAULT_RATING_DEVIATON = 350.0;
    public static double DEFAULT_VOLATILITY = 0.06;

    public long Id { get; init; }

    public string? Name { get; init; } = "Unknown";

    public string? MatchToken { get; init; } = null;

    public double Rating { get; init; } = DEFAULT_RATING;

    public double RatingDeviation { get; init; } = DEFAULT_RATING_DEVIATON;

    public double Volatility { get; init; } = DEFAULT_VOLATILITY;
}