using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(BogeyBuddies.API.Startup))]

namespace BogeyBuddies.API
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            // Register the CosmosClient as a Singleton
            builder.Services.AddSingleton((s) => {
                var connectionString = configuration["CosmosDBConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("CosmosDB connection string not found.");

                return new CosmosClient(connectionString);
            });

            // Add Logging
            builder.Services.AddLogging();
        }
    }
}
