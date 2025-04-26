namespace Tournament.Data.Models
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int TeamAId { get; set; }                  // ✅ Ясен FK
        public Team TeamA { get; set; }

        public int TeamBId { get; set; }                  // ✅ Ясен FK
        public Team TeamB { get; set; }

        [Range(0, 100)]
        public int? ScoreA { get; set; }

        [Range(0, 100)]
        public int? ScoreB { get; set; }

        public DateTime? PlayedOn { get; set; }
        public bool IsPostponed { get; set; } = false;

        public bool IsFinal { get; set; } = false;

    }
}
