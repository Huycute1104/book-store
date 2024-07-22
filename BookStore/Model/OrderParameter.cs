namespace BookStore.Model
{
    public class OrderParameter
    {

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}

