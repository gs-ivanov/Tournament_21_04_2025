namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Models;

    [Authorize(Roles = "Administrator")]
    public class ManagerRequestsController : Controller
    {
        private readonly TurnirDbContext _context;

        public ManagerRequestsController(TurnirDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string status = "Pending")
        {
            var query = _context.ManagerRequests
                    .Include(r => r.Team)
                    .Include(r => r.User)
                    .AsQueryable();

                if (Enum.TryParse<RequestStatus>(status, out var parsedStatus))
                {
                    query = query.Where(r => r.Status == parsedStatus);
                }

                ViewData["CurrentStatus"] = status;
            return View(await query.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            request.Status = RequestStatus.Approved;
            request.Team.FeePaid = true;
            request.FeePaid = true;
            request.IsApproved=true;

            await _context.SaveChangesAsync();

            // Изпращане на имейл известие
            var link = Url.Action("Download", "Certificates", new { teamId = request.TeamId }, Request.Scheme);
            var subject = "Удостоверение за участие в турнир";
            var body = $"Уважаеми {request.User.FullName},\n\nВашата заявка за участие с отбор \"{request.Team.Name}\" беше одобрена.\n\nМожете да изтеглите удостоверението си от следния линк:\n{link}\n\nПоздрави,\nЕкипът на Tournament";

            //await _emailSender.SendAsync(request.User.Email, subject, body); // ✅ правилен ред 

            TempData["Message"] = $"✅ Заявката от {request.User.FullName} за отбор '{request.Team.Name}' беше одобрена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.User)
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            request.Status = RequestStatus.Rejected;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"❌ Заявката от {request.User.FullName} за отбор '{request.Team.Name}' беше отхвърлена.";
            return RedirectToAction(nameof(Index));
        }

        public static string GenerateJson(string email, TournamentType tournamentType)
        {
            var payload = new
            {
                Email = email,
                TournamentType = tournamentType.ToString(),
                RequestedAt = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
