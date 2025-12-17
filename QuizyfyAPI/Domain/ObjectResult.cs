using System.Diagnostics.CodeAnalysis;

namespace QuizyfyAPI.Domain;

internal sealed record ObjectResult<T> : DetailedResult
{
    public T? Object { get; init; }
    
    /// <summary>
    /// Shadows base.Success to apply the Null-State attribute.
    /// Indicates that if Success is true, Object is not null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Object))]
    public new bool Success
    {
        get => base.Success;
        init => base.Success = value;
    }
}