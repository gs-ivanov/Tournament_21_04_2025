namespace Tournament.Controllers
{
    using global::Tournament.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;

    [Authorize(Roles = "Administrator")]
    public class MaintenanceController : Controller
    {
        private readonly TurnirDbContext _context;

        public MaintenanceController(TurnirDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CleanTemporaryTeams()
        {
            var teamsToDelete = await _context.Teams
                .Where(t => t.Name == "Временен отбор")
                .ToListAsync();

            int deletedCount = 0;

            foreach (var team in teamsToDelete)
            {
                var request = await _context.ManagerRequests
                    .FirstOrDefaultAsync(r => r.TeamId == team.Id && !r.IsApproved);

                if (request != null)
                {
                    _context.ManagerRequests.Remove(request);
                }

                _context.Teams.Remove(team);
                deletedCount++;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Премахнати са {deletedCount} временни отбора и свързаните им заявки.";
            return RedirectToAction("Index", "Home");
        }

    }
}
