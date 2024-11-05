using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using OrleansTest.IGrains;

namespace OrleansTest.Silos
{
    public class SilosTest
    {
        static async Task Main(string[] args)
        {
            Console.Title = typeof(SilosTest).Namespace;

            /*await Host.CreateDefaultBuilder()
                .UseOrleans((builder) =>
                {
                    builder.UseLocalhostClustering()
                    .AddMemoryGrainStorageAsDefault()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "HelloGrain";
                        options.ServiceId = "HelloGrain";
                    })
                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = System.Net.IPAddress.Loopback);
                })
                .ConfigureServices(service =>
                {
                    service.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })
                .RunConsoleAsync();*/

            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                .UseOrleans(silo =>
                {
                    silo.UseLocalhostClustering()
                        .ConfigureLogging(logging => logging.AddConsole());
                })
                .UseConsoleLifetime();

            using IHost host = builder.Build();

            await host.RunAsync();
        }
    }
}