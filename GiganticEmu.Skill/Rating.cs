using Glicko2;
using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Skill;

public class Rating
{
    private static RatingCalculator RATING_CALCULATOR = new RatingCalculator(0.06, 0.5);

    public static ICollection<Participant> UpdateRatings(ICollection<Participant> winners, ICollection<Participant> losers)
    {
        var games = new RatingPeriodResults();

        var winnerRatings = winners.Select(x => (x, new Glicko2.Rating(RATING_CALCULATOR, x.Rating, x.RatingDeviation, x.Volatility))).ToList();
        var loserRatings = losers.Select(x => (x, new Glicko2.Rating(RATING_CALCULATOR, x.Rating, x.RatingDeviation, x.Volatility))).ToList();

        foreach (var (_, winner) in winnerRatings)
        {
            foreach (var (_, loser) in loserRatings)
            {
                games.AddResult(winner, loser);
            }
        }

        RATING_CALCULATOR.UpdateRatings(games);

        return winnerRatings.Concat(loserRatings).Select(rating => new Participant
        {
            Id = rating.x.Id,
            Name = rating.x.Name,
            Rating = rating.Item2.GetRating(),
            RatingDeviation = rating.Item2.GetRatingDeviation(),
            Volatility = rating.Item2.GetVolatility(),
        }).ToList();
    }
}