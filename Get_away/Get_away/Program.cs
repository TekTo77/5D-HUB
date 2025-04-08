using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������������ ��� Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ���������� Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Middleware ��� ����������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ������������� Ocelot ��� middleware
app.UseOcelot().Wait();

// ��������� �����
app.Urls.Add("http://localhost:5054");

// ������ ����������
app.Run();