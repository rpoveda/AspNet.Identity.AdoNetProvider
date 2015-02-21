using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace AspNet.Identity.AdoNetProvider.WebUI.Infrastructure
{
    public class EmailManager
    {
        private readonly MailMessage _mail;
        private readonly SmtpClient _smtpClient;

        /// <summary>
        ///     Constructor
        /// </summary>
        public EmailManager()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var mailSettings = (MailSettingsSectionGroup)configuration.GetSectionGroup("system.net/mailSettings");

            if (mailSettings == null)
            {
                return;
            }

            _mail = new MailMessage
            {
                From = new MailAddress(mailSettings.Smtp.From)
            };

            _smtpClient = new SmtpClient(mailSettings.Smtp.Network.Host)
            {
                Credentials = new NetworkCredential(mailSettings.Smtp.Network.UserName, mailSettings.Smtp.Network.Password),
                Port = mailSettings.Smtp.Network.Port
            };
        }

        /// <summary>
        ///     Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="recepientMails">
        ///     The mail addresses of the recepients. If multiple addresses are specified,
        ///     they must be separated by the comma character
        /// </param>
        /// <param name="subject">The subject of the email</param>
        /// <param name="fileName">The name of the file that has the mail text</param>
        /// <param name="parameters">Parameters to pass in the mail text</param>
        public async Task<bool> SendMailAsync(string recepientMails, string subject, string fileName, Dictionary<string, string> parameters)
        {
            var filePath = HostingEnvironment.MapPath("~/Content/email_templates/");

            try
            {
                string mailText;

                using (var streamReader = new StreamReader(filePath + fileName))
                {
                    mailText = streamReader.ReadToEnd();
                }

                mailText = parameters.Aggregate(mailText, (current, item) => current.Replace(item.Key, item.Value));
                _mail.To.Add(recepientMails);
                _mail.Subject = subject;
                _mail.Body = mailText;
                _mail.IsBodyHtml = true;

                await _smtpClient.SendMailAsync(_mail);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendMailAsync(string recepientMail, string subject, string body)
        {
            try
            {
                _mail.To.Add(recepientMail);
                _mail.Subject = subject;
                _mail.Body = body;
                _mail.IsBodyHtml = true;

                await _smtpClient.SendMailAsync(_mail);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}