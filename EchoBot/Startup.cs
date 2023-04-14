﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using Azure.Data.Tables;
using EchoBot.Bots;
using EchoBot.Dialogs;
using EchoBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EchoBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // In memory storage of conversation references for proactive messages
            //services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Register Azure Table Storage
            var tableServiceClient =
                new TableServiceClient(new Uri(Configuration["AzureTableStorageSettings:Endpoint"]),
                    new TableSharedKeyCredential(Configuration["AzureTableStorageSettings:AccountName"], Configuration["AzureTableStorageSettings:AccountKey"]));

            services.AddSingleton(tableServiceClient);

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton(_ =>
            {
                var url = Configuration.GetSection("AzureCosmosDbSettings")
                    .GetValue<string>("URL");

                var primaryKey = Configuration.GetSection("AzureCosmosDbSettings")
                    .GetValue<string>("PrimaryKey");

                var dbName = Configuration.GetSection("AzureCosmosDbSettings")
                    .GetValue<string>("DatabaseName");

                var containerName = Configuration.GetSection("AzureCosmosDbSettings")
                    .GetValue<string>("ContainerName");

                var cosmosClient = new CosmosClient(
                    url,
                    primaryKey
                );

                return new MessageCosmosService(cosmosClient, dbName, containerName);
            });

            ConfigureState(services);

            services.AddSingleton<MainDialog>();

            //services.AddTransient<IBot, Bots.EchoBot>();

            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private void ConfigureState(IServiceCollection services)
        {
            var connectionString = Configuration["StateSettings:ConnectionString"];
            var containerName = Configuration["StateSettings:ContainerName"];

            services.AddSingleton<IStorage>(new BlobsStorage(connectionString, containerName));

            //var cosmosDbStorageOptions = new CosmosDbPartitionedStorageOptions
            //{
            //    CosmosDbEndpoint = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("URL"),
            //    AuthKey = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("PrimaryKey"),
            //    DatabaseId = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("DatabaseName"),
            //    ContainerId = "ConversationsHistory"
            //};

            //services.AddSingleton<IStorage>(new CosmosDbPartitionedStorage(cosmosDbStorageOptions));

            services.AddSingleton<UserState>();

            services.AddSingleton<ConversationState>();

            services.AddSingleton<StateService>();
        }
    }
}
