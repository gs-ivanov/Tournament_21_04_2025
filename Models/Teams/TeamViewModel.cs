namespace Tournament.Models.Teams
{
    // ViewModel за списък и детайли
    public class TeamViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CoachName { get; set; }
        public string LogoUrl { get; set; }
        public string ContactEmail { get; set; }
        public bool FeePaid { get; set; }
    }
}
