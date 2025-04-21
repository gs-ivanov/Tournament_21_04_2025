namespace Tournament.Data
{
    using Tournament.Data.Models;
    using Tournament.Models;

    public class MatchSubscription
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; }

        public NotificationType Type { get; set; }
    }
}
