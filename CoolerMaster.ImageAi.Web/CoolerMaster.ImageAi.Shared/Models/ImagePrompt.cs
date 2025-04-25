using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared.Models
{
    public class ImagePrompt
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public string Prompt { get; set; }
        public DateTime CreateTime { get; set; }

        public ProductImage ProductImage { get; set; }
    }
}
