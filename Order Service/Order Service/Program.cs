using Microsoft.OpenApi.Models;
using Order_Service.Servise;
using Order_Service.Servise.Kafka;
using Serilog;
using Order_Service;


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console());




builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowALL", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});



builder.Services.AddScoped<OrderServise>();


builder.Services.AddSingleton<KafkaResponseHandler>();

builder.Services.AddSingleton(sp =>
{
    var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
    var requestTopic = builder.Configuration["Kafka:ProductCheckRequestTopic"] ?? "product-check-request";
   
    return new KafkaProducer(
        bootstrapServers,
        requestTopic,
        sp.GetRequiredService<KafkaResponseHandler>());
});



var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
  

    app.UseSwagger();
    

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
        c.RoutePrefix = string.Empty;
    });
   
}


app.UseRouting();


app.UseCors("AllowALL");



app.MapWhen(context =>
{
    var path = context.Request.Path.Value?.ToLower();
    var isTargetPath = path != null &&
                       path.StartsWith("/api/orders/orders/") &&
                       path.Count(c => c == '/') == 4;
 
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
});




app.Run("http://localhost:5053/");