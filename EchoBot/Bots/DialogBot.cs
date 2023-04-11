using EchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        protected readonly Dialog Dialog;
        protected readonly StateService StateService;
        protected readonly ILogger Logger;

        public DialogBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, T dialog, StateService stateService, ILogger<DialogBot<T>> logger)
        {
            _conversationReferences = conversationReferences;

            Dialog = dialog;
            StateService = stateService;
            Logger = logger;
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

            AddConversationReference(turnContext.Activity);

            await Dialog.RunAsync(turnContext, StateService.DialogStateAccessor, cancellationToken);
        }

        private void AddConversationReference(IActivity activity)
        {
            var conversationReference = activity.GetConversationReference();

            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference,
                (_, _) => conversationReference);
        }
    }
}
