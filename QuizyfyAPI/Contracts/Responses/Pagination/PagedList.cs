namespace QuizyfyAPI.Contracts.Responses.Pagination;

internal sealed class PagedList<T>
{
    public List<T> List { get; }
    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int NextPageNumber => HasNextPage ? PageNumber + 1 : TotalPages;
    public int PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : 1;

    private PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalItems = count;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        List = items;
    }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    public PagingHeader GetHeader() => new(TotalItems, PageNumber, PageSize, TotalPages);
}