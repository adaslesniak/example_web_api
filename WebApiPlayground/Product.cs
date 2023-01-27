using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
//namespace WebApiPlayground;


class Product {
    public string Name { get; set; }
    public string? Description { get; set; }
    public float WeightInKg { get; set; }
    public int? WidthInCm { get; set; }
    public int? HeightInCm { get; set; }
    public int? DepthInCm { get; set; }
    public int PricePer100 { get; set; } //per 100 to not keep moeny in floats
}

class ProductDb : DbContext {
    public ProductDb(DbContextOptions<ProductDb> options) 
        : base(options) { }

    public DbSet<Product> Products =>
        Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Product>().HasKey(item => new {
            item.Name,
            item.WeightInKg
        });
    }
}
