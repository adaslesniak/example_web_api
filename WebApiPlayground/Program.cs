using Microsoft.EntityFrameworkCore;
using WebApiPlayground;



var app = PrepareApp();
MapEndpoints();
app.Run();


WebApplication PrepareApp() {
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDbContext<ProductDb>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    //builder.Services.AddDbContext<ProductDb>(opt => opt.UseInMemoryDatabase("products"));
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
    root.MapGet("", Api.GetProducts);
    root.MapPost("", Api.PostProduct);
    root.MapPut("", Api.PutProduct);
    root.MapGet("/csv", Api.GetProductsCsv);
    root.MapPut("/csv", Api.PutProductsCsv).Accepts<IFormFile>("text/plain");
}



