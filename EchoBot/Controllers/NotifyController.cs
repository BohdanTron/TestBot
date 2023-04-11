using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EchoBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _confConversationReferences;

        private readonly ILogger _logger;

        public NotifyController(
            IBotFrameworkHttpAdapter adapter,
            IConfiguration configuration,
            ConcurrentDictionary<string, ConversationReference> confConversationReferences,
            ILogger<NotifyController> logger)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _confConversationReferences = confConversationReferences;
            _logger = logger;
        }

        public async Task<ActionResult> Get()
        {
            foreach (var conversation in _confConversationReferences.Values)
            {
                var message =
                    $"ActivityId = {conversation.ActivityId}\n ChannelId = {conversation.ChannelId}\n BotName = {conversation.Bot.Name}\n Username = {conversation.User.Name}\n ServiceUrl = {conversation.ServiceUrl}";

                _logger.LogInformation(message);

                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversation,
                    async (context, token) =>
                    {
                        await context.SendActivityAsync("Hi, it's a proactive message", cancellationToken: token);
                    }, CancellationToken.None);
            }

            return new ContentResult
            {
                Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }
    }
}
