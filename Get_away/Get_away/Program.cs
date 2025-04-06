using Ocelot.Middleware;
using Ocelot.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://localhost:5054").ConfigureServices(services =>
                {
                    services.AddOcelot();
                }).Configure(app =>
                {
                    app.UseOcelot().GetAwaiter().GetResult();
                });
            });
}
