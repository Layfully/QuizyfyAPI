namespace QuizyfyAPI.Options
{
    public class SendGridOptions
    {
        public string HostEmail { get; set; }
        public MailInfo RegistrationInfo { get; set; }
        public MailInfo PasswordResetInfo { get; set; }
    }

    public class MailInfo
    {
        public string Subject { get; set; }
        public string PlainContent { get; set; }
        public string HtmlContent { get; set; }
    }
}
