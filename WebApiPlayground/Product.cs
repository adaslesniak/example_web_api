using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace WebApiPlayground;


class Product {
    [Key]
    public string Name { get; set; }
    public string? Description { get; set; }
    public float WeightInKg { get; set; }
    public int? WidthInCm { get; set; }
    public int? HeightInCm { get; set; }
    public int? DepthInCm { get; set; }
    public int PricePer100 { get; set; } //per 100 to not keep moeny in floats

    public Product() { Name = string.Empty; } //required for ef engine and csv


    internal void Update(Product updated) {
        Description = updated.Description;
        PricePer100 = updated.PricePer100;
        WeightInKg = updated.WeightInKg;
        WidthInCm = updated.WidthInCm;
        HeightInCm = updated.HeightInCm;
        DepthInCm = updated.DepthInCm;
        PricePer100 = updated.PricePer100;
    }
}


//second class in one file, but those are small and don't make the file complex
//while information that Product is DbEntry is crucial for working with it - eg. can't mess with constructors
class ProductDb : DbContext {
    public ProductDb(DbContextOptions<ProductDb> options) 
        : base(options) { }

    public DbSet<Product> Products =>
        Set<Product>();
}
