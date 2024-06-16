using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public partial class Image
    {
        public int ImageId { get; set; }
        public string Url { get; set; } = null!;
        public int? BookId { get; set; }

        public virtual Book? Book { get; set; }
    }
}
