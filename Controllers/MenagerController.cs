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
    using Tournament.Models.Menagers;

    [Authorize(Roles = "Editor")]
    public class MenagerController : Controller
    {
        private readonly TurnirDbContext _context;

        public MenagerController(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == userId);
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            var model = new MenagerDashboardViewModel
            {
                Team = team,
                Request = request
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == userId);

            if (team == null)
            {
                return RedirectToAction("Index");
            }

            var tournaments = await _context.Tournaments
                .Where(t => t.IsOpenForApplications)
                .ToListAsync();

            ViewBag.Tournaments = new SelectList(tournaments, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(int tournamentId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.UserId == userId);

            if (team == null)
            {
                return RedirectToAction("Index");
            }

            var existingRequest = await _context.ManagerRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TournamentId == tournamentId);

            if (existingRequest != null)
            {
                TempData["Message"] = "Вече сте подали заявка за този турнир.";
                return RedirectToAction("Index");
            }

            var request = new ManagerRequest
            {
                UserId = userId,
                TeamId = team.Id,
                TournamentId = tournamentId,
                CreatedOn = DateTime.Now,
                IsApproved = false,
                FeePaid = false
            };

            _context.ManagerRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Заявката е изпратена успешно.";
            return RedirectToAction("Index");
        }
    }

}

