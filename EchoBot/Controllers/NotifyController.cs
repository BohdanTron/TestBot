using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;

namespace EchoBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> _confConversationReferences;

        public NotifyController(
            IBotFrameworkHttpAdapter adapter, 
            ConcurrentDictionary<string, ConversationReference> confConversationReferences)
        {
            _adapter = adapter;
            _confConversationReferences = confConversationReferences;
        }

        public async Task<ActionResult> Get()
        {
            foreach (var conversation in _confConversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(string.Empty, conversation,
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
