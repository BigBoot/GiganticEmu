using System;
using System.Collections.Generic;
using System.Linq;

namespace GiganticEmu.Skill;

public static class MatchMaking
{
    public static (ICollection<Participant>, ICollection<Participant>) MakeTeams(ICollection<Participant> participants)
    {
        var createdTeams = new List<List<Participant>>();

        var halfSize = participants.Count() / 2;
        var halfScore = participants.Sum(x => x.Rating) / 2;

        foreach (var participant in participants)
        {
            foreach (var i in Enumerable.Range(0, createdTeams.Count))
            {
                var newTeam = new List<Participant>(createdTeams[i]);
                if (newTeam.Count != halfSize && newTeam.Sum(x => x.Rating) + participant.Rating <= halfScore)
                {
                    newTeam.Add(participant);
                    createdTeams.Add(newTeam);
                }
            }
            createdTeams.Add(new List<Participant> { participant });
        }

        var teamOne = createdTeams
            .Where(team => team.Count == halfSize)
            .OrderBy(team => team.Sum(x => x.Rating))
            .Last();

        var teamTwo = participants
            .Except(teamOne)
            .ToList();

        return (teamOne, teamTwo);
    }
}