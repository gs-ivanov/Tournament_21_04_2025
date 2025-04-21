namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;

    public class MatchSchedulerService: IMatchSchedulerService
    {
        private readonly TurnirDbContext _context;

        public MatchSchedulerService(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<int> GenerateScheduleAsync(DateTime startDate)
        {
            var teams = _context.Teams.ToList();
            var matches = new List<Match>();

            if (teams.Count < 2)
                return 0;

            int matchIntervalDays = 1;
            int round = 0;

            // Round-robin алгоритъм
            for (int i = 0; i < teams.Count - 1; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    matches.Add(new Match
                    {
                        TeamAId = teams[i].Id,
                        TeamBId = teams[j].Id,
                        PlayedOn = startDate.AddDays(round * matchIntervalDays),
                    });
                    round++;
                }
            }

            await _context.Matches.AddRangeAsync(matches);
            await _context.SaveChangesAsync();

            return matches.Count;
        }
    }

}
