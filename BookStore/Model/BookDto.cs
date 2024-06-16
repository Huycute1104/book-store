namespace BookStore.Model
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public double Discount { get; set; }
        public CategoryDto Category { get; set; }
    }
}
