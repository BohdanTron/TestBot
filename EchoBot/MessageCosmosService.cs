using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot
{
    public class MessageCosmosService
    {
        private readonly Container _container;

        public MessageCosmosService(CosmosClient cosmosClient, string dbName, string containerName)
        {
            _container = cosmosClient.GetContainer(dbName, containerName);
        }

        public async Task<Message> Add(Message message)
        {
            var item = await _container.CreateItemAsync(message, new PartitionKey(message.Id));
            return item;
        }

        public async Task<List<Message>> Get(string sqlCosmosQuery)
        {
            var query = _container.GetItemQueryIterator<Message>(new QueryDefinition(sqlCosmosQuery));

            var result = new List<Message>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                result.AddRange(response);
            }

            return result;
        }
    }
}
