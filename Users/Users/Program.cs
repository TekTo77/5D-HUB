using Microsoft.AspNetCore.Hosting;
using Product_Service;
using Serilog;

namespace Startups
{
    public class Program
    {
        static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            await CreateHostBuilder(args).Build().RunAsync();


        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls("http://localhost:5052/");
                });
    }
}