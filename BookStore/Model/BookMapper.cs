using Repository.Models;

namespace BookStore.Model
{
    public class BookMapper
    {
        public BookMapper() { }

        public BookMapper(int id, string name, string description, decimal unitPrice, int unitsInStock, double discount, Category category)
        {
            Id = id;
            Name = name;
            Description = description;
            UnitPrice = unitPrice;
            UnitsInStock = unitsInStock;
            Discount = discount;
            this.Category = category;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public double Discount { get; set; }

        public Category Category {  get; set; } 
        
    }
}
