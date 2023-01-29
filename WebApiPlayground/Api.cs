using Microsoft.EntityFrameworkCore;

public static class Api
{
    internal static async Task<string> GetProductsCsv(ProductDb db) =>
        ProductsCsv.Serialize(await db.Products.ToListAsync());

    internal static async Task<IResult> GetProducts(ProductDb db) =>
        TypedResults.Ok(await db.Products.ToListAsync());

    internal static async Task<IResult> PutProductsCsv(HttpRequest request, ProductDb db) {
        using(var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8)) {
            var csvData = await reader.ReadToEndAsync();
            if(ProductsCsv.Parse(csvData, out var updatedProducts, out var errors) is false) {
                return TypedResults.BadRequest(errors);
            }
            await db.AddRangeAsync(updatedProducts.ToList());
            await db.SaveChangesAsync();
            return TypedResults.Ok(errors); //is partial success an success? it creates only confusion
        }
    }

    internal static async Task<IResult> PostProduct(Product newItem, ProductDb db) {
        if(db.Products.Any(existing => existing.Name == newItem.Name)) {
            return Results.Conflict($"Can't add {newItem.Name}, such item already exists");
        }
        return await AddAndSave(
            newItem: newItem,
            db: db,
            toReturnOnSucces: Results.Created($"/products/{newItem.Name}", newItem));
    }

    internal static async Task<IResult> PutProduct(Product newOrUpdated, ProductDb db) =>
       await AddAndSave(
            newItem: newOrUpdated,
            db: db,
            toReturnOnSucces: Results.Created($"/products/{newOrUpdated.Name}", newOrUpdated));


    static async Task<IResult> AddAndSave(Product newItem, ProductDb db, IResult toReturnOnSucces) {
        await db.Products.AddAsync(newItem);
        await db.SaveChangesAsync();
        return toReturnOnSucces;
    }
}
