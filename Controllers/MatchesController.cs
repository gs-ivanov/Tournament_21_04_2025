namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Matches;
    using Tournament.Services.MatchScheduler;

    public class MatchesController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly IMatchSchedulerService _matchScheduler;
        //private readonly IMatchResultNotifierService _notifier;


        public MatchesController(
            TurnirDbContext context,
            IMatchSchedulerService matchScheduler)
        {
            this._context = context;
            this._matchScheduler = matchScheduler;
            //this._notifier = notifier;
        }

        // Final
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateFinal(int tournamentId)
        {
            var matches = await _context.Matches
                .Where(m => m.TournamentId == tournamentId)
                .OrderBy(m => m.PlayedOn)
                .ToListAsync();

            if (matches.Count < 2)
            {
                TempData["Message"] = "Не са налични достатъчно полуфинали за създаване на финал.";
                return RedirectToAction("Index");
            }

            var semi1 = matches[0];
            var semi2 = matches[1];

            // Проверка дали и двата полуфинала имат резултат
            if (semi1.ScoreA == null || semi1.ScoreB == null || semi2.ScoreA == null || semi2.ScoreB == null)
            {
                TempData["Message"] = "Трябва първо да бъдат въведени резултатите от полуфиналите.";
                return RedirectToAction("Index");
            }

            // Определяме победителите
            var winner1Id = semi1.ScoreA > semi1.ScoreB ? semi1.TeamAId : semi1.TeamBId;
            var winner2Id = semi2.ScoreA > semi2.ScoreB ? semi2.TeamAId : semi2.TeamBId;

            // Намираме максималната дата от съществуващите полуфинали
            var maxPlayedOn = matches.Max(m => m.PlayedOn) ?? DateTime.Now;

            // Създаваме финален мач
            var finalMatch = new Match
            {
                TeamAId = winner1Id,
                TeamBId = winner2Id,
                TournamentId = tournamentId,
                PlayedOn = maxPlayedOn.AddDays(7),
                IsFinal = true // 🏆 Маркираме го като финал
            };

            _context.Matches.Add(finalMatch);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Финалът беше успешно създаден!";
            return RedirectToAction("Index");
        }
        //*************

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();

            return View(match);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ScoreA,ScoreB")] Match updated)
        {
            if (!ModelState.IsValid)
            {
                var original = await _context.Matches
                    .Include(m => m.TeamA)
                    .Include(m => m.TeamB)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return View(original);
            }

            var match = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();
            var now = DateTime.Now;

            var postponedMatches = _context.Matches
                .Where(m => m.PlayedOn < now && m.ScoreA == null && m.ScoreB == null)
                .ToList();

            foreach (var matchData in postponedMatches)
            {
                matchData.IsPostponed = true;
            }

            await _context.SaveChangesAsync();

            var previousUnplayed = await _context.Matches
                .Where(m => m.TournamentId == match.TournamentId
                         && m.PlayedOn < match.PlayedOn
                         && m.ScoreA == null
                         && m.ScoreB == null)
                .AnyAsync();

            if (previousUnplayed)
            {
                TempData["Message"] = "❌ Не може да въведете резултат за този мач, докато има неизиграни мачове от предишни кръгове.";
                return RedirectToAction("Index");
            }


            // 🔒 Забрана ако вече има резултат
            if (match.ScoreA.HasValue || match.ScoreB.HasValue)
            {
                TempData["Message"] = "❌ Резултат вече е въведен и не може да бъде редактиран.";
                return RedirectToAction("Index");
            }

            // 🔒 Забрана ако мачът не е изигран още
            if (match.PlayedOn > DateTime.Now)
            {
                TempData["Message"] = $"❌ Мачът още не е изигран. Днес е {now}, а мача е насрочен за {match.PlayedOn}. Не може да въведете резултат.";
                return RedirectToAction("Index");
            }

            match.ScoreA = updated.ScoreA;
            match.ScoreB = updated.ScoreB;

            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Резултатът беше успешно записан.";

            //❗ SMS временно изключен
            TempData["Message"] = "✅ Резултатът беше успешно записан и изпратен с СМС.";

            //await _smsSender.SendSmsAsync(
            //    "+359885773102",
            //    $"📢 Резултат от {match.TeamA.Name} срещу {match.TeamB.Name}: {match.ScoreA}:{match.ScoreB}"
            //);


            return RedirectToAction("Index");
        }


        // GET: Matches
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            //if ((String)TempData["NonDisplay"] != "Yes")
            //{
            var now = DateTime.Now;

            var matches = await _context.Matches
                .Include(m => m.TeamA)
                .Include(m => m.TeamB)
                .OrderBy(m => m.PlayedOn)
                .ToListAsync();

            // 🔁 Обновяване на отложени
            foreach (var match in matches)
            {
                if (match.PlayedOn < now && match.ScoreA == null && match.ScoreB == null)
                {
                    match.IsPostponed = true;
                }
            }

            await _context.SaveChangesAsync();

            var tourType = this._context
                .Tournaments
                .Where(t => t.IsActive == true)
                .Select(t => t.Name)
                .FirstOrDefault();

            ViewData["TournamentType"] = tourType;
            return View(matches);
            //}
            //else
            //{
            //    TempData["NoDisplay"] = "Все още няма създаден График?!";
            //    return RedirectToAction("Index", "Home");
            //}

        }

        // GET: Matches/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            if (!_context.Teams.Any())
            {
                var model = new MatchFormModel
                {
                    PlayedOn = DateTime.Now,
                    Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                };

                return View(model);
            }

            return View();
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(MatchFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });
                return View(model);
            }

            if (model.TeamAId == model.TeamBId)
            {
                ModelState.AddModelError("", "Не можеш да избираш един и същ отбор два пъти.");
                model.Teams = (List<SelectListItem>)_context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });
                return View(model);
            }

            var match = new Match
            {
                TeamAId = model.TeamAId,
                TeamBId = model.TeamBId,
                PlayedOn = model.PlayedOn
            };

            if (model.PlayedOn <= DateTime.Now)
            {
                match.ScoreA = model.ScoreA;
                match.ScoreB = model.ScoreB;
            }
            else
            {
                TempData["Message"] = "Мачът е в бъдещето – резултатът ще бъде маркиран като 'Предстои'.";
            }

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);

            var model = new MatchViewModel
            {
                Id = match.Id,
                TeamA = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamB = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
                PlayedOn = (DateTime)match.PlayedOn,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);

            var model = new MatchViewModel
            {
                Id = match.Id,
                TeamA = teams.ContainsKey(match.TeamAId) ? teams[match.TeamAId] : "???",
                TeamB = teams.ContainsKey(match.TeamBId) ? teams[match.TeamBId] : "???",
                PlayedOn = (DateTime)match.PlayedOn,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            _context.Matches.Remove(match);
            //await _context.SaveChangesAsync();

            TempData["Message"] = "Мачът NE беше успешно изтрит. Commented Save Changes";
            return RedirectToAction(nameof(Index));
        }


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

            var requests = await _context.ManagerRequests
                .Include(r => r.Team)
                .Where(r => r.IsApproved && r.FeePaid && r.TournamentId == tournamentId)
                .ToListAsync();

            if (requests.Count != 4)
            {
                TempData["Message"] = "❌ Необходим е точно 4 отбора за графика.";
                return RedirectToAction("Index", "Home");
            }

            // Изтриваме всички предишни мачове за турнира
            var oldMatches = _context.Matches
                .Where(m => m.TournamentId == tournamentId);
            _context.Matches.RemoveRange(oldMatches);

            // Взимаме и разбъркваме отборите
            var teams = requests.Select(r => r.Team).ToList();
            var rng = new Random();
            teams = teams.OrderBy(t => rng.Next()).ToList();

            var matches = new List<Match>();

            // Генерираме Double Round-Robin чрез helper
            var firstLeg = GenerateRoundRobin(teams);
            var secondLeg = GenerateRoundRobin(teams, reverseHomeAway: true);

            var allRounds = firstLeg.Concat(secondLeg).ToList();

            for (int round = 0; round < allRounds.Count; round++)
            {
                var matchDay = tournament.StartDate.AddDays(7 * round);

                foreach (var (home, away) in allRounds[round])
                {
                    matches.Add(new Match
                    {
                        TeamAId = home.Id,
                        TeamBId = away.Id,
                        TournamentId = tournamentId,
                        PlayedOn = matchDay.Date
                    });
                }
            }

            _context.Matches.AddRange(matches);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"✅ Графикът беше успешно генериран: {matches.Count} мача.";
            return RedirectToAction("Index", "Matches");
        }

        private List<List<(Team Home, Team Away)>> GenerateRoundRobin(List<Team> teams, bool reverseHomeAway = false)
        {
            var rounds = new List<List<(Team, Team)>>();

            var teamCount = teams.Count;
            var teamList = new List<Team>(teams);

            if (teamCount % 2 != 0)
            {
                teamList.Add(null); // dummy team
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
