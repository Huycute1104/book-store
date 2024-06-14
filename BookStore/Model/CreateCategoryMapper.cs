using System.ComponentModel.DataAnnotations;

namespace BookStore.Model
{
    public class CreateCategoryMapper
    {
        public CreateCategoryMapper() { }
        public CreateCategoryMapper(string name)
        {
            this.Name = name;
        }

        [Required]
        public string Name { get; set; } = null!;
    }
}
