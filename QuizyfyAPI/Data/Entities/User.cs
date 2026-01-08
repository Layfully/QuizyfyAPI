#pragma warning disable CA1852
namespace QuizyfyAPI.Data.Entities;

public  class User
{
    [Key] 
    public int Id { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }
    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required]
    [MaxLength(250)] 
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Role { get; set; } = Entities.Role.User;

    [Required] 
    public required byte[] PasswordHash { get; set; }

    [Required]
    public required byte[] PasswordSalt { get; set; }

    public bool EmailConfirmed { get; set; }

    [MaxLength(500)]
    public string? JwtToken { get; set; }
    
    [MaxLength(500)]
    public string? VerificationToken { get; set; }
    
    [MaxLength(500)]
    public string? RecoveryToken { get; set; }
    public RefreshToken? RefreshToken { get; set; }
}