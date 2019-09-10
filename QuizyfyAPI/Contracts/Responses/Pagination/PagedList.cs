using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizyfyAPI.Contracts.Responses.Pagination
{
    public class PagedList<T>
    {
        public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            TotalItems = source.Count();
            PageNumber = pageNumber;
            PageSize = pageSize;
            List = source.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
        }

        public int TotalItems { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public List<T> List { get; }
        public int TotalPages =>
              (int)Math.Ceiling(this.TotalItems / (double)this.PageSize);
        public bool HasPreviousPage => this.PageNumber > 1;
        public bool HasNextPage => this.PageNumber < this.TotalPages;
        public int NextPageNumber =>
               this.HasNextPage ? this.PageNumber + 1 : this.TotalPages;
        public int PreviousPageNumber =>
               this.HasPreviousPage ? this.PageNumber - 1 : 1;

        public PagingHeader GetHeader()
        {
            return new PagingHeader(
                 this.TotalItems, this.PageNumber,
                 this.PageSize, this.TotalPages);
        }
    }
}
