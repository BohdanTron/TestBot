using Azure.Data.Tables;
using EchoBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;

        private readonly ILogger _logger;

        private readonly TableServiceClient _tableServiceClient;

        public NotifyController(
            IBotFrameworkHttpAdapter adapter,
            IConfiguration configuration,
            ILogger<NotifyController> logger,
            TableServiceClient tableServiceClient)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _logger = logger;
            _tableServiceClient = tableServiceClient;
        }

        public async Task<ActionResult> Get()
        {
            var table = _tableServiceClient.GetTableClient("ConversationReferences");

            var conversations = table.QueryAsync<ConversationReferenceEntity>();

            await foreach (var conversation in conversations)
            {
                await ((BotAdapter) _adapter).ContinueConversationAsync(_appId, JsonConvert.DeserializeObject<ConversationReference>(conversation.ConversationReference),
                    async (context, token) =>
                    {
                        await context.SendActivityAsync("Hi, it's a proactive message", cancellationToken: token);
                        await context.SendActivityAsync(conversation.ConversationReference, cancellationToken: token);
                    }, CancellationToken.None);
            }

            return new ContentResult
            {
                Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
            };
        }
    }
}
