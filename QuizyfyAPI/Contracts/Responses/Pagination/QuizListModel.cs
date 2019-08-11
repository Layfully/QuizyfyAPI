using System.Collections.Generic;

namespace QuizyfyAPI.Contracts.Responses.Pagination
{
    public class QuizListResponse
    {
        public PagingHeader Paging { get; set; }
        public List<LinkInfo> Links { get; set; }
        public List<QuizResponse> Items { get; set; }
    }
}
