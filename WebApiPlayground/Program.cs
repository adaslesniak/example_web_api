using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Mysqlx.Prepare;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;


var app = PrepareApp();
MapEndpoints();
app.Run();


WebApplication PrepareApp() {
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDbContext<ProductDb>(opt => opt.UseInMemoryDatabase("products"));
    //builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var app = builder.Build();
    if(app.Environment.IsDevelopment()) {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    return app;
}

void MapEndpoints() {
    var root = app.MapGroup("/products");
    root.MapGet("", GetProducts);
    root.MapPost("", PostProduct);
    root.MapPut("", PutProduct);
    root.MapGet("/csv", GetProductsCsv);
    root.MapPost("/csv", PostProductsCsv).Accepts<IFormFile>("text/plain");
}

async Task<string> GetProductsCsv(ProductDb db) =>
    ProductsCsv.Serialize(await db.Products.ToListAsync());

async Task<IResult> GetProducts (ProductDb db) =>
    TypedResults.Ok(await db.Products.ToListAsync());

async Task<IResult> PostProductsCsv(HttpRequest request, ProductDb db) {
    using(var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8)) {
        var csvData = await reader.ReadToEndAsync();
        if(ProductsCsv.Parse(csvData, out var updatedProducts) is false) {
            return TypedResults.BadRequest(); //could return explanaition what went wrong
        }
        await db.AddRangeAsync(updatedProducts.ToList());
        await db.SaveChangesAsync();
        return TypedResults.Ok(); //could return some errors, ok may mean not every row was parsed correctly
    }
}

async Task<IResult> PostProduct(Product updatedItem, ProductDb db) =>
    await AddAndSave(
        newItem: updatedItem,
        db: db,
        toReturnOnSucces: Results.Created($"/products/{updatedItem.Name}:{updatedItem.WeightInKg}kg", updatedItem));

async Task<IResult> PutProduct(Product updatedItem, ProductDb db) {
    await db.Products.AddAsync(updatedItem);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{updatedItem.Name}", updatedItem);
}

async Task<IResult> AddAndSave(Product newItem, ProductDb db, IResult toReturnOnSucces) {
    await db.Products.AddAsync(newItem);
    await db.SaveChangesAsync();
    return toReturnOnSucces;
}



