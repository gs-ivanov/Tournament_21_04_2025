namespace Tournament.Services.Ranking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tournament.Models.TeamRanking;

    public interface IRankingService
    {
        public Task<List<TeamRankingViewModel>> GetRankingsAsync();
    }
}
