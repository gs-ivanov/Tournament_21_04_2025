namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tournament.Data;
    using Tournament.Models;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public class VerifyCodeController : Controller
    {
        private readonly TurnirDbContext _context;
        private readonly TwilioSettings _twilioSettings;

        public VerifyCodeController(TurnirDbContext context,
            IOptions<TwilioSettings> twilioOptions
            )
        {
            this._twilioSettings = twilioOptions.Value;
            this._context = context;
        }

        [Authorize(Roles = "Editor")]
        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "❌ Заявката не беше намерена.";
                return RedirectToAction("EnterCode");
            }

            // Изтриваме свързания отбор (ако има)
            if (request.Team != null)
            {
                _context.Teams.Remove(request.Team);
            }

            // Изтриваме самата заявка
            _context.ManagerRequests.Remove(request);

            await _context.SaveChangesAsync();

            TempData["VerifyCode"] = "✅ Кандидат-мениджърът беше отстранен.";
            return RedirectToAction("EnterCode");
        }


        [HttpGet]
        public IActionResult EnterCode()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterCode(string email, string receiptNumber)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(receiptNumber))
            {
                TempData["Error"] = "Моля, попълнете всички полета.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Невалиден имейл.";
                return View();
            }

            var isNotApproved = await _context.ManagerRequests
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.User.Email == email && !r.IsApproved);

            if (isNotApproved == null)
            {
                    TempData["Error"] = "Няма чакаща заявка за този имейл.";
                    return RedirectToAction("EnterCode");
            }

            // Потвърждаваме заявката
            isNotApproved.IsApproved = true;
            isNotApproved.FeePaid = true;

            //// Depricated yet!!!!. Добавяме отбора към турнира, ако още не е
            //var tournament = await _context.Tournaments
            //    //.Include(t => t.Teams)
            //    .FirstOrDefaultAsync(t => t.Id == request.TournamentId);

            //if (tournament != null)
            //{
            //    var team = await _context.Teams.FindAsync(request.TeamId);
            //    if (team != null)
            //    {
            //        tournament.Teams.Add(team);
            //    }
            //}

            // Записваме промените
            await _context.SaveChangesAsync();

            // ✅ Проверка за точно 4 отбора, свързани с турнира

            var numberApprovedTeams = this._context.ManagerRequests
                .Count(r => r.IsApproved);

            if (numberApprovedTeams >= 4 && numberApprovedTeams % 2 == 0)
            {
                TempData["Message"] = $"Можеш да генерираш график с {numberApprovedTeams} отбора. Логвай се като админ и от падащо меню инициирай генерирането.";
            }
            var message = $"✅ Номера на вносната бележка за превод по IBAN No: {receiptNumber} е приета.\nДобре дошъл и успешно представяне!";
            TempData["VerifyCode"] = message;

            return RedirectToAction("Index", "Home");

            //// Изпращаме SMS
            //await _smsSender.SendSmsAsync("+359885773102", $"✅ Отборът {request.Team.Name} е включен в турнира {tournament.Name}.");

        }

        [HttpGet]
        public IActionResult TestSms()
        {
            List<int> a = new List<int> { 1 };
            return View(a);
        }

        [HttpPost]
        public IActionResult TestSms(IEnumerable<int> a)
        {
            var x = a.First();
            return Ok(x);
        }
        //[HttpGet]
        //public IActionResult TestSms()
        //{
        //    //try
        //    //{
        //    //    TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

        //    //    var message = MessageResource.Create(
        //    //        body: "Hello World from Tournament (test mode)",
        //    //        from: new PhoneNumber(_twilioSettings.FromNumber),
        //    //        to: new PhoneNumber("+15005550006") // test recipient (Twilio test only)
        //    //    );

        //    //    return Content($"✅ Тестово SMS съобщение изпратено! SID: {message.Sid}, Status: {message.Status}");
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return Content($"❌ Грешка: {ex.Message}");
        //    //}
        //}

        //[HttpGet]
        //public IActionResult CheckTwilioConfig()
        ////{
        ////    return Content($"SID: {_twilioSettings.AccountSid}, Token: {_twilioSettings.AuthToken}, From: {_twilioSettings.FromNumber}");
        ////}


    }
}
