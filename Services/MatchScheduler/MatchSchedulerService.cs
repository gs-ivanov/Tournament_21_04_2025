namespace Tournament.Services.MatchScheduler
{
    using System.Collections.Generic;
    using System;
    using Tournament.Models;
    using Tournament.Data.Models;

    public class MatchSchedulerService : IMatchSchedulerService
    {
        public List<Match> GenerateSchedule(List<Team> teams, Tournament tournament)
        {
            IMatchGenerator generator = tournament.Type switch
            {
                TournamentType.RoundRobin => new RoundRobinScheduler(),
                TournamentType.Knockout => new KnockoutScheduler(),
                // Можеш да добавиш и други тук:
                // TournamentType.Swiss => new SwissScheduler(),
                _ => throw new NotSupportedException("Типът турнир не се поддържа.")
            };

            return generator.Generate(teams, tournament);
        }

    }
}
