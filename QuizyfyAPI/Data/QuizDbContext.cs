using Microsoft.EntityFrameworkCore;
using QuizyfyAPI.Data.Entities.Interfaces;

namespace QuizyfyAPI.Data;
public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override int SaveChanges()
    {
        SetProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetProperties();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetProperties()
    {
        foreach (var entity in ChangeTracker.Entries().Where(p => p.State == EntityState.Added))
        {
            var created = entity.Entity as ICreatedDate;
            if (created != null)
            {
                created.CreatedDate = DateTime.Now;
            }
        }
    }
}
