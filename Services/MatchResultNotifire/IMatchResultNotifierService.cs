namespace Tournament.Services.MatchResultNotifire
{
    using System.Threading.Tasks;

    public interface IMatchResultNotifierService
    {
        public Task NotifyAsync(int matchId);

        //private Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
