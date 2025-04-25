using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared.Models
{
    public class ProductImage
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public int ProductId { get; set; }
        public string ProductCategory { get; set; }
        public ImageSource ImageSource { get; set; }

        public Product Product { get; set; }
        public List<ImageSpec> Specs { get; set; }
        public List<ImagePrompt> Prompts { get; set; }
    }
}
