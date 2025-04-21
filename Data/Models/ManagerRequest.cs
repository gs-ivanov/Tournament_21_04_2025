namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;

    public class ManagerRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public TournamentType? TournamentType { get; set; }
        public string JsonPayload { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public string ReceiptNumber { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool FeePaid { get; set; } = false;


        // ✅ За регистрация (email)
        public static string GenerateJson(string email, TournamentType tournamentType)
        {
            var payload = new
            {
                Email = email,
                TournamentType = tournamentType.ToString(),
                RequestedAt = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(payload);
        }

        // ✅ За създаване на отбор
        public static string GenerateJson(Team team, TournamentType tournamentType)
        {
            var payload = new
            {
                TeamName = team.Name,
                Coach = team.CoachName,
                TournamentType = tournamentType.ToString(),
                SubmittedAt = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
