using Microsoft.EntityFrameworkCore;

public static class Api
{
    internal static async Task<string> GetProductsCsv(ProductDb db) =>
        ProductsCsv.Serialize(await db.Products.ToListAsync());

    internal static async Task<IResult> GetProducts(ProductDb db) =>
        TypedResults.Ok(await db.Products.ToListAsync());

    internal static async Task<IResult> PostProductsCsv(HttpRequest request, ProductDb db) {
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

    internal static async Task<IResult> PostProduct(Product newItem, ProductDb db) =>
        await AddAndSave(
            newItem: newItem,
            db: db,
            toReturnOnSucces: Results.Created($"/products/{newItem.Name}", newItem));

    internal static async Task<IResult> PutProduct(Product updatedItem, ProductDb db) {
        await db.Products.AddAsync(updatedItem);
        await db.SaveChangesAsync();
        return Results.Created($"/products/{updatedItem.Name}", updatedItem);
    }

    static async Task<IResult> AddAndSave(Product newItem, ProductDb db, IResult toReturnOnSucces) {
        await db.Products.AddAsync(newItem);
        await db.SaveChangesAsync();
        return toReturnOnSucces;
    }
}
