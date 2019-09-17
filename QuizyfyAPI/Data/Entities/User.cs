using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string JwtToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
