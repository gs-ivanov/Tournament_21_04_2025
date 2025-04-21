namespace Tournament.Controllers
{

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;

    [Authorize(Roles = "Administrator")]
    public class SetupController : Controller
    {
        private readonly TurnirDbContext _context;

        public SetupController(TurnirDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Step1()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step1Confirmed()
        {
            var matches = await _context.Matches.ToListAsync();
            _context.Matches.RemoveRange(matches);

            var tournaments = await _context.Tournaments.ToListAsync();
            foreach (var t in tournaments)
            {
                t.IsActive = false;
                t.IsOpenForApplications = false;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "⚠️ Системата беше успешно занулена.";
            return RedirectToAction("Step2");
        }
        [HttpGet]
        public IActionResult Step2()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step2(TournamentType selectedType, string name, DateTime startDate)
        {
            var all = await _context.Tournaments.ToListAsync();
            foreach (var t in all)
            {
                t.IsActive = false;
                t.IsOpenForApplications = false;
            }

            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Type == selectedType);

            if (tournament == null)
            {
                tournament = new Tournament
                {
                    Type = selectedType
                };
                _context.Tournaments.Add(tournament);
            }

            tournament.Name = name;
            tournament.StartDate = startDate;
            tournament.IsActive = true;
            tournament.IsOpenForApplications = true;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Турнирът \"{name}\" беше активиран.";
            return RedirectToAction("Step3");
        }

        [HttpGet]
        public async Task<IActionResult> Step3()
        {
            var activeTournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);

            if (activeTournament == null)
            {
                TempData["Message"] = "❌ Няма активен турнир.";
                return RedirectToAction("Step2");
            }

            var approvedRequests = await _context.ManagerRequests
                .Where(r => r.TournamentId == activeTournament.Id && r.IsApproved && r.FeePaid)
                .CountAsync();

            ViewBag.ApprovedCount = approvedRequests;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Step4()
        {
            var activeTournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);
            if (activeTournament == null)
            {
                TempData["Message"] = "❌ Няма активен турнир.";
                return RedirectToAction("Step2");
            }

            var teams = await _context.ManagerRequests
                .Include(r => r.Team)
                .Where(r => r.TournamentId == activeTournament.Id && r.IsApproved && r.FeePaid)
                .Select(r => r.Team)
                .ToListAsync();

            ViewBag.Teams = teams;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSchedule()
        {
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);
            if (tournament == null)
                return RedirectToAction("Step2");

            var requests = await _context.ManagerRequests
                .Include(r => r.Team)
                .Where(r => r.TournamentId == tournament.Id && r.IsApproved && r.FeePaid)
                .ToListAsync();

            if (requests.Count < 4 || requests.Count % 2 != 0)
            {
                TempData["Message"] = "❗ Нужни са поне 4 отбрани отбора с четен брой.";
                return RedirectToAction("Step4");
            }

            // Изчистваме предишни мачове
            var oldMatches = _context.Matches.Where(m => m.TournamentId == tournament.Id);
            _context.Matches.RemoveRange(oldMatches);

            var teams = requests.Select(r => r.Team).ToList();
            var rng = new Random();
            teams = teams.OrderBy(t => rng.Next()).ToList();

            var matches = new List<Match>();
            var rounds = GenerateRoundRobin(teams);
            rounds.AddRange(GenerateRoundRobin(teams, true)); // double

            for (int round = 0; round < rounds.Count; round++)
            {
                var date = tournament.StartDate.AddDays(7 * round);
                foreach (var (home, away) in rounds[round])
                {
                    matches.Add(new Match
                    {
                        TeamAId = home.Id,
                        TeamBId = away.Id,
                        TournamentId = tournament.Id,
                        PlayedOn = date
                    });
                }
            }

            _context.Matches.AddRange(matches);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Генерирани {matches.Count} мача.";
            return RedirectToAction("Index", "Matches");
        }

        private List<List<(Team Home, Team Away)>> GenerateRoundRobin(List<Team> teams, bool reverse = false)
        {
            var rounds = new List<List<(Team, Team)>>();
            var count = teams.Count;
            var list = new List<Team>(teams);

            if (count % 2 != 0)
            {
                list.Add(null); // dummy team
                count++;
            }

            for (int round = 0; round < count - 1; round++)
            {
                var roundMatches = new List<(Team, Team)>();
                for (int i = 0; i < count / 2; i++)
                {
                    var home = list[i];
                    var away = list[count - 1 - i];
                    if (home != null && away != null)
                        roundMatches.Add(reverse ? (away, home) : (home, away));
                }

                var last = list[count - 1];
                list.RemoveAt(count - 1);
                list.Insert(1, last);

                rounds.Add(roundMatches);
            }

            return rounds;
        }


    }
}
