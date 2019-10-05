using Microsoft.Extensions.Options;
using QuizyfyAPI.Data;
using QuizyfyAPI.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public class SendGridService : ISendGridService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridOptions _sendGridOptions;

        public SendGridService(ISendGridClient sendGridClient, IOptions<SendGridOptions> sendGridOptions)
        {
            _sendGridClient = sendGridClient;
            _sendGridOptions = sendGridOptions.Value;
        }

        public Task<Response> SendEmailTo(User user)
        {
            var from = new EmailAddress(_sendGridOptions.HostEmail);
            var to = new EmailAddress(user.Email);
            var subject = _sendGridOptions.RegistrationInfo.Subject;
            var plainContent = _sendGridOptions.RegistrationInfo.PlainContent;
            var htmlContent = $"<a href='http://localhost:8080/confirm/{user.Id}/{user.VerificationToken}'>Potwierdź test.</a>";
            var email = MailHelper.CreateSingleEmail(from, to, subject, plainContent, htmlContent);
            return _sendGridClient.SendEmailAsync(email);
        }
    }
}
