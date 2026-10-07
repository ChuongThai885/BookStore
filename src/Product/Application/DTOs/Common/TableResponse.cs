namespace Product.Application.DTOs.Common
{
    public class TableResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int Total { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public Boolean HasNextPage { get; set; }
        public Boolean HasPreviousPage { get; set; }
    }
}
