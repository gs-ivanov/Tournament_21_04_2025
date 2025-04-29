namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
    using Tournament.Models;
    using Tournament.Data;
    using Tournament.Models.TeamRanking;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;

    public class HomeController : Controller
    {
        private readonly TurnirDbContext _context;

        public HomeController(TurnirDbContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index()
        {
            var activeTournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);

            if (activeTournament != null)
            {
                var matches = await _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .Where(m => m.TournamentId == activeTournament.Id)
                    .ToListAsync();

                if (matches.Any()) // 👈 Важно: има график, дори без резултати
                {
                    var teams = await _context
                        .Teams
                        .Where(t=>t.FeePaid==true)
                        .ToListAsync();

                    var tournamentName = _context.Tournaments
                        .Where(t => t.IsActive == true)
                        .Select(t => t.Name)
                        .FirstOrDefault();

                    var rankings = teams.Select(team =>
                    {
                        var played = matches
                            .Where(m => m.TeamAId == team.Id || m.TeamBId == team.Id)
                            .Where(m => m.ScoreA != null && m.ScoreB != null)
                            .ToList();

                        int wins = 0, draws = 0, losses = 0, goalsFor = 0, goalsAgainst = 0;

                        foreach (var m in played)
                        {
                            int scored = m.TeamAId == team.Id ? m.ScoreA ?? 0 : m.ScoreB ?? 0;
                            int conceded = m.TeamAId == team.Id ? m.ScoreB ?? 0 : m.ScoreA ?? 0;

                            goalsFor += scored;
                            goalsAgainst += conceded;

                            if (scored > conceded) wins++;
                            else if (scored == conceded) draws++;
                            else losses++;
                        }

                        return new TeamRankingViewModel
                        {
                            TeamName = team.Name,
                            MatchesPlayed = played.Count,
                            Wins = wins,
                            Draws = draws,
                            Losses = losses,
                            GoalsFor = goalsFor,
                            GoalsAgainst = goalsAgainst,
                            LogoUrl = team.LogoUrl,
                            TournamentName = tournamentName
                        };
                    })
                    .OrderByDescending(r => r.Points)
                    .ThenByDescending(r => r.GoalDifference)
                    .ToList();

                    return View(rankings);
                }
                else
                {
                    TempData["Message"] = "Все още няма регистрирани участници!";
                    return RedirectToAction(nameof(HtmlCertificate));
                }
            }

            //// 👉 Няма график или няма активен турнир
            //TempData["NonDisplay"] = "Yes";

            //if (!User.Identity.IsAuthenticated)
            //{
            //    TempData["ShowWelcome"] = true;
            //    return RedirectToPage("/Account/Login", new { area = "Identity" });
            //}

            //if (User.IsInRole("Administrator"))
            //{
            //    TempData["Message"] = "Добре дошли, Администратор! Все още няма активен турнир.";
            //}
            //else
            //{
            //    TempData["Message"] = "В момента няма активен турнир. Моля, върнете се по-късно.";
            //}

            return View();
        }
        [HttpPost]
        public IActionResult Index(string s)
        {
            TempData["Message"] = "OOOOOOOO Waiting Admin to take his duty, please!";
            return RedirectToAction("Step2", "Setup");
        }
        public IActionResult HtmlCertificate()
        {
            TempData["Message_UUUUUUUU"] = "Future PDF Certificate";
            return View();
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
