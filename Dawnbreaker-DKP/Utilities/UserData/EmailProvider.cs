using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Dawnbreaker_DKP.Data.Utility;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;

namespace Dawnbreaker_DKP.Utilities.UserData
{
    public class EmailProvider : IEmailProvider
    {
        public string AdminEmailAddress { get; }
        public string AdminEmailAccessToken { get; }

        public EmailProvider(IRepository<KeystoreEntry> keystoreRepository)
        {
            AdminEmailAddress = keystoreRepository.GetWhere(x => x.KeyName == KeyNames.ADMIN_EMAIL_ADDRESS).FirstOrDefault()?.TokenValue ?? "";
            AdminEmailAccessToken = keystoreRepository.GetWhere(x => x.KeyName == KeyNames.ADMIN_EMAIL_ACCESS_TOKEN).FirstOrDefault()?.TokenValue ?? "";
        }

        public void SendRecoveryEmail(string userName, string recoveryEmail, string recoveryTicket) => Task.Run(() =>
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(AdminEmailAddress, AdminEmailAccessToken),
                EnableSsl = true
            })
            {
                using (var mailMessage = new MailMessage(AdminEmailAddress, recoveryEmail)
                {
                    Subject = $"Dawnbreaker DKP - Password Recovery For {userName}",
                    SubjectEncoding = Encoding.UTF8,
                    Body = RecoveryTemplate(userName, recoveryTicket),
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                })
                {
                    try
                    {
                        client.Send(mailMessage);
                    }
                    catch (SmtpException ex)
                    {
                        var logItem = new ExceptionLog
                        {
                            Message = $"Failure to send recovery email for user: {userName}; email: {recoveryEmail}",
                            ExceptionType = ex.GetType().FullName,
                        };
                        RepositoryFactory<ExceptionLog>.SystemRepository().Upsert(logItem);
                    }
                }
            }
        });

        public static string RecoveryTemplate(string userName, string recoveryTicket)=> $@"<!DOCTYPE html>
<html>
<body>
<h3>Hello, {userName}!</h3>
You appear to be rather forgetful. It's okay, we can fix that! Just copy and paste the following recovery ticket to the password recovery form:<br><br>
<strong>{recoveryTicket}</strong><br><br>
<font size=""2"">(If you did not request this recovery email, please ignore and delete this message, and notify the site administrator)</font>
</body>
</html>";

    }
}
