using QuizyfyAPI.Data.Repositories.Interfaces;

namespace QuizyfyAPI.Data.Repositories;

internal abstract partial class Repository(QuizDbContext context, ILogger<Repository> logger) : IRepository
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Adding an object of type {Type} to the context.")]
    private static partial void LogAddingEntity(ILogger logger, string type);

    [LoggerMessage(Level = LogLevel.Information, Message = "Removing an object of type {Type} from the context.")]
    private static partial void LogRemovingEntity(ILogger logger, string type);

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempting to save the changes in the context")]
    private static partial void LogSavingChanges(ILogger logger);
    
    protected readonly QuizDbContext _context = context;
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly ILogger<Repository> _logger = logger;

    public void Add<T>(T entity) where T : class
    {
        LogAddingEntity(_logger, entity.GetType().Name);
        _context.Add(entity);
    }
    
    public void Delete<T>(T entity) where T : class
    {
        LogRemovingEntity(_logger, entity.GetType().Name);
        _context.Remove(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        // EF Core tracks changes automatically if the entity was loaded from DB, 
        // but this forces the state to Modify.
        _context.Update(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        LogSavingChanges(_logger);
        
        // Only return success if at least one row was changed
        return await _context.SaveChangesAsync() > 0;
    }
}
