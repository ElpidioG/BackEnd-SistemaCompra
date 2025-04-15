using BackEnd_SistemaCompra.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // <-- Asegúrate de tener este using

namespace BackEnd_SistemaCompra
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient();

            builder.Services.AddDbContext<ConexionDB>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

            builder.Services.AddControllers();

            // ✅ Agrega esta configuración explícita
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Sistema de Compras",
                    Version = "v1"
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}