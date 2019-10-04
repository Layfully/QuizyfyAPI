using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// A quiz with name and questions properties. Used for DTO.
    /// </summary>
    public class QuizUpdateRequest

    {
        /// <summary>
        /// Quiz name.
        /// </summary>
        [MaxLength(70)]
        public string Name { get; set; }

        /// <summary>
        /// Quiz description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Quiz image url which we get when we upload image.
        /// </summary>
        public int ImageId { get; set; }
    }
}
