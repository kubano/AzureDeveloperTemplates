﻿using Azure.Cosmos;
using Azure.Cosmos.Serialization;
using Azure.Storage.Blobs;
using AzureDeveloperTemplates.Starter.Infrastructure.Configuration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AzureDeveloperTemplates.Starter.API.Core.DependencyInjection
{
    public static class AzureClientServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureServices(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var storageConfiguration = serviceProvider.GetRequiredService<IStorageServiceConfiguration>();
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfiguration.ConnectionString);
            services.TryAddSingleton(blobServiceClient);

            var cosmoDbConfiguration = serviceProvider.GetRequiredService<ICosmosDbDataServiceConfiguration>();
            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            CosmosClient cosmosClient = new CosmosClient(cosmoDbConfiguration.ConnectionString, cosmosClientOptions);
            CosmosDatabase database = cosmosClient.CreateDatabaseIfNotExistsAsync(cosmoDbConfiguration.DatabaseName)
                                                   .GetAwaiter()
                                                   .GetResult();
            CosmosContainer container = database.CreateContainerIfNotExistsAsync(
                cosmoDbConfiguration.ContainerName,
                cosmoDbConfiguration.PartitionKeyPath,
                400)
                .GetAwaiter()
                .GetResult();

            services.TryAddSingleton(cosmosClient);

            return services;
        }
    }
}
