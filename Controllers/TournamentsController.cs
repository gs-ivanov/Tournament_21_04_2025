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
    using Tournament.Services.MatchScheduler;

    public class TournamentsController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly IMatchSchedulerService _matchScheduler;

        public TournamentsController(IMatchSchedulerService matchScheduler, TurnirDbContext context)
        {
            _context = context;
            _matchScheduler = matchScheduler;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var matches = this._context.Matches.Any();
            var tournaments = await _context.Tournaments.ToListAsync();
            TempData["data"] = !matches ? "You can set items!" : "Attension! Tournament on progress";
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
                .Where(t => t.IsActive == true)
                .Select(t => t.Id)
                .FirstOrDefault();

            if (existingTournamentId > 0)
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

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null || !tournament.IsActive)
            {
                TempData["Message"] = "Няма активен турнир или той е невалиден.";
                return RedirectToAction("Index");
            }
            var teamIds = await _context.ManagerRequests
                .Where(r => r.TournamentId == tournament.Id && r.IsApproved && r.FeePaid)
                .Select(r => r.TeamId)
                .ToListAsync();

            var approvedTeams = await _context.Teams
                .Where(t => teamIds.Contains(t.Id))
                .ToListAsync();

            //var approvedTeams = await _context.ManagerRequests
            //    .Include(r => r.Team)
            //    .Where(r => r.TournamentId == tournament.Id && r.IsApproved && r.FeePaid)
            //    .Select(r => r.Team)
            //    .ToListAsync();

            if (approvedTeams.Count < 4)
            {
                TempData["Message"] = "Не са налични достатъчно отбори за генериране на график.";
                return RedirectToAction("Index");
            }

            // 🔴 Изтриваме съществуващи мачове за турнира
            var existingMatches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();

            if (!existingMatches.Any())
            {
                _context.Matches.RemoveRange(existingMatches);

                await _context.SaveChangesAsync();
            }

            // 🟢 Ето тук идва новият ред:
            var matches = _matchScheduler.GenerateSchedule(approvedTeams, tournament);

            _context.Matches.AddRange(matches);

            await _context.SaveChangesAsync();

            TempData["Message"] = "Графикът беше успешно генериран.";
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