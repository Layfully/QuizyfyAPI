using Newtonsoft.Json;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// Error with status code and message.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Int which tells us error status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
