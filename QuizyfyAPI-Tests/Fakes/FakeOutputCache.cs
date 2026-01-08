using Microsoft.AspNetCore.OutputCaching;

namespace QuizyfyAPI_Tests.Fakes;

public class FakeOutputCache : IOutputCacheStore
{
    public IList<string> EvictedTags { get; } = [];

    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        EvictedTags.Add(tag);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) => ValueTask.FromResult<byte[]?>(null);
    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}