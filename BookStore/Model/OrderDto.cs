namespace BookStore.Model
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal? Total { get; set; }
        public int? UserId { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public List<ViewOrderDetail> OrderDetails { get; set; }
    }
}
