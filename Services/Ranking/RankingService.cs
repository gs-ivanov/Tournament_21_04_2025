namespace Tournament.Services.Ranking
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Models.TeamRanking;

    public class RankingService //: IRankingService
    {
        private readonly TurnirDbContext _context;

        public RankingService(TurnirDbContext context)
        {
            _context = context;
        }

        //public async Task<List<TeamRankingViewModel>> GetRankingsAsync()
        //{
            //var teams = await _context.Teams.ToListAsync();
            //var matches = await _context.Matches
            //    .Where(m => m.ScoreA.HasValue && m.ScoreB.HasValue)
            //    .ToListAsync();

            //var rankings = new Dictionary<int, TeamRankingViewModel>();

            //foreach (var team in teams)
            //{
            //    rankings[team.Id] = new TeamRankingViewModel
            //    {
            //        TeamName = team.Name,
            //        Wins = 0,
            //        Draws = 0,
            //        Losses = 0,
            //        Points = 0
            //    };
            //}

            //foreach (var match in matches)
            //{
            //    var teamA = rankings[match.TeamAId];
            //    var teamB = rankings[match.TeamBId];

            //    if (match.ScoreA > match.ScoreB)
            //    {
            //        teamA.Wins++;
            //        teamA.Points += 3;
            //        teamB.Losses++;
            //    }
            //    else if (match.ScoreA < match.ScoreB)
            //    {
            //        teamB.Wins++;
            //        teamB.Points += 3;
            //        teamA.Losses++;
            //    }
            //    else
            //    {
            //        teamA.Draws++;
            //        teamB.Draws++;
            //        teamA.Points++;
            //        teamB.Points++;
            //    }
            //}

            //return rankings.Values
            //    .OrderByDescending(r => r.Points)
            //    .ThenByDescending(r => r.Wins)
            //    .ToList();
        //}

    }
}
