using Microsoft.EntityFrameworkCore;
using Vendinha.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<VendinhaContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Vendinha")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var banco = scope.ServiceProvider.GetRequiredService<VendinhaContext>();
    banco.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
