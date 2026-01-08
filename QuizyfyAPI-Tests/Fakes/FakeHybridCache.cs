using Microsoft.Extensions.Caching.Hybrid;
using System.Collections.Concurrent;

namespace QuizyfyAPI_Tests.Fakes;

public class FakeHybridCache : HybridCache
{
    public ConcurrentDictionary<string, object> Store { get; } = new();
    public IList<string> RemovedTags { get; } = [];
    
    public override async ValueTask<T> GetOrCreateAsync<TState, T>(
        string key, 
        TState state, 
        Func<TState, CancellationToken, ValueTask<T>> factory, 
        HybridCacheEntryOptions? options = null, 
        IEnumerable<string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        
        if (Store.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        T result = await factory(state, cancellationToken);
        
        if (result is not null) 
        {
            Store[key] = result;
        }
        
        return result;
    }

    public override ValueTask SetAsync<T>(
        string key, 
        T value, 
        HybridCacheEntryOptions? options = null, 
        IEnumerable<string>? tags = null, 
        CancellationToken cancellationToken = default)
    {
        Store[key] = value!;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Store.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        RemovedTags.Add(tag);
        return ValueTask.CompletedTask;
    }
}