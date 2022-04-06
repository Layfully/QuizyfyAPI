namespace QuizyfyAPI.Domain;
public class ObjectResult<T> : DetailedResult
{
    public T Object { get; set; } = default(T)!;
}
