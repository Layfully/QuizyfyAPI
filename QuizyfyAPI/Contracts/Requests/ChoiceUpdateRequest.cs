namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// A choice with text(actual answer) and isRight bool. Used for displaying questions and DTO.
    /// </summary>
    public class ChoiceUpdateRequest
    {
        /// <summary>
        /// Choice text (answer).
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Bool which defines whether this answer is right or not.
        /// </summary>
        public bool IsRight { get; set; }
    }
}
