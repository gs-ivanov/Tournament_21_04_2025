namespace Tournament.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string CoachName { get; set; }
        public string LogoUrl { get; set; }
        public bool FeePaid { get; set; }

        public ICollection<Match> MatchesAsTeamA { get; set; } = new List<Match>();
        public ICollection<Match> MatchesAsTeamB { get; set; } = new List<Match>();
        public ICollection<ManagerRequest> ManagerRequests { get; set; } = new List<ManagerRequest>();

        [Required]
        public string? UserId { get; set; }
        public User User { get; set; }

        // 🆕 Свързване с Tournament
        public int? TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }
    }
}
