using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
    }
}
