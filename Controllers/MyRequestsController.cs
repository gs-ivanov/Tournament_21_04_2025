namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Infrastructure.Extensions;

    [Authorize(Roles = "Editor")]
    public class MyRequestsController : Controller
    {
        private readonly TurnirDbContext _context;

        public MyRequestsController(TurnirDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Id();

            var requests = await _context.ManagerRequests
                .Include(r => r.Team)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedOn)
                .ToListAsync();

            return View(requests);
        }
    }
}
