namespace Chatly.Contracts.Common.Pagination;

public sealed record PaginationResult<T>(
    IReadOnlyList<T> Items,
    PaginationMetadata Pagination);