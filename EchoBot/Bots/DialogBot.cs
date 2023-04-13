using Azure.Data.Tables;
using EchoBot.Models;
using EchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EchoBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly TableServiceClient _tableServiceClient;

        protected readonly Dialog Dialog;
        protected readonly StateService StateService;
        protected readonly ILogger Logger;

        public DialogBot(T dialog, StateService stateService, ILogger<DialogBot<T>> logger, TableServiceClient tableServiceClient)
        {
            Dialog = dialog;
            StateService = stateService;
            Logger = logger;
            _tableServiceClient = tableServiceClient;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new())
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await StateService.ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
            await StateService.UserState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog from Message Activity");

            await AddConversationReference(turnContext.Activity);

            await Dialog.RunAsync(turnContext, StateService.DialogStateAccessor, cancellationToken);
        }

        private async Task AddConversationReference(IActivity activity)
        {
            var conversation = activity.GetConversationReference();

            var conversationReferenceEntity = new ConversationReferenceEntity
            {
                PartitionKey = conversation.User.Id,
                RowKey = conversation.Conversation.Id,
                ConversationReference = JsonConvert.SerializeObject(activity.GetConversationReference())
            };

            var table = _tableServiceClient.GetTableClient("ConversationReferences");

            await table.UpsertEntityAsync(conversationReferenceEntity);
        }
    }
}
