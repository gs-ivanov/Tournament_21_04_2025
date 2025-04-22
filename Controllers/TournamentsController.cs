namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;

    public class TournamentsController : Controller
    {
        private readonly TurnirDbContext _context;

        public TournamentsController(TurnirDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var tournaments = await _context.Tournaments.ToListAsync();
            return View(tournaments);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            return View(tournament);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Tournament updated)
        {
            if (id != updated.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(updated);

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
                return NotFound();

            // Променям текущия турнир
            tournament.Name = updated.Name;
            tournament.StartDate = updated.StartDate;
            tournament.IsActive = updated.IsActive;

            // Обвързваме логически: ако е активен → заявки = true, иначе false
            tournament.IsOpenForApplications = updated.IsActive;

            // Ако активираме този турнир, всички останали стават неактивни и затворени
            if (updated.IsActive)
            {
                var others = await _context.Tournaments
                    .Where(t => t.Id != updated.Id)
                    .ToListAsync();

                foreach (var t in others)
                {
                    t.IsActive = false;
                    t.IsOpenForApplications = false;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Турнирът беше успешно обновен.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult SelectForSchedule()
        {
            int tournamentId = -3;

            var existingTournamentId = _context
                .Tournaments
                .Where(t => t.IsActive==true)
                .Select(t=>t.Id)
                .FirstOrDefault();

            if (existingTournamentId>0)
            {
                tournamentId = (int)existingTournamentId;
            }


            var isExistingMatches = _context.Matches.Any();

            if (isExistingMatches)
            {
                var existingMatches = _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .Where(m => m.TournamentId == tournamentId)
                    .OrderBy(m => m.PlayedOn)
                    .ToList();

                ViewBag.TournamentId = tournamentId;
                return View("ConfirmScheduleOverwrite", existingMatches);
            }

            return RedirectToAction(nameof(GenerateSchedule), new { tournamentId });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateNewSchedule(int id)
        {
            return await GenerateSchedule(id);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                TempData["Message"] = "❌ Турнирът не беше намерен.";
                return RedirectToAction("Index", "Home");
            }
            
            var pending=await _context.ManagerRequests.CountAsync();

            var requests = await _context.ManagerRequests
                .Include(r => r.Team)
                .Where(r => r.IsApproved && r.FeePaid && r.TournamentId == tournamentId)
                .ToListAsync();

            if (requests.Count < 4)
            {
                TempData["Message"] = "❌ Трябват 4 или повече четно число отбори с платена такса и одобрение!";
                return RedirectToAction("Index", "Home");
            }
            else if (requests.Count ==0 && pending>= 4)
            {
                TempData["Message"] = $"❌ Имате {pending} отборa с ne платена такса и одобрение!";
                return RedirectToAction("Index", "Home");
            }

            var teams = requests.Select(r => r.Team).ToList();
            var rng = new System.Random();
            teams = teams.OrderBy(t => rng.Next()).ToList();

            var oldMatches = _context.Matches
                .Where(m => m.TournamentId == tournamentId);
            _context.Matches.RemoveRange(oldMatches);

            var matches = new List<Match>();
            var firstLeg = GenerateRoundRobin(teams);
            var secondLeg = GenerateRoundRobin(teams, reverseHomeAway: true);
            var allRounds = firstLeg.Concat(secondLeg).ToList();

            for (int round = 0; round < allRounds.Count; round++)
            {
                var matchDate = tournament.StartDate.AddDays(7 * round);

                foreach (var (home, away) in allRounds[round])
                {
                    matches.Add(new Match
                    {
                        TeamAId = home.Id,
                        TeamBId = away.Id,
                        TournamentId = tournamentId,
                        PlayedOn = matchDate.Date
                    });
                }
            }

            _context.Matches.AddRange(matches);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Двоен график беше генериран ({matches.Count} мача).";
            return RedirectToAction("Index", "Matches");
        }

        private List<List<(Team Home, Team Away)>> GenerateRoundRobin(List<Team> teams, bool reverseHomeAway = false)
        {
            var rounds = new List<List<(Team, Team)>>();

            var teamCount = teams.Count;
            var teamList = new List<Team>(teams);

            if (teamCount % 2 != 0)
            {
                teamList.Add(null);
                teamCount++;
            }

            int totalRounds = teamCount - 1;
            int matchesPerRound = teamCount / 2;

            for (int round = 0; round < totalRounds; round++)
            {
                var roundMatches = new List<(Team, Team)>();

                for (int match = 0; match < matchesPerRound; match++)
                {
                    var home = teamList[match];
                    var away = teamList[teamCount - 1 - match];

                    if (home != null && away != null)
                    {
                        roundMatches.Add(reverseHomeAway ? (away, home) : (home, away));
                    }
                }

                var last = teamList[teamCount - 1];
                teamList.RemoveAt(teamCount - 1);
                teamList.Insert(1, last);

                rounds.Add(roundMatches);
            }

            return rounds;
        }
    }
}