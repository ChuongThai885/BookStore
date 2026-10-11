namespace Product.Application.DTOs.Common
{
    public class TableResponse<T>
    {
        public IEnumerable<T> Data { get; }
        public int Total { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }

        public TableResponse(IEnumerable<T> data, int total = 0, int currentPage = 1, int pageSize = 10)
        {
            Data = data ?? [];
            Total = total;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            HasNextPage = CurrentPage < TotalPages;
            HasPreviousPage = CurrentPage > 1;
        }
    }
}
