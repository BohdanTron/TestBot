using EchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly StateService StateService;
        protected readonly ILogger Logger;

        public DialogBot(T dialog, StateService stateService, ILogger<DialogBot<T>> logger)
        {
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

            await Dialog.RunAsync(turnContext, StateService.DialogStateAccessor, cancellationToken);
            //await Dialog.Run(turnContext, StateService.DialogStateAccessor, cancellationToken);
        }
    }
}
