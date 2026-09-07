namespace Chatly.Contracts.Pagination;

public sealed record PaginationResult<T>(
    IReadOnlyList<T> Items,
    PaginationMetadata Pagination);