using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Options;
public class SendGridOptions
{
    [Required]
    public string HostEmail { get; set; } = null!;
    [Required]
    public MailInfo RegistrationInfo { get; set; } = null!;
    [Required]
    public MailInfo PasswordResetInfo { get; set; } = null!;
    [Required]
    public MailInfo EmailChangeInfo { get; set; } = null!;
}

public class MailInfo
{
    [Required]
    public string Subject { get; set; } = null!;
    [Required]
    public string PlainContent { get; set; } = null!;
    [Required]
    public string HtmlContent { get; set; } = null!;
}
