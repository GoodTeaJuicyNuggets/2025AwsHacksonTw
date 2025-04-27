using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared
{
    public class Utils
    {
        public static string GetContentTypeFromExtension(string input)
        {
            switch (Path.GetExtension(input).ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
