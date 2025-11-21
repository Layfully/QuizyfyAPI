using Microsoft.Extensions.Options;
using QuizyfyAPI.Data;
using QuizyfyAPI.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace QuizyfyAPI.Services;

public class SendGridService : ISendGridService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly SendGridOptions _sendGridOptions;
    private readonly AppOptions _appOptions;
    private readonly ILogger<SendGridService> _logger;

    public SendGridService(
        ISendGridClient sendGridClient, 
        IOptions<SendGridOptions> sendGridOptions,
        IOptions<AppOptions> appOptions,
        ILogger<SendGridService> logger)
    {
        _sendGridClient = sendGridClient;
        _sendGridOptions = sendGridOptions.Value;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task<Response> SendConfirmationEmailTo(User user)
    {
        string baseUrl = _appOptions.ServerPath.TrimEnd('/');
        string confirmationLink = $"{baseUrl}/confirm/{user.Id}/{user.VerificationToken}";
        
        string htmlContent = $"<a href='{confirmationLink}'>Potwierdź email.</a>";
        
        return await SendEmailAsync(
            user.Email, 
            _sendGridOptions.RegistrationInfo.Subject, 
            _sendGridOptions.RegistrationInfo.PlainContent, 
            htmlContent
        );
    }

    public async Task<Response> SendPasswordResetEmailTo(User user)
    {
        string baseUrl = _appOptions.ServerPath.TrimEnd('/');
        string resetLink = $"{baseUrl}/resetPassword/{user.Id}/{user.RecoveryToken}";
        
        string htmlContent = $"<a href='{resetLink}'>Zresetuj hasło.</a>";

        return await SendEmailAsync(
            user.Email, 
            _sendGridOptions.PasswordResetInfo.Subject, 
            _sendGridOptions.PasswordResetInfo.PlainContent, 
            htmlContent
        );
    }
    
    private async Task<Response> SendEmailAsync(string toEmail, string subject, string plainContent, string htmlContent)
    {
        EmailAddress from = new(_sendGridOptions.HostEmail, "Quizyfy");
        EmailAddress to = new(toEmail);
        
        SendGridMessage? message = MailHelper.CreateSingleEmail(from, to, subject, plainContent, htmlContent);
        
        try 
        {
            var response = await _sendGridClient.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {Status}. Body: {Body}", toEmail, response.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("Email sent to {Email}", toEmail);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            throw;
        }
    }
}