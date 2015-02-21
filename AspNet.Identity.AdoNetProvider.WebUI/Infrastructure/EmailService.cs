using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.AdoNetProvider.WebUI.Infrastructure
{
    public class EmailService : IIdentityMessageService
    {
        private readonly EmailManager _emailManager;

        public EmailService()
        {
            _emailManager = new EmailManager();
        }

        public EmailService(EmailManager emailManager)
        {
            _emailManager = emailManager;
        }

        public Task SendAsync(IdentityMessage message)
        {
            return Task.Run(() => _emailManager.SendMailAsync(message.Destination, message.Subject, message.Body));
        }
    }
}