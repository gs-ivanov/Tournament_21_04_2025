namespace Tournament.Services.MatchScheduler
{
    using Tournament.Data.Models;
    using System.Collections.Generic;

    public interface IMatchSchedulerService
    {
        List<Match> GenerateSchedule(List<Team> teams, Tournament tournament);
    }
}
