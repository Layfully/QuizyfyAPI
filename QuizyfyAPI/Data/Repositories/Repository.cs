namespace QuizyfyAPI.Data;
public abstract class Repository : IRepository
{
    protected readonly QuizDbContext _context;
    protected readonly ILogger<Repository> _logger;

    protected Repository(QuizDbContext context, ILogger<Repository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Delete<T>(T entity) where T : class
    {
        _logger.LogInformation($"Removing an object of type {entity.GetType()} to the context.");
        _context.Remove(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        _context.Update(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        _logger.LogInformation($"Attempitng to save the changes in the context");

        // Only return success if at least one row was changed
        return (await _context.SaveChangesAsync()) > 0;
    }

    public void Add<T>(T entity) where T : class
    {
        _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");
        _context.Add(entity);
    }
}
