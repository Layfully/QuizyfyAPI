using System.Collections.Generic;

namespace QuizyfyAPI.Domain;
public class BasicResult
{
    public bool Success { get; set; }
    public IEnumerable<string> Errors { get; set; }
}
