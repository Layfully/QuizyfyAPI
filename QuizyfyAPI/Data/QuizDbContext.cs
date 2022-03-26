using Microsoft.EntityFrameworkCore;

namespace QuizyfyAPI.Data;
public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
