using QuizyfyAPI.Data.Entities.Interfaces;

namespace QuizyfyAPI.Data;
/// <summary>
/// A User model, which is a representation of row from Users table.
/// </summary>
public class User : ICreatedDate
{
    /// <summary>
    /// Id of a user.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// First name of a user.
    /// </summary>
    public string? FirstName { get; set; }
    /// <summary>
    /// Last name of a user.
    /// </summary>
    public string? LastName { get; set; }
    /// <summary>
    /// Made up username.
    /// </summary>
    public string Username { get; set; } = null!;
    /// <summary>
    /// Avatar image of a user.
    /// </summary>
    public Image? Avatar { get; set; }
    /// <summary>
    /// Description of a user.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Password in a hashed form.
    /// </summary>
    public byte[] PasswordHash { get; set; } = null!;
    /// <summary>
    /// A salt, which is used to make every hashed password unique.
    /// </summary>
    public byte[] PasswordSalt { get; set; } = null!;
    /// <summary>
    /// Role of a user, which specifies things he has access to.
    /// </summary>
    public string Role { get; set; } = null!;
    /// <summary>
    /// Email of a user.
    /// </summary>
    public string Email { get; set; } = null!;
    /// <summary>
    /// Property, which specifies whether an email has been confirmed by a user.
    /// </summary>
    public bool EmailConfirmed { get; set; }
    /// <summary>
    /// JWT token used for auth.
    /// </summary>
    public string? JwtToken { get; set; }
    /// <summary>
    /// Token used for refreshing a JWT token.
    /// </summary>
    public RefreshToken? RefreshToken { get; set; }
    /// <summary>
    /// Token used for confirming an email.
    /// </summary>
    public string VerificationToken { get; set; } = null!;
    /// <summary>
    /// Token used for password recovery.
    /// </summary>
    public string? RecoveryToken { get; set; }
    /// <summary>
    /// Token used to change email.
    /// </summary>
    public string? EmailChangeToken { get; set; }
    /// <summary>
    /// Date which represents when user was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
