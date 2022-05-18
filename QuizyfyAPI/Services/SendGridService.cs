﻿using Microsoft.Extensions.Options;
using QuizyfyAPI.Data;
using QuizyfyAPI.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace QuizyfyAPI.Services;
public class SendGridService : ISendGridService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly SendGridOptions _sendGridOptions;
    public SendGridService(ISendGridClient sendGridClient, IOptions<SendGridOptions> sendGridOptions)
    {
        _sendGridClient = sendGridClient;
        _sendGridOptions = sendGridOptions.Value;
    }

    public Task<Response> SendConfirmationEmailTo(User user)
    {
        var from = new EmailAddress(_sendGridOptions.HostEmail);
        var to = new EmailAddress(user.Email);
        var subject = _sendGridOptions.RegistrationInfo.Subject;
        var plainContent = _sendGridOptions.RegistrationInfo.PlainContent;
        var htmlContent = $"<a href='http://localhost:8080/confirm/{user.Id}/{user.VerificationToken}'>Potwierdź email.</a>";
        var email = MailHelper.CreateSingleEmail(from, to, subject, plainContent, htmlContent);
        return _sendGridClient.SendEmailAsync(email);
    }

    public Task<Response> SendPasswordResetEmailTo(User user)
    {
        var from = new EmailAddress(_sendGridOptions.HostEmail);
        var to = new EmailAddress(user.Email);
        var subject = _sendGridOptions.PasswordResetInfo.Subject;
        var plainContent = _sendGridOptions.PasswordResetInfo.PlainContent;
        var htmlContent = $"<a href='http://localhost:8080/resetPassword/{user.Id}/{user.RecoveryToken}'>Zresetuj hasło.</a>";
        var email = MailHelper.CreateSingleEmail(from, to, subject, plainContent, htmlContent);
        return _sendGridClient.SendEmailAsync(email);
    }

    public Task <Response> SendChangeEmailTo(User user, string newEmail)
    {
        var from = new EmailAddress(_sendGridOptions.HostEmail);
        var to = new EmailAddress(newEmail);
        var subject = _sendGridOptions.EmailChangeInfo.Subject;
        var plainContent = _sendGridOptions.EmailChangeInfo.PlainContent;
        var htmlContent = $"<a href='http://localhost:8080/changeEmail?id={user.Id}&emailChangeToken={user.EmailChangeToken}&email={newEmail}'>Zmień adres e-mail.</a>";
        var email = MailHelper.CreateSingleEmail(from, to, subject, plainContent, htmlContent);
        return _sendGridClient.SendEmailAsync(email);
    }
}
