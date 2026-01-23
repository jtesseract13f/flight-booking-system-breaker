namespace GatewayService.DTO;

public class PaginationList<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalElements { get; init; }
    
    public IEnumerable<T>? Items { get; init; }
}

/*
 * {
         "page": 1,
         "pageSize": 1,
         "totalElements": 1,
         "items": [
 */