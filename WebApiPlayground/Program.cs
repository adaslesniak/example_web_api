var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", Lala);
app.MapGet("/baja", Baja);
app.Run();




string Lala() => "Gudbaj word";

string Baja() => "Fiu fiu";
