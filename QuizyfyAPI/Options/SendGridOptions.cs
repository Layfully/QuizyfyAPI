namespace QuizyfyAPI.Options;
public record SendGridOptions
{
    public required string HostEmail { get; init; }
    public required MailInfo RegistrationInfo { get; init; }
    public required MailInfo PasswordResetInfo { get; init; }
}

public record MailInfo
{
    public required string Subject { get; init; }
    public required string PlainContent { get; init; }
    public required string HtmlContent { get; init; }
}
