using Microsoft.OpenApi.Models;
using SeccuretyRepos;
using Servise.Tools;
using System.Text.Json;
using Users.Servise;
using UserServise;

namespace Startups
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // Регистрирует контроллеры

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            }); 


            services.AddScoped<JWTservise, JWTservise>();
            services.AddScoped<UserService, UserService>();
            services.AddScoped<PasswordService, PasswordService>();





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

            app.UseRouting(); 

            app.UseCors("AllowALL"); 

            app.UseSwagger(); 
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; 
            });
           
            app.MapWhen(context => !context.Request.Path.StartsWithSegments("/api/Users/register") &&
                                  !context.Request.Path.StartsWithSegments("/api/Users/login"), appBuilder =>
                                  {
                                      appBuilder.UseJwtAuthentication(); 
                                      appBuilder.UseRouting();
                                      appBuilder.UseEndpoints(endpoints => endpoints.MapControllers());
                                  });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); 
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

            string email = await CheckEmail.GetEmailByIdAsync(userId);

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
