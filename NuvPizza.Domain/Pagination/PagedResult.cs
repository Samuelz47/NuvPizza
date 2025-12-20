namespace NuvPizza.Domain.Pagination;

public class PagedResult<T> 
{
    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public IEnumerable<T> Items { get; }       // Itens por página
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }             // Lista total de itens
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}