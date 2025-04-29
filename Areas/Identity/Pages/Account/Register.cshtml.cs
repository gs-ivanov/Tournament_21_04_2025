namespace Tournament.Areas.Identity.Pages.Account
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Data.Models;
    using Tournament.Models;



    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly TurnirDbContext context;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TurnirDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Имейл")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Парола")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Потвърди парола")]
            [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; }

            [Phone]
            [Display(Name = "Телефонен номер")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Пълно име")]
            public string FullName { get; set; }

            [Display(Name = "Стани мениджър")]
            public bool BecomeManager { get; set; }

            [Display(Name = "Избери отбор")]
            public int? TeamId { get; set; }

            public SelectList AvailableTeams { get; set; }
        }

        public SelectList AvailableTeams { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!context.Teams.Any())
            {
                TempData["Error"] = "Няма записани отбори в БД. Изпълни URL команда /Teams/CreateMultiple за добаяне на отборите";
                //return RedirectToPage("/Index"); // или където искаш да го насочиш
                return RedirectToPage("Teams/CreateMultiple"); // или където искаш да го насочиш
            }

            AvailableTeams = new SelectList(
                await context.Teams
                    .Where(t => t.UserId == null)
                    .Select(t => new { t.Id, t.Name })
                    .ToListAsync(),
                "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await OnGetAsync();
                return Page();
            }

            if (Input.BecomeManager)
            {
                await userManager.AddToRoleAsync(user, "Editor");

                var team = await context.Teams.FirstOrDefaultAsync(t => t.Id == Input.TeamId && t.UserId == null);
                if (team == null)
                {
                    ModelState.AddModelError("", "Избраният отбор вече е зает. Моля, опитайте с друг.");
                    await OnGetAsync();
                    return Page();
                }

                var tournament = await context.Tournaments
                    .Where(t => t.IsOpenForApplications)
                    .OrderBy(t => t.StartDate)
                    .FirstOrDefaultAsync();

                if (tournament == null)
                {
                    TempData["Error"] = "Няма активен турнир от тип 'Всеки срещу всеки'.";
                    return RedirectToPage("/Index");
                }

                team.UserId = user.Id;
                team.Tournament = tournament;
                team.TournamentId = tournament.Id;

                var request = new ManagerRequest
                {
                    UserId = user.Id,
                    TeamId = team.Id,
                    TournamentType = TournamentType.RoundRobin,
                    TournamentId = tournament.Id,
                    JsonPayload = $"{{ \"email\": \"{user.Email}\" }}",
                    Status = RequestStatus.Pending,
                    IsApproved = false,
                    FeePaid = false
                };

                context.ManagerRequests.Add(request);
                await context.SaveChangesAsync();

                //var smsText = $"✅ Заявката ви за участие в турнир \"Всеки срещу всеки\" е приета.\nСлед превод по IBAN: BG00XXXX00000000000000, въведете имейл {user.Email} във формата за потвърждение.";
                //var phone = user.PhoneNumber ?? "+359885773102";
                //await smsSender.SendSmsAsync(phone, smsText);
                //TempData["Message"] = $"Изпратен СМС на телефонен нномер {phone}.";
                TempData["Register"] = "✅ Регистрацията е завършена. Направете банков превод на сметката на Федерацията по футбол IBAN :XXXXXXXXXXXX , влезте в профила си и въведете номера на вносната бележка.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                //await userManager.AddToRoleAsync(user, "Fan");
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("/Index");
        }
    }
}
