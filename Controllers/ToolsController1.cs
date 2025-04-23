namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> ResetTeamsAndRequests()
        {
            TempData["ToolsReset"] = "Reset Teams and ManagerRequests items: FeePaid, Status, IsApproved and FeePaid.";
            var teams = await _context.Teams.ToListAsync();
            foreach (var team in teams)
            {
                team.FeePaid = false;
                team.TournamentId = 0;
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
    }
}
