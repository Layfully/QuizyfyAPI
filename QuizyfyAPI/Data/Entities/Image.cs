using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Data
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        public string ImageUrl { get; set; }
    }
}
