using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizyfyAPI.Data;
/// <summary>
/// A Refresh token model, which is a representation of row from RefreshTokens table.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Token, which due to uniqueness of guid is also a primary key.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Token { get; set; } = null!;
    /// <summary>
    /// ID of JWT token to which refresh token belongs to.
    /// </summary>
    public string JwtId { get; set; } = null!;
    /// <summary>
    /// Creation date of a refresh token.
    /// </summary>
    public DateTime CreationDate { get; set; }
    /// <summary>
    /// Expiry date of a refresh token.
    /// </summary>
    public DateTime ExpiryDate { get; set; }
    /// <summary>
    /// Flag, which informs whether refresh token has been used.
    /// </summary>
    public bool Used { get; set; }
    /// <summary>
    /// Flag, which informs whether refresh token has been invalidated.
    /// </summary>
    public bool Invalidated { get; set; }
    /// <summary>
    /// ID of a User to which refresh token belongs to.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// User to which refresh token belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}
