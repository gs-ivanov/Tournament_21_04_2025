namespace Tournament.Models.Matches
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class MatchFormModel
    {
        public int Id { get; set; }

        [Display(Name = "Отбор A")]
        [Required]
        public int TeamAId { get; set; }

        [Display(Name = "Отбор B")]
        [Required]
        public int TeamBId { get; set; }

        [Display(Name = "Резултат A")]
        public int? ScoreA { get; set; }

        [Display(Name = "Резултат B")]
        public int? ScoreB { get; set; }

        [Display(Name = "Дата на мача")]
        public DateTime? PlayedOn { get; set; }

        public List<SelectListItem> Teams { get; set; } = new();
    }
}
