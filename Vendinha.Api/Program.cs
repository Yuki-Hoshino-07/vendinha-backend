using Vendinha.Api.Controllers;
using Vendinha.Api.Dados;
using Vendinha.Api.Negocio;

namespace Vendinha.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers(options => options.Filters.Add<ErroNegocioFilter>());
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<BancoDados>();
            builder.Services.AddScoped<ClienteDados>();
            builder.Services.AddScoped<DividaDados>();
            builder.Services.AddScoped<ClienteNegocio>();
            builder.Services.AddScoped<DividaNegocio>();

            var app = builder.Build();
            var banco = app.Services.GetRequiredService<BancoDados>();
            banco.CriarTabelas();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();
            app.Run();
        }
    }
}
