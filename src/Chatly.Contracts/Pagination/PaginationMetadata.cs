namespace Chatly.Contracts.Pagination;

public sealed record PaginationMetadata(
    int PageIndex,
    int PageSize,
    int TotalPages,
    int TotalCount,
    int? PreviousPageIndex,
    int? NextPageIndex)
{
    public bool HasPreviousPage => PreviousPageIndex.HasValue;

    public bool HasNextPage => NextPageIndex.HasValue;
}