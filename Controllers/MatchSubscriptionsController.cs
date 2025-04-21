namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Tournament.Data;
    using Tournament.Data.Models;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Models;

    [Authorize]
    public class MatchSubscriptionsController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly UserManager<User> _userManager;

        public MatchSubscriptionsController(TurnirDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(int matchId, NotificationType type)
        {
            var user = await _userManager.GetUserAsync(User);

            bool alreadySubscribed = await _context.MatchSubscriptions
                .AnyAsync(ms => ms.MatchId == matchId && ms.UserId == user.Id);

            if (!alreadySubscribed)
            {
                var subscription = new MatchSubscription
                {
                    UserId = user.Id,
                    MatchId = matchId,
                    Type = type
                };

                _context.MatchSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Успешно се абонирахте за мача.";
            }
            else
            {
                TempData["Message"] = "Вече сте абонирани за този мач.";
            }

            return RedirectToAction("Details", "Matches", new { id = matchId });
        }
    }
}
