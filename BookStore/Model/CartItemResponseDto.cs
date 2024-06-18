namespace BookStore.Model
{
    public class CartItemResponseDto
    {
        public int CartId { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public List<ImageResponseDto> Images { get; set; }
    }
}
