namespace BookStore.Model
{
    public class UpdateBook
    {
        public string BookName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public int CategoryId { get; set; }
    }
}
