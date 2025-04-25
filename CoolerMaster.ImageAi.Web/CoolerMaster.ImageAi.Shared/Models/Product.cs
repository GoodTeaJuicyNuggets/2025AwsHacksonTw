using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductCategory { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductPageUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<ProductImage> ProductImages { get; set; }
    }
}
