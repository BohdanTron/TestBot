// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            // Create the storage with User and Conversation state  
            var cosmosDbStorageOptions = new CosmosDbPartitionedStorageOptions
            {
                CosmosDbEndpoint = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("URL"),
                AuthKey = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("PrimaryKey"),
                DatabaseId = Configuration.GetSection("AzureCosmosDbSettings").GetValue<string>("DatabaseName"),
                ContainerId = "ConversationsHistory"
            };
            var storage = new CosmosDbPartitionedStorage(cosmosDbStorageOptions);

            var userState = new UserState(storage);
            services.AddSingleton(userState);

            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.EchoBot>();
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
    }
}
