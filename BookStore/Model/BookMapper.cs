using Repository.Models;

namespace BookStore.Model
{
    public class BookMapper
    {
        public BookMapper() { }

        public BookMapper( string name, string description, decimal unitPrice, int unitsInStock, double discount, int categoryId, List<IFormFile> images)
        {
            Name = name;
            Description = description;
            UnitPrice = unitPrice;
            UnitsInStock = unitsInStock;
            Discount = discount;
            CategoryId = categoryId;
            Images = images;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public double Discount { get; set; }

        public int CategoryId {  get; set; }
        public List<IFormFile> Images { get; set; }

    }
}
