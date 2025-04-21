using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tournament.Data; // или TurnirDbContext
using Tournament.Models; // за ManagerRequest, TournamentType, User


[Authorize(Roles = "Admin")]
public class AdminApprovalModel : PageModel
{
    private readonly TurnirDbContext _context;

    public AdminApprovalModel(TurnirDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = await _context.ManagerRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == UserId);

        if (request == null)
        {
            return NotFound();
        }

        request.IsApproved = true;
        request.Status = RequestStatus.Approved;

        await _context.SaveChangesAsync();

        // Изпращане на SMS (примерно)
        var message = $"Вашият код за регистриране на отбор е: {request.UserId}";

        if (!string.IsNullOrWhiteSpace(request.User.PhoneNumber))
        {
            //await _smsSender.SendSmsAsync(request.User.PhoneNumber, message);
        }

        TempData["Message"] = "Заявката беше одобрена и SMS беше изпратен.";
        return RedirectToPage("/Index");
    }
}
