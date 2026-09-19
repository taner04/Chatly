namespace Chatly.Contracts.Common.Pagination;

public abstract record PaginationQuery(int PageIndex, int PageSize);