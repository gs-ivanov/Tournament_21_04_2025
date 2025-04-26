namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class TestEmailController : Controller
    {

        public async Task<IActionResult> Send()
        {

            return RedirectToAction("Index", "Home");
            //var to = "gs.ivanov50@gmail.com"; // замени с твоя адрес
            //var subject = "⚽ Тестване на Email от Tournament";
            //var body = "Това е тестово известие от приложението Tournament.\nУспешно сме свързали Gmail SMTP.";

            ////await _emailSender.SendAsync(to, subject, body);

            //return Content("✅ Изпратено успешно!");
        }

        public IActionResult Test()
        {
           return View();
        
        }
    }
}
