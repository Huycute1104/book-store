namespace BookStore.Model
{
    public class OrderResponse
    {
        public DateTime OrderDate { get; set; }
        public decimal? Total { get; set; }
        public int? UserId { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public BookDto2 Book { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public string PaymentUrl { get; set; }
    }
}
