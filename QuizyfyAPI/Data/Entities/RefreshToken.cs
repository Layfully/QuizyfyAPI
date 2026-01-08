#pragma warning disable CA1852
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizyfyAPI.Data.Entities;

public  class RefreshToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Token { get; set; } = null!;
    [Required]
    [MaxLength(500)]
    public required string JwtId { get; set; }

    public DateTime CreationDate { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    
    public bool Used { get; set; }
    
    public bool Invalidated { get; set; }

    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}