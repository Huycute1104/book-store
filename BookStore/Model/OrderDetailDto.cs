namespace BookStore.Model
{
    public class OrderDetailDto
    {
        public int BookId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
    }
}
