using System.ComponentModel.DataAnnotations;

namespace BookStore.Model
{
    public class CategoryMapper
    {
        public CategoryMapper() { }
        public CategoryMapper(string name) {
            this.Name = name;
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
    }
}
