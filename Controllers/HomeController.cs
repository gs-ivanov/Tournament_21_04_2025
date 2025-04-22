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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var activeTournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.IsActive);

            if (activeTournament != null)
            {
                var matchesExist = await _context.Matches
                    .AnyAsync(m => m.TournamentId == activeTournament.Id);

                if (matchesExist)
                {
                    //// Генерирай класиране...
                    //var rankings = await GenerateRankings(activeTournament.Id);
                    //return View(rankings);
                }
            }

            // Ако няма активен турнир
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ShowWelcome"] = true;
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (User.IsInRole("Administrator"))
            {
                TempData["Message"] = "Добре дошли, Администратор! Все още няма активен турнир. Създайте нов.";
            }
            else
            {
                TempData["Message"] = "В момента няма активен турнир. Моля, върнете се по-късно.";
            }

            return View();
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
