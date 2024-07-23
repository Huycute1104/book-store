namespace BookStore.Model
{
    public class UpdateBookImages
    {
        public int BookId { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
