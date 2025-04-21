namespace Tournament.Data.Models
{
    using Microsoft.EntityFrameworkCore;
    [Keyless]
    public class Ranking
    {
        public int TeamId { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int Points { get; set; }

    }
}
