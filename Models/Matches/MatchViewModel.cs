namespace Tournament.Models.Matches
{
    using System;
    using System.ComponentModel.DataAnnotations;

    // ViewModel за показване на мачове
    public class MatchViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tournament Id")]
        public int TournamentId { get; set; }

        [Display(Name = "Отбор A")]
        public string TeamA { get; set; }

        [Display(Name = "Отбор B")]
        public string TeamB { get; set; }

        [Display(Name = "Дата на мача")]
        public DateTime PlayedOn { get; set; }

        public int? ScoreA { get; set; }
        public int? ScoreB { get; set; }
    }
}
