using CoolerMaster.ImageAi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ImageSpec> ImageSpecs { get; set; }
        public DbSet<ImagePrompt> ImagePrompts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "sqlite", "products.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<ProductImage>()
                .HasKey(pi => pi.ImageId);

            modelBuilder.Entity<ProductImage>()
                .Property(p => p.ImageSource)
                .HasConversion<string>(); // enum 儲存為字串

            modelBuilder.Entity<ImageSpec>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<ImageSpec>()
                .HasOne(s => s.ProductImage)
                .WithMany(pi => pi.Specs)
                .HasForeignKey(s => s.ImageId);

            modelBuilder.Entity<ImagePrompt>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<ImagePrompt>()
                .HasOne(p => p.ProductImage)
                .WithMany(pi => pi.Prompts)
                .HasForeignKey(p => p.ImageId);

            var listToJsonConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>()
            );

            modelBuilder.Entity<Product>()
                .Property(p => p.ImageUrls)
                .HasConversion(listToJsonConverter);
        }
    }

}
