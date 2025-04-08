using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Настройка конфигурации для Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Добавление Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Middleware для разработки
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Использование Ocelot как middleware
app.UseOcelot().Wait();

// Установка порта
app.Urls.Add("http://localhost:5054");

// Запуск приложения
app.Run();