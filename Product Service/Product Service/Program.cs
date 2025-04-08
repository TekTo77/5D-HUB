using Microsoft.OpenApi.Models;
using Product_Service.Servise;
using Product_Service.Servise.Kafka;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddSingleton<ProductServise>();
builder.Services.AddScoped<JWTservise>();
builder.Services.AddHostedService<ProductKafkaConsumer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowALL", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.UseJwtAuthentication();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run("http://localhost:5051/");
