namespace Chatly.Contracts.Pagination;

public abstract record PaginationQuery(int PageIndex, int PageSize);