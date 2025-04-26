namespace Tournament.Data.Models
{
    using global::Tournament.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Име на турнира")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Тип турнир")]
        public TournamentType Type { get; set; }

        [Required]
        [Display(Name = "Име на турнир")]
        public string TypeName { get; set; }

        [Display(Name = "Начална дата")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Отворен за заявки")]
        public bool IsOpenForApplications { get; set; }

        public bool IsActive { get; set; } // 🆕 Добавяме това

    }
}
