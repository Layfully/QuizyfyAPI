namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// Image with id from database and url to access it.
    /// </summary>
    public class ImageResponse
    {
        /// <summary>
        /// Image id.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// URL to image resource on server.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
