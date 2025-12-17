using System.Text.Json.Serialization;
using QuizyfyAPI.Contracts.Responses.Pagination;

namespace QuizyfyAPI.Data;

[JsonSerializable(typeof(UserLoginRequest))]
[JsonSerializable(typeof(UserRegisterRequest))]
[JsonSerializable(typeof(UserRefreshRequest))]
[JsonSerializable(typeof(UserUpdateRequest))]
[JsonSerializable(typeof(VerifyEmailRequest))]
[JsonSerializable(typeof(RecoverPasswordRequest))]
[JsonSerializable(typeof(RecoveryTokenGenerationRequest))]
[JsonSerializable(typeof(QuizCreateRequest))]
[JsonSerializable(typeof(QuizUpdateRequest))]
[JsonSerializable(typeof(QuestionCreateRequest))]
[JsonSerializable(typeof(QuestionUpdateRequest))]
[JsonSerializable(typeof(ChoiceCreateRequest))]
[JsonSerializable(typeof(ChoiceUpdateRequest))]
[JsonSerializable(typeof(PagingHeader))]

[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(QuizResponse))]
[JsonSerializable(typeof(QuizListResponse))]
[JsonSerializable(typeof(QuestionResponse))]
[JsonSerializable(typeof(ChoiceResponse))]
[JsonSerializable(typeof(ImageResponse))]
[JsonSerializable(typeof(LikeResponse))]

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(string))]

internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}