using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared.Models
{
    public class ImageSpec
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public string SpecKey { get; set; }
        public string SpecValue { get; set; }

        public ProductImage ProductImage { get; set; }
    }
}
