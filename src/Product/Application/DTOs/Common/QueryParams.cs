namespace Product.Application.DTOs.Common
{
    public class QueryParams
    {
        private const int MaxPageSize = 100;
        private int _pageNumber = 1;
        private int _pageSize = 20;
        public int PageNumber { get => _pageNumber; set => _pageNumber = Math.Max(1, value); }
        public int PageSize { get => _pageSize; set => _pageSize = Math.Clamp(value, 1, MaxPageSize); }
        public string Search { get; set; } = String.Empty;
        public string OrderBy { get; set; } = String.Empty;
    }
}
