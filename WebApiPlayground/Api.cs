using CsvHelper;
using Microsoft.EntityFrameworkCore;
namespace WebApiPlayground;

public static class Api
{
    //TODO should be async as well
    internal static string GetProductsCsv(ProductDb db) =>
        db.Products.GenerateCsv();

    internal static async Task<IResult> GetProducts(ProductDb db) =>
        TypedResults.Ok(await db.Products.ToListAsync());


    internal static async Task<IResult> PutProductsCsv(HttpRequest request, ProductDb db) {
        await ProductsCsv.Process(request.Body, extracted => Put(extracted, db));
        await db.SaveChangesAsync();
        return TypedResults.Ok("Database updated");
    }

    internal static async Task<IResult> PostProduct(Product newItem, ProductDb db) {
        if(db.Products.Any(existing => existing.Name == newItem.Name)) {
            return Results.Conflict($"Can't add {newItem.Name}, such item already exists");
        }
        await Put(newItem, db);
        db.SaveChanges();
        return Results.Created($"/products/{newItem.Name}", newItem);
    }

    internal static async Task<IResult> PutProduct(Product newOrUpdated, ProductDb db) {
        await Put(newOrUpdated, db);
        db.SaveChanges();
        return Results.Created($"/products/{newOrUpdated.Name}", newOrUpdated);
    }

    static async Task Put(Product newOrUpdated, ProductDb db) {
        var existing = await db.Products.FirstOrDefaultAsync(any => any.Name == newOrUpdated.Name);
        if(existing == null) {
            await db.AddAsync(newOrUpdated);
        } else {
            existing.Update(newOrUpdated);
            db.Update(existing);
        }
    }
}
