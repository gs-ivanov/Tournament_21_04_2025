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
    public class ToolsController : Controller
    {
        private readonly TurnirDbContext _context;

        public ToolsController(TurnirDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ResetScheduleAndTournaments()
        {
            // 1. Изчистваме всички срещи
            var allMatches = await _context.Matches.ToListAsync();
            _context.Matches.RemoveRange(allMatches);

            // 2. Променяме имената и статусите на всички турнири
            var listOfTitles = new List<string>
                {
                    "Пролетен турнир", "Летен шампионат", "Есена купа", "Зимна надпревара", "Купа Балканика"
                };

            var tournaments = await _context.Tournaments.ToListAsync();
            int n = 0;
            foreach (var t in tournaments)
            {
                t.Name = listOfTitles[n % listOfTitles.Count]; // сигурно обхождане
                t.IsActive = false;
                t.IsOpenForApplications = false;
                n++;
            }

            // 3. Записваме наведнъж
            var affected = await _context.SaveChangesAsync();
            TempData["Message"] = $"✅ Таблицата Matches беше изчистена. Обновени турнири: {tournaments.Count}. Записани промени: {affected}";
            return RedirectToAction("Index", "Tournaments");
        }

        [HttpPost]
        public async Task<IActionResult> ResetTeamsAndRequests()
        {
            TempData["ToolsReset"] = "Reset Teams and ManagerRequests items: FeePaid, Status, IsApproved and FeePaid.";
            var teams = await _context.Teams.ToListAsync();
            foreach (var team in teams)
            {
                team.FeePaid = false;
                //team.TournamentId = 0;
            }

            var requests = await _context.ManagerRequests.ToListAsync();
            foreach (var req in requests)
            {
                req.Status = RequestStatus.Pending;
                req.IsApproved = false;
                req.FeePaid = false;
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "🔁 Всички заявки и отбори бяха върнати в начално състояние.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> GenerateSchadel()
        {
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.IsActive);
            if (tournament == null)
            {
                TempData["Message"] = "Not active tournament!";

                return RedirectToAction(nameof(Index));
            }


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





        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            request.Status =RequestStatus.Approved;
            request.IsApproved = true;
            request.FeePaid = true;

            if (request.Team != null)
                request.Team.FeePaid = true;
                request.Team.TournamentId = request.Id;

            await _context.SaveChangesAsync();
            TempData["Message"] = $"✅ Заявката за отбор '{request.Team?.Name}' е одобрена.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel()
        {
            TempData["Message"] = "❎ Операцията беше отменена.";
            return RedirectToAction("Index");
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
