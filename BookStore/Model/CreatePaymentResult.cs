namespace BookStore.Model
{
    public class CreatePaymentResult
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public string PaymentUrl { get; set; } // This might be the property you need

    }

}
