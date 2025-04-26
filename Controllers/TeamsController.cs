namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Services.PDF;
    using Tournament.Models.Teams;
    using Tournament.Infrastructure.Extensions;
    using System;

    public class TeamsController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly PdfService pdfService;

        public TeamsController(TurnirDbContext context, PdfService pdfService)
        {
            _context = context;
            this.pdfService = pdfService;
        }

        public async Task<IActionResult> Index()
        {
                var teams = await _context.Teams
                .Include(t => t.MatchesAsTeamA)
                .Include(t => t.MatchesAsTeamB)
                .Include(t => t.ManagerRequests)
                .ToListAsync();

                return View(teams);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CoachName = t.CoachName,
                    LogoUrl = t.LogoUrl,
                    FeePaid = t.FeePaid
                })
                .FirstOrDefaultAsync();

            if (team == null)
                return NotFound();

            return View(team);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!TempData.ContainsKey("VerifiedManagerId"))
            {
                TempData["Error"] = "Нямате достъп до създаване на отбор. Въведете код за достъп.";
                //return RedirectToAction("EnterCode", "VerifyCode");
            }

            TempData.Keep("VerifiedManagerId"); // запазваме за POST

            return View(new CreateTeamViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel model)
        {
            //if (!TempData.ContainsKey("VerifiedManagerId"))
            //{
            //    TempData["Error"] = "Нямате достъп до създаване на отбор.";
            //    return RedirectToAction("EnterCode", "VerifyCode");
            //}

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //var userId = TempData["VerifiedManagerId"].ToString();

            //var team = new Team
            //{
            //    Name = model.Name,
            //    CoachName = model.CoachName,
            //    LogoUrl = model.LogoUrl,
            //    FeePaid = true,
            //    UserId = userId
            //};

            //_context.Teams.Add(team);
            //await _context.SaveChangesAsync();

            // Актуализираме заявката
            //var request = await _context.ManagerRequests.FirstOrDefaultAsync(r => r.UserId == userId);
            //if (request != null)
            //{
            //    request.TeamId = team.Id;
            //    await _context.SaveChangesAsync();
            //}

            // PDF сертификат
            var teamName = "Cherno More";
            var TournamentType = "RoundRobin";
            var certificateId = Guid.NewGuid().ToString().Substring(0, 8);
            var html = $@"
                    <html>
                    <head><style>body {{ font-family: Arial; padding: 40px; }}</style></head>
                    <body>
                        <h1>СЕРТИФИКАТ ЗА УЧАСТИЕ</h1>
                        <p>Отбор: <strong>{teamName}</strong></p>
                        <p>Тип турнир: <strong>{TournamentType}</strong></p>
                        <p>Дата: {DateTime.Now:dd.MM.yyyy}</p>
                        <p>Сертификат №: <strong>{certificateId}</strong></p>
                    </body>
                    </html>";

            var pdfBytes = pdfService.GeneratePdfFromHtml(html);

            if (pdfBytes != null && pdfBytes.Length > 0)
            {  //debug
                TempData["Message"] = "Отборът е създаден успешно.";
                return File(pdfBytes, "application/pdf", "sertifikat.pdf");
            }

            TempData["Error"] = "Създаден е отбор, но не беше генериран сертификат.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            var model = new EditTeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                CoachName = team.CoachName,
                LogoUrl = team.LogoUrl,
                FeePaid = team.FeePaid
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(EditTeamViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var team = await _context.Teams.FindAsync(model.Id);
            if (team == null) return NotFound();

            team.Name = model.Name;
            team.CoachName = model.CoachName;
            team.LogoUrl = model.LogoUrl;
            team.FeePaid = model.FeePaid;

            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Отборът \"{team.Name}\" е обновен.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    CoachName = t.CoachName,
                    LogoUrl = t.LogoUrl,
                    FeePaid = t.FeePaid
                })
                .FirstOrDefaultAsync();

            if (team == null)
                return NotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            //_context.Teams.Remove(team);
            //await _context.SaveChangesAsync();

            TempData["Message"] = $"Отборът \"{team.Name}\"не беше изтрит, а само за тест на функцията.";
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }

        // GET: Teams/CreateMultiple
        public IActionResult CreateMultiple()
        {
            return View(new TeamFormModel());
        }

        // POST: Teams/CreateMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(TeamFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Teams.Any())
            {
                TempData["Message"] = "Вече има записи в базата. Изчисти ги преди да добавиш нови.";
                return RedirectToAction(nameof(Index));
            }

            var teams = SeedTeams().Take(model.TeamCount).ToList();

            _context.Teams.AddRange(teams);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Успешно добавени {teams.Count} отбора.";
            return RedirectToAction(nameof(Index));
        }

        private static List<Team> SeedTeams()
        {
            List<string> teamNames = new()
            {
                "Лудогорец",
                "Крумовград",
                "Левски София",
                "Локомотив Пловдив",
                "Славия София",
                "Черно море",
                "Арда",
                "Ботев Враца",
                "ЦСКА София",
                "Септември София",
                "Спартак Варна",
                "Ботев Пловдив",
                "Берое",
                "Хебър",
                "ЦСКА 1948",
                "Миньор Перник"
            };

            List<string> teamLogos = new()
            {
                "/logos/ludogorec.png",
                "/logos/krumovgrad.png",
                "/logos/levski.png",
                "/logos/lokomotivplovdiv.png",
                "/logos/slavia.png",
                "/logos/chernomore.png",
                "/logos/arda.png",
                "/logos/botevvraca.png",
                "/logos/cskasofia.png",
                "/logos/septemvri.png",
                "/logos/spartakvarna.png",
                "/logos/botevplovdiv.png",
                "/logos/beroe.png",
                "/logos/hebar.png",
                "/logos/cska1948.png",
                "/logos/minyor.png"
            };

            var teams = new List<Team>();
            for (int i = 0; i < teamNames.Count; i++)
            {
                teams.Add(new Team
                {
                    Name = teamNames[i],
                    CoachName = "Н/Д",
                    FeePaid = false,
                    LogoUrl = teamLogos[i]
                });
            }

            return teams;
        }
    }
}
