namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class TestSmsController : Controller
    {

        //public async Task<IActionResult> Send()
        public IActionResult Send()
        {
            var to = "+359885773102"; // или друг твой номер
            var message = "⚽ Test SMS от Tournament чрез Twilio.";

            //await ISmsSender.SendSmsAsync(to, message);

            return Content("✅ SMS изпратен успешно!");
        }
    }
    //public void ConfirmFormExample()
    //{
    //    public void Confirm_Test()
    //    {
    //        if (true)
    //        {
    //            TempData["ConfirmMessage"] = "Ще изтриеш всички текущи мачове за турнира и ще създадеш нов график. Сигурен ли си?";
    //            TempData["ConfirmAction"] = "ConfirmDeleteSchedule"; // POST метод за изтриване и създаване
    //            TempData["ConfirmController"] = "Tournaments";

    //            //return RedirectToAction("Confirm", "Tournaments");
    //        }
    //    }

    //    [HttpGet]
    //    public IActionResult Confirm()
    //    {
    //        return View("~/Views/Shared/Confirm.cshtml");
    //    }

    //    [HttpPost]
    //    public async Task<IActionResult> ConfirmDeleteSchedule()
    //    {
    //        var matches = await _context.Matches
    //            .Where(m => m.TournamentId == 1)  //activeTournamentId) // подмени със съответен ид
    //        .ToListAsync();

    //        _context.Matches.RemoveRange(matches);
    //        await _context.SaveChangesAsync();

    //        TempData["Message"] = "✅ Всички стари мачове бяха изтрити успешно.";
    //        return RedirectToAction("Index", "Tournaments");
    //    }

    //Confirm.cshtml
    //@{
    //    ViewData["Title"] = "Потвърждение";
    //    var message = TempData["ConfirmMessage"] as string ?? "Сигурен ли сте?";
    //    var confirmAction = TempData["ConfirmAction"] as string ?? "Index";
    //    var controller = TempData["ConfirmController"] as string ?? "Home";
    //}

    //< div class= "container text-center mt-5" >
    //    < h4 > @message </ h4 >

    //    < div class= "mt-4" >
    //        < form method = "post" asp - controller = "@controller" asp - action = "@confirmAction" >
    //            < button type = "submit" class= "btn btn-danger mx-2" > Да </ button >
    //            < a asp - action = "Index" asp - controller = "@controller" class= "btn btn-success mx-2" > Не </ a >
    //            < a asp - action = "Index" asp - controller = "@controller" class= "btn btn-secondary mx-2" > Назад </ a >
    //        </ form >
    //    </ div >
    //</ div >


    //}
}
