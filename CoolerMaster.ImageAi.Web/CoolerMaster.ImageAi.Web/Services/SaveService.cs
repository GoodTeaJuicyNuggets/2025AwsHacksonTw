using System.Collections.Generic;
using System.Threading.Tasks;
using CoolerMaster.ImageAi.Shared;
using CoolerMaster.ImageAi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CoolerMaster.ImageAi.Web.Services
{
    public class SaveService(ProductDbContext dbContext) : ISaveService
    {
        public async Task SaveImage(ImageSource imageSource, string imageUrl, string imagePrompt = "")
        {
            var fakeProduct = await dbContext.Products.FirstOrDefaultAsync(x => x.Name == "no_product");
            if (fakeProduct == null)
            {
                fakeProduct = new Product()
                {
                    Name = "no_product",
                    ProductCategory = ""
                };
                dbContext.Products.Add(fakeProduct);
                await dbContext.SaveChangesAsync();
            }
            
            var entity = new ProductImage()
            {
                ImageSource = imageSource,
                ImageUrl = imageUrl,
                ProductCategory = "pc-cases",
                ProductId = fakeProduct.Id,
                Prompts = new List<ImagePrompt>()
                {
                    new ImagePrompt()
                    {
                        Prompt = imagePrompt
                    }
                }
            };

            await dbContext.ProductImages.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}
