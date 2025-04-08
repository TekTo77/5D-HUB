using Microsoft.OpenApi.Models;
using SeccuretyRepos;
using Servise.Tools;
using System.Text.Json;
using Users.Servise;
using UserServise;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Users;

var builder = WebApplication.CreateBuilder(args);

// ��������� Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console());

// ����������� �������������
Log.Information("������������� ����������...");

// ���������� ��������
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});


builder.Services.AddScoped<JWTservise, JWTservise>();
builder.Services.AddScoped<UserService, UserService>();
builder.Services.AddScoped<PasswordService, PasswordService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowALL", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});


// ���������� ����������
var app = builder.Build();



// Middleware ��� ����������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
   

    app.UseSwagger();


    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
  
}

// �������� middleware
app.UseRouting();


app.UseCors("AllowALL");


// ����� ��� ���������, ������� �� ������ ������������ JwtAuthentication
app.MapWhen(context =>
{
    var path = context.Request.Path.Value?.ToLower();
    var isExcludedPath = path != null &&
                         (path.StartsWith("/api/users/register") || path.StartsWith("/api/users/login"));
    Log.Information($"�������� ����: {path}, isExcludedPath={isExcludedPath}");
    return !isExcludedPath;
},
appBuilder =>
{
    appBuilder.UseJwtAuthentication();
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(endpoints => endpoints.MapControllers());
});

// ����� ��� ��������� ��������� (��� JwtAuthentication)
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
});



// ��������� �����
app.Urls.Add("http://localhost:5052");

// ������ ����������
app.Run();