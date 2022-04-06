using Microsoft.EntityFrameworkCore;

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
}
