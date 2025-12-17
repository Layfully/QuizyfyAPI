namespace QuizyfyAPI.Options;

internal sealed record SendGridOptions
{
    public string HostEmail { get; init; } = string.Empty;
    public MailInfo RegistrationInfo { get; init; } = new();
    public MailInfo PasswordResetInfo { get; init; } = new();
}

internal sealed record MailInfo
{
    public string Subject { get; init; } = string.Empty;
    public string PlainContent { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;
}
