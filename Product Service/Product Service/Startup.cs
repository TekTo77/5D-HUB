using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Product_Service.Servise;
using Product_Service.Servise.Kafka;
using Product_Service.SQL.Repository;
using Serilog;
using Servise.Tools;
using System.IdentityModel.Tokens.Jwt;

namespace Product_Service
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // Регистрирует контроллеры

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            }); // Добавление Swagger с версией API

            
            // Регистрация сервисов
            services.AddSingleton<ProductServise>();  // Изменено на Singleton вместо Scoped
            services.AddScoped<JWTservise>();     // Оставляем Scoped
           

            // Оставляем Scoped


           

            // Регистрация фонового сервиса NotesKafkaConsumer
            services.AddHostedService<ProductKafkaConsumer>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowALL", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting(); // Использование маршрутизации

            app.UseCors("AllowALL"); // Разрешаем CORS

            app.UseSwagger(); // Включаем Swagger
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; // Swagger UI доступен по корневому адресу
            });
            app.UseJwtAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Добавляем маршруты для контроллеров
            });
        }
    }


    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
     

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
           
        }

        public async Task InvokeAsync(HttpContext context)
        {

            string authHeader = context.Request.Headers["Authorization"];
            string token = authHeader?.Trim();

            if (token.IsNullOrEmpty())
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Войдите в аккаунт");
                return;
            }

            string email = await JWTservise.ExtractEmailFromTokenAsync(token);

            if (email.IsNullOrEmpty())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Не удалось извлечь email из токена");
                return;
            }

            int userId = await EmsilRepos.GetIdlByEmailAsync(email);

            if (userId == 0)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Войдите в аккаунт");
                return;
            }

            bool isValid = await JWTservise.ValidateJwtTokenAsync(token, email);

            if (!isValid)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Войдите в аккаунт");

                return;
            }

            await _next(context);
        }

  
    }

    
    public static class JwtAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtAuthenticationMiddleware>();
        }
    }
}


