using System.Linq;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using GiganticEmu.Web;
using GiganticEmu.Skill;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using Polly;
using Flurl.Http;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;
        private readonly BackendConfiguration _configuration;

        public MatchController(ILogger<FriendsController> logger, IDbContextFactory<ApplicationDatabase> databaseFactory, IOptions<BackendConfiguration> configuration)
        {
            _logger = logger;
            _databaseFactory = databaseFactory;
            _configuration = configuration.Value;
        }


        [HttpPost]
        [RequireApiKey]
        [Produces("application/json")]
        public async Task<IActionResult> Post(MatchPostRequest request)
        {
            var participants = await Task.WhenAll(request.Players.Select(async player =>
            {
                var id = long.Parse(player);
                using var db = _databaseFactory.CreateDbContext();
                var user = await db.Users.SingleOrDefaultAsync(x => x.DiscordId == id);

                if (user == null)
                {
                    return new Participant { Id = id };
                }

                user.MatchToken = Crypt.CreateSecureRandomString(10);
                await db.SaveChangesAsync();

                return new Participant
                {
                    Id = id,
                    Name = user.UserName,
                    Rating = user.SkillRating,
                    RatingDeviation = user.SkillDeviation,
                    Volatility = user.SkillVolatility,
                    MatchToken = user.MatchToken,
                };
            }));

            var (team1, team2) = MatchMaking.MakeTeams(participants);

            using var db = _databaseFactory.CreateDbContext();

            var reportToken = Crypt.CreateSecureRandomString();
            await db.AddAsync(new ReportToken() { Token = reportToken });
            await db.SaveChangesAsync();

            return Ok(new MatchPostResponse(RequestResult.Success)
            {
                Team1 = new MatchPostResponse.Team(team1.Select(x => x.Id.ToString()).ToList(), team1.Average(x => x.Rating)),
                Team2 = new MatchPostResponse.Team(team2.Select(x => x.Id.ToString()).ToList(), team2.Average(x => x.Rating)),
                ReportToken = reportToken,
            });
        }

        [HttpPost("token")]
        [RequireApiKey]
        [Produces("application/json")]
        public async Task<IActionResult> PostToken(MatchTokenPostRequest request)
        {
            var id = long.Parse(request.DiscordId);

            using var db = _databaseFactory.CreateDbContext();
            var user = await db.Users.SingleOrDefaultAsync(x => x.DiscordId == id);

            if (user == null)
            {
                return Ok(new ResponseBase(RequestResult.UnknownUser));
            }

            user.MatchToken = Crypt.CreateSecureRandomString(10);
            await db.SaveChangesAsync();

            return Ok(new MatchTokenPostResponse(RequestResult.Success)
            {
                MatchToken = user.MatchToken,
                Name = user.UserName!,
            });
        }


        [HttpPost("report")]
        [Produces("application/json")]
        public async Task<IActionResult> PostReport(ReportPostRequest request, string token)
        {
            using var db = _databaseFactory.CreateDbContext();
            var (result, error, server) = (request.Result, request.ParseError, request.Server);

            var tokenEntity = await db.ReportTokens
                .Where(x => x.Token == token)
                .SingleOrDefaultAsync();

            if (tokenEntity == null)
            {
                return Ok(new ResponseBase(RequestResult.Unauthorized));
            }

            db.Remove(tokenEntity);
            await db.SaveChangesAsync();

            int unknownPlayerCounter = 0;
            var findUser = async (Player player) =>
            {
                using var db = _databaseFactory.CreateDbContext();
                var user = await db.Users.SingleOrDefaultAsync(
                    x => x.MotigaId == player.MotigaId ||
                    (player.MatchToken != null && x.MatchToken == player.MatchToken)
                );

                return user switch
                {
                    { DiscordId: long } => new Participant
                    {
                        Id = user.DiscordId.Value,
                        Name = user.UserName!,
                        Rating = user.SkillRating,
                        RatingDeviation = user.SkillDeviation,
                        Volatility = user.SkillVolatility,
                    },
                    _ => new Participant { Id = -Interlocked.Increment(ref unknownPlayerCounter), Name = player.Name },
                };
            };

            if (result == null)
            {
                if (_configuration.Discord.ReportWebhookUrl != null)
                {
                    await _configuration.Discord.ReportWebhookUrl
                        .WithPolly(policy => policy.RetryAsync(3))
                        .PostJsonAsync(new
                        {
                            embeds = new[] {
                                new {
                                    title= "MatchResult Error",
                                    fields= new object[] {
                                        new {
                                            name = "Server",
                                            value = server,
                                        },
                                        new {
                                            name = "Error",
                                            value = error ?? "The server did not provide an error message...",
                                        }
                                    }
                                }
                            },
                            attachments = new object[] { },
                            flags = 4096
                        })
                        .ReceiveString();
                }

                return Ok(new ReportPostResponse(RequestResult.Success));
            }

            var team1Participants = await Task.WhenAll(result.Team1.Select(findUser));
            var team2Participants = await Task.WhenAll(result.Team2.Select(findUser));

            if (result.Winner != null)
            {
                var updatedRatings = result.Winner switch
                {
                    MatchResult.Team.Team1 => Rating.UpdateRatings(team1Participants, team2Participants),
                    MatchResult.Team.Team2 => Rating.UpdateRatings(team2Participants, team1Participants),
                };

                foreach (var participant in updatedRatings)
                {
                    if (participant.Id <= 0)
                    {
                        continue;
                    }

                    var user = await db.Users.SingleAsync(x => x.DiscordId == participant.Id);

                    user.SkillDeviation = participant.RatingDeviation;
                    user.SkillRating = participant.Rating;
                    user.SkillVolatility = participant.Volatility;

                    db.Update(user);
                }

                await db.SaveChangesAsync();

                if (_configuration.Discord.ReportWebhookUrl != null)
                {
                    var printUser = (Participant participant) =>
                    {
                        var newRating = updatedRatings.Single(x => x.Id == participant.Id);
                        return participant.Id <= 0 ? participant.Name : $"<@{participant.Id}> {((int)participant.Rating)} -> {((int)newRating.Rating)}";
                    };

                    await _configuration.Discord.ReportWebhookUrl
                        .WithPolly(policy => policy.RetryAsync(3))
                        .PostJsonAsync(new
                        {
                            embeds = new[] {
                                new {
                                    title= "MatchResults",
                                    fields= new object[] {
                                        new {
                                            name = "Server",
                                            value = server,
                                        },
                                        new {
                                            name = $"Team1 ({((int)team1Participants.Average(x => x.Rating))})",
                                            value = string.Join("\n", team1Participants.Select(printUser)),
                                            inline = true
                                        },
                                        new {
                                            name = $"Team2 ({((int)team2Participants.Average(x => x.Rating))})",
                                            value = string.Join("\n", team2Participants.Select(printUser)),
                                            inline = true
                                        },
                                        new {
                                            name = "Winner",
                                            value = result.Winner.ToString()
                                        }
                                    }
                                }
                            },
                            attachments = new object[] { },
                            flags = 4096
                        })
                        .ReceiveString();
                }
            }

            return Ok(new ReportPostResponse(RequestResult.Success));
        }
    }
}