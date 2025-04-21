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
            
            var activeTournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.IsActive);

            var isMatches = await _context.Matches
                .Where(m => m.Id > 0)
                .FirstOrDefaultAsync();

            if (isMatches!=null && activeTournament!=null)
            {
                var matches = await _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .Where(m => m.TournamentId == activeTournament.Id && m.ScoreA != null && m.ScoreB != null)
                    .ToListAsync();

                var teams = await _context.Teams
                    .Where(t => t.TournamentId == activeTournament.Id)
                    .ToListAsync();

                var rankings = teams.Select(team =>
                {
                    var played = matches
                        .Where(m => m.TeamAId == team.Id || m.TeamBId == team.Id)
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
                        LogoUrl = team.LogoUrl
                    };
                })
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ToList();

                return View(rankings);
            }
            else
            {
                TempData["NonDisplay"] = "Yes";
                if (User.IsInRole("Administrator"))
                {
                    if (TempData["Step3"] != null)
                    {
                        TempData["Step3"] =null;
                        TempData["Message"] = "Има обявен турнир.\nРегестрирай се като Мениджър за участие в него.\nУспех!";
                        return View();

                    }
                    else
                    {
                        ViewData["ShowWelcome"] = false;
                        TempData["Message"] = "Welcome Admin. Take your duty, please!";
                        return View();
                    }
                }
                else
                {
                    ViewData["ShowWelcome"] = true;
                    TempData["Message"] = "Waiting Admin to take his duty, please!";

                    //return RedirectToAction(nameof(Index));
                    return View();
                }
            }

        }

        [HttpPost]
        public IActionResult Index(string s)
        {
            TempData["Message"] = "OOOOOOOO Waiting Admin to take his duty, please!";
            return  RedirectToAction("Step2", "Setup");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
