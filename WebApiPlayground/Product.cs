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

    private Product() { Name = string.Empty; } //required for ef engine, 

    public Product(string withName) {
        Name = withName;
    }
}


class ProductDb : DbContext {
    public ProductDb(DbContextOptions<ProductDb> options) 
        : base(options) { }

    public DbSet<Product> Products =>
        Set<Product>();
}
