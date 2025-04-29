namespace Tournament.Models
{

    using System.ComponentModel.DataAnnotations;

    public enum TournamentType
    {
        [Display(Name = "Елиминации")]
        Knockout,
        [Display(Name = "Двойна елиминация")]
        DoubleElimination,
        [Display(Name = "Всеки срещу всеки")]
        RoundRobin,
        [Display(Name = "Групи + елиминации")]
        GroupAndKnockout,
        [Display(Name = "Швейцарска система")]
        Swiss
    }
}
