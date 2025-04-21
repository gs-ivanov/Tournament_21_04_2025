using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tournament.Data;
using Tournament.Data.Models;

public class PostRegisterModel : PageModel
{
    private readonly TurnirDbContext _context;
    private readonly UserManager<User> _userManager;

    public PostRegisterModel(TurnirDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public ManagerRequest Request { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToPage("/Account/Login");

        Request = await _context.ManagerRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == user.Id);

        if (Request == null)
            return RedirectToPage("/Index");

        return RedirectToPage("/Account/PostRegister");
    }
}
