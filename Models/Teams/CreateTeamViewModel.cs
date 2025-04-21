namespace Tournament.Models.Teams
{
    using System.ComponentModel.DataAnnotations;

    // ViewModel за създаване на отбор
    public class CreateTeamViewModel
    {
        [Required]
        [Display(Name = "Име на отбора")]
        public string Name { get; set; }

        [Display(Name = "Треньор")]
        public string CoachName { get; set; }

        [Display(Name = "Лого (URL)")]
        public string LogoUrl { get; set; }

        [EmailAddress]
        [Display(Name = "Имейл за контакт")]
        public string ContactEmail { get; set; }

        [Display(Name = "Такса платена")]
        public bool FeePaid { get; set; }

        [Required]
        [Display(Name = "Тип турнир")]
        public TournamentType TournamentType { get; set; }
    }
}
