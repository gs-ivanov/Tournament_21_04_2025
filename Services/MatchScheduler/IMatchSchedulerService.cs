namespace Tournament.Services.MatchScheduler
{
    using System;
    using System.Threading.Tasks;

    public interface IMatchSchedulerService
    {
        public Task<int> GenerateScheduleAsync(DateTime startDate);
    }
}
