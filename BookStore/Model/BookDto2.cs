namespace BookStore.Model
{
    public class BookDto2
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public List<ImageDto> Images { get; set; }
    }
}
