using Microsoft.EntityFrameworkCore;

// Punto de entrada de la API: configura base de datos, CORS, controladores y Swagger.
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<TiendaDatos>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
