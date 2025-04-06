using Microsoft.OpenApi.Models;
using Order_Service.Servise;
using Order_Service.Servise.Kafka;
using Product_Service.Servise;
using Product_Service.SQL.Repository;
using Serilog;
using Servise.Tools;
using System.Text.Json;

namespace Order_Service
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            Log.Information("🔧 Startup создан");
            Log.Information("Kafka:BootstrapServers = {BootstrapServers}", _configuration["Kafka:BootstrapServers"]);
            Log.Information("Kafka:ProductCheckRequestTopic = {RequestTopic}", _configuration["Kafka:ProductCheckRequestTopic"]);
            Log.Information("Kafka:ResponseTopic = {ResponseTopic}", _configuration["Kafka:ResponseTopic"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("🔧 ConfigureServices начат");

            services.AddControllers();
            Log.Information("✅ Контроллеры добавлены");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
            });
            Log.Information("✅ Swagger добавлен");

            services.AddCors(options =>
            {
                options.AddPolicy("AllowALL", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            Log.Information("✅ CORS добавлен");

            // Регистрация сервисов
            services.AddScoped<OrderServise>();
            Log.Information("✅ OrderServise добавлен");

            services.AddSingleton<KafkaResponseHandler>();
            Log.Information("✅ KafkaResponseHandler добавлен");

            services.AddSingleton(sp =>
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
                var requestTopic = _configuration["Kafka:ProductCheckRequestTopic"] ?? "product-check-request";
                Log.Information("Создание KafkaProducer с BootstrapServers={BootstrapServers}, RequestTopic={RequestTopic}", bootstrapServers, requestTopic);
                return new KafkaProducer(
                    bootstrapServers,
                    requestTopic,
                    sp.GetRequiredService<KafkaResponseHandler>());
            });
            Log.Information("✅ KafkaProducer добавлен");

            Log.Information("🔧 ConfigureServices завершён");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log.Information("🔧 Configure начат");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Log.Information("✅ DeveloperExceptionPage добавлен");
            }

            app.UseRouting();
            Log.Information("✅ Routing добавлен");

            app.UseCors("AllowALL");
            Log.Information("✅ CORS добавлен");

            app.UseSwagger();
            Log.Information("✅ Swagger middleware добавлен");

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
                c.RoutePrefix = string.Empty;
            });
            Log.Information("✅ Swagger UI добавлен по адресу: /");

            app.MapWhen(context =>
            {
                var path = context.Request.Path.Value?.ToLower();

                // Пропускаем путь /api/orders/orders/{id} (но не /api/orders/orders без id)
                var isTargetPath = path != null &&
                                   path.StartsWith("/api/orders/orders/") &&
                                   path.Count(c => c == '/') == 4; // точный формат /api/orders/orders/{id}

                return !isTargetPath;
            },
        appBuilder =>
        {
            appBuilder.UseJwtAuthentication();
            appBuilder.UseRouting();
            appBuilder.UseEndpoints(endpoints => endpoints.MapControllers());
        });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                Log.Information("✅ Endpoints для контроллеров добавлены");
            });

            Log.Information("🔧 Configure завершён");
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

            string token = authHeader.IsNullOrEmpty() ? "" : authHeader.Trim();

            if (token.IsNullOrEmpty())
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Войдите в аккаунт");
                return;
            }
            int userId = await ExtractUserIdFromBody(context);

            if (userId == 0)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("UserId не указан или недействителен");
                return;
            }

            string email = await EmsilRepos.GetEmailByIdAsync(userId);

            if (email.IsNullOrEmpty())
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


        private async Task<int> ExtractUserIdFromBody(HttpContext context)
        {
            try
            {

                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!body.IsNullOrEmpty())
                {
                    using var jsonDocument = JsonDocument.Parse(body);
                    var root = jsonDocument.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in root.EnumerateObject())
                        {
                            if (new[] { "userid", "UserId", "user_id" }.Contains(property.Name.ToLower()))
                            {
                                if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out int userId))
                                {
                                    return userId;
                                }
                                return 0;
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
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
