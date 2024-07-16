namespace BookStore.Model
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
