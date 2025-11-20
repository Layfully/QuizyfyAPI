using Newtonsoft.Json;

namespace QuizyfyAPI.Contracts.Responses;
/// <summary>
/// Error with status code and message.
/// </summary>
/// <param name="StatusCode">Int which tells us error status code.</param>
/// <param name="Message">Error message.</param>
public record ErrorResponse(int StatusCode, string Message);
