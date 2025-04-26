namespace Tournament.Models
{

    using System.ComponentModel.DataAnnotations;

    public enum TournamentType
    {
        [Display(Name = "Елиминации")]
        Knockout=1,

        [Display(Name = "Двойна елиминация")]
        DoubleElimination=2,

        [Display(Name = "Всеки срещу всеки")]
        RoundRobin=3,

        [Display(Name = "Групи + елиминации")]
        GroupAndKnockout=4,

        [Display(Name = "Швейцарска система")]
        Swiss=5
    }
}
