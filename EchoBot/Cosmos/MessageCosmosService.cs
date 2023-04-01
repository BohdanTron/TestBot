using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
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
            var item = await _container.CreateItemAsync(message, new PartitionKey(message.Id.ToString()));
            return item;
        }

        public async Task<List<Message>> GetAll()
        {
            var iterator = _container.GetItemLinqQueryable<Message>().ToFeedIterator();

            var result = new List<Message>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                result.AddRange(response);
            }

            return result;
        }
    }
}
