using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;

namespace OrleansChess.Web.Orleans
{
    public static class OrleansServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansClient(this IServiceCollection services)
        {
            var addOrleansClientAsync = AddOrleansClientAsync(services);
            addOrleansClientAsync.Wait();
            var servicesWithOrleans = addOrleansClientAsync.Result;
            return servicesWithOrleans;
        }

        public delegate void OnFailure(int attempt, int attemptsBeforeFailing);

        private static async Task<IServiceCollection> AddOrleansClientAsync(this IServiceCollection services)
        {
            var client = await StartClientWithRetries();
            services.AddSingleton<IClusterClient>(client);
            return services;
        }

        private static async Task<T> ExecuteFuncWithRetries<T>(int attemptsBeforeFailing, Func<T> func, OnFailure onFailure)
        {
            int attempt = 0;
            T result;
            while (true)
            {
                try
                {
                    result = func();
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    onFailure(attempt, attemptsBeforeFailing);
                    if (attempt > attemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }
            return result;
        }

        private static async Task<IClusterClient> StartClientWithRetries(int attemptsBeforeFailing = 5)
        {
            Func<IClusterClient> buildClient = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .UseSignalR()
                .Build;
            OnFailure onFailure = (attempt, totalAttempts) => Console.WriteLine($"Attempt {attempt} of {attemptsBeforeFailing} failed to initialize the Orleans client.");
            var client = await ExecuteFuncWithRetries(attemptsBeforeFailing, buildClient, onFailure);
            await client.Connect();
            Console.WriteLine("Client successfully connect to silo host");
            return client;
        }
    }
}