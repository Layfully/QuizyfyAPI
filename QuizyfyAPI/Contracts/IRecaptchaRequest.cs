namespace QuizyfyAPI.Contracts;

internal interface IRecaptchaRequest
{
    string RecaptchaToken { get; }
}