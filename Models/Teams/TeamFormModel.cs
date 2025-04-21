namespace Tournament.Models.Teams
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class TeamFormModel
    {
            [Display(Name = "Брой отбори")]
            [Range(1, 16, ErrorMessage = "Избери между 1 и 16 отбора.")]
            public int TeamCount { get; set; }
    }
}
