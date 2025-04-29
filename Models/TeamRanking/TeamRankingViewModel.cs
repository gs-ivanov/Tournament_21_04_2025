namespace Tournament.Models.TeamRanking
{
    public class TeamRankingViewModel
    {
        public string TeamName { get; set; }
        public string TournamentName { get; set; }

        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }

        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }

        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => Wins * 3 + Draws;

        public string LogoUrl { get; set; } // ✅ Взимаме директно от dbo.Teams
    }
}

