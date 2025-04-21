namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models.Matches;

    public class TournamentsOldOOOOOOController : Controller
    {
        private readonly TurnirDbContext _context;

        public TournamentsOldOOOOOOController(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //var dataTournaments = this._context
            //    .Tournaments
            //    .Select(t => new MatchViewModel){

            //}
            var tournaments = await _context.Tournaments
                .Include(t => t.Matches) // ← това е важното за проверката "има мачове"
                .ToListAsync();

            //return View(tournaments);  //ToDo
            TempData["Message"] = "✅ Demo test for Commented  List/Edit of available tournaments /ToDo/.";
            return RedirectToAction(nameof(Index),"Home");

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (!ModelState.IsValid)
            {
                return View(tournament);
            }

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Demo test for CommentedТурнирът е създаден успешно.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (tournament.Matches.Any())
            {
                TempData["Error"] = "❌ Турнирът има свързани мачове и не може да бъде изтрит.";
                return RedirectToAction("Index");
            }

            //_context.Tournaments.Remove(tournament);
            //await _context.SaveChangesAsync();

            TempData["Message"] = "Only Demo NOT=> ✅ Турнирът е изтрит успешно.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
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
        public async Task<IActionResult> Edit(int id, Tournament updated)
        {
            if (id != updated.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updated);
            }

            try
            {
                _context.Tournaments.Update(updated);
                await _context.SaveChangesAsync();

                TempData["Message"] = "✅ Турнирът е обновен успешно.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "⚠ Грешка при обновяване на турнира.";
                return View(updated);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult SelectForSchedule()
        {
            if (this._context.ManagerRequests.Count()<4)
            {
                TempData["Message"] = "Невъзможно създаване на турнир. Поне 4 отбора трябва да са одобрени за включване в турнир. Виж <Заявки за участие в турнири>";
                return RedirectToAction(nameof(Index),"Home");
            }

            var eligibleTournaments = _context.Tournaments
                .Include(t => t.Teams)
                .Where(t => t.Teams.Count == 4)
                .ToList();

            ViewBag.Tournaments = new SelectList(eligibleTournaments, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSchedule(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null || tournament.Teams.Count != 4)
            {
                TempData["Error"] = "Турнирът не съществува или няма точно 4 отбора подходящи за включване в турнир.";
                return RedirectToAction("SelectForSchedule");
            }

            var teams = tournament.Teams.ToList();

            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = i + 1; j < teams.Count; j++)
                {
                    var match = new Match
                    {
                        TournamentId = tournamentId,
                        TeamAId = teams[i].Id,
                        TeamBId = teams[j].Id,
                        PlayedOn = DateTime.Now.AddDays(i + j)
                    };
                    _context.Matches.Add(match);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Графикът беше успешно генериран.";
            return RedirectToAction("Index", "Matches");
        }

    }
}
