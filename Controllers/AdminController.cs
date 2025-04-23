namespace Tournament.Controllers
{
    using global::Tournament.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    namespace Tournament.Controllers
    {
        [Authorize(Roles = "Administrator")]
        public class AdminController : Controller
        {
            private readonly TurnirDbContext _context;

            public AdminController(TurnirDbContext context)
            {
                _context = context;
            }

            // 🔁 RESET на всички заявки и отбори
            [HttpPost]
            public async Task<IActionResult> ResetTeamsAndRequests()
            {
                var teams = await _context.Teams.ToListAsync();
                foreach (var team in teams)
                {
                    team.FeePaid = false;
                }

                var requests = await _context.ManagerRequests.ToListAsync();
                foreach (var req in requests)
                {
                    req.Status = 0;            // Pending
                    req.IsApproved = false;
                    req.FeePaid = false;
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "🔁 Всички заявки и отбори бяха върнати в начално състояние.";
                return RedirectToAction("Index", "Tournaments");
            }

            // ✅ Одобрение на заявка по ID
            [HttpPost]
            public async Task<IActionResult> ApproveRequest(int id)
            {
                var request = await _context.ManagerRequests
                    .Include(r => r.Team)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                    return NotFound();

                request.Status = 1;
                request.IsApproved = true;
                request.FeePaid = true;

                if (request.Team != null)
                {
                    request.Team.FeePaid = true;
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Заявката за отбор '{request.Team?.Name}' е одобрена.";
                return RedirectToAction("Index", "ManagerRequests");
            }
        }
    }
}
