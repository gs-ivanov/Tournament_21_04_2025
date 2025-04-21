namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PdfSharpCore.Drawing;
    using PdfSharpCore.Pdf;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Infrastructure.Extensions;
    using Tournament.Models;

    [Authorize(Roles = "Editor")]
    public class CertificatesController : Controller
    {
        private readonly TurnirDbContext _context;

        public CertificatesController(TurnirDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ForTeam(int teamId)
        {
            var userId = User.Id();

            var team = await _context.Teams
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.UserId == userId);

            if (team == null)
                return NotFound();

            var request = await _context.ManagerRequests
                .Where(r => r.TeamId == teamId && r.Status == RequestStatus.Approved)
                .OrderByDescending(r => r.CreatedOn)
                .FirstOrDefaultAsync();

            if (request == null || !team.FeePaid)
            {
                return View("NotApproved");
            }

            return View("Certificate", request);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int teamId)
        {
            var userId = User.Id();

            var team = await _context.Teams
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.UserId == userId);

            if (team == null)
                return NotFound();

            var request = await _context.ManagerRequests
                .Where(r => r.TeamId == teamId && r.Status == RequestStatus.Approved)
                .OrderByDescending(r => r.CreatedOn)
                .FirstOrDefaultAsync();

            if (request == null || !team.FeePaid)
            {
                return BadRequest("Неодобрена заявка или неплатена такса.");
            }

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);

            gfx.DrawString("Удостоверение за участие", font, XBrushes.Black, new XRect(0, 40, page.Width, 40), XStringFormats.TopCenter);
            gfx.DrawString($"Отбор: {team.Name}", font, XBrushes.Black, 40, 100);
            gfx.DrawString($"Треньор: {team.CoachName}", font, XBrushes.Black, 40, 130);
            gfx.DrawString($"Мениджър: {team.User.FullName} ({team.User.Email})", font, XBrushes.Black, 40, 160);
            gfx.DrawString($"Тип турнир: {request.TournamentType}", font, XBrushes.Black, 40, 190);
            gfx.DrawString($"Дата: {request.CreatedOn:dd.MM.yyyy HH:mm}", font, XBrushes.Black, 40, 220);
            gfx.DrawString("Такса: Платена ✅", font, XBrushes.Black, 40, 250);

            using var stream = new MemoryStream();
            doc.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", $"Certificate_{team.Name}.pdf");
        }
    }
}
