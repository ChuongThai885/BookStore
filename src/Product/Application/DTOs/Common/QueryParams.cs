namespace Product.Application.DTOs.Common
{
    public class QueryParams
    {
        public int StartIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; } = "";
        public string? OrderBy { get; set; } = "";
    }
}
