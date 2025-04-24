namespace Tournament.Areas.Identity.Pages.Account
{
    using global::Tournament.Data;
    using global::Tournament.Data.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly TurnirDbContext context;




        public LoginModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TurnirDbContext context
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }
        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember Me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                var roles = await userManager.GetRolesAsync(user);

                // 🔁 Пренасочване на администратор при липса на активен турнир

                if (roles.Contains("Administrator"))
                {
                    var activeTournament = await context.Tournaments
                        .FirstOrDefaultAsync(t => t.IsActive);

                    var tournamentUsedForMatches = await context.Matches
                        .Select(m => m.TournamentId)
                        .Distinct()
                        .FirstOrDefaultAsync();

                    if (activeTournament == null || activeTournament.Id != tournamentUsedForMatches)
                    {
                        TempData["Message"] = $"Здравей, Администраторе!\nВсе още няма активен турнир или той не съвпада с турнира, използван за графика (активен: {activeTournament?.Id}, график: {tournamentUsedForMatches}).";
                        return RedirectToAction("Index", "Home");
                    }

                    TempData["Message"] = $"Здравей, Администраторе!\nТекущ турнир: {activeTournament.Name} ({activeTournament.Type})";
                    return RedirectToAction("Index", "Home");
                }
                // Проверка: ако не е Editor или Administrator
                var isManagerLike = !roles.Contains("Editor") && !roles.Contains("Administrator");

                if (isManagerLike)
                {
                    var approvedRequest = await context.ManagerRequests
                        .Include(r => r.Team)
                        .FirstOrDefaultAsync(r =>
                            r.UserId == user.Id &&
                            r.IsApproved);

                    if (approvedRequest != null)
                    {
                        TempData["VerifiedManagerId"] = user.Id;
                        return RedirectToAction("EnterCode", "VerifyCode");
                    }
                }

                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Грешен опит за вход.");
            return Page();
        }
    }
}