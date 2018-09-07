using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansChess.Common;
using OrleansChess.GrainClasses.Chess;

namespace OrleansSiloHost
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddMemoryGrainStorage(GrainPersistence.SeatWhiteStateStore)
                .AddMemoryGrainStorage(GrainPersistence.SeatBlackStateStore)
                .AddMemoryGrainStorage(GrainPersistence.BoardStateStore)
                .AddMemoryGrainStorage(GrainPersistence.GameStateStore)
                .AddSimpleMessageStreamProvider(GrainPersistence.SeatWhiteStateStore)
                .AddSimpleMessageStreamProvider(GrainPersistence.SeatBlackStateStore)
                .AddSimpleMessageStreamProvider(GrainPersistence.BoardStateStore)
                .AddSimpleMessageStreamProvider(GrainPersistence.GameStateStore)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansChess";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Game).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}