namespace Tournament.Models.Menagers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Tournament.Models.Teams;

    // ViewModel за мениджър на турнир
    public class MenagerViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Име на мениджъра")]
        public string Name { get; set; }

        [Display(Name = "Тип турнир")]
        public TournamentType TournamentType { get; set; }

        [Display(Name = "Отбори на мениджъра")]
        public List<TeamViewModel> Teams { get; set; } = new();
    }
}
