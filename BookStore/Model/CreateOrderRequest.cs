namespace BookStore.Model
{
    public class CreateOrderRequest
    {
        public DateTime OrderDate { get; set; }
        public decimal? Total { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public List<OrderDetailDto> orderDetailDtos { get; set; }
    }

    
}
