using System;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Services;
using Microsoft.Bot.Builder.Dialogs;

namespace EchoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        public MainDialog(StateService stateService)
            : base(nameof(MainDialog))
        {
            _stateService = stateService;

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStep,
                FinalStep
            };

            // Add Named Dialog
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugReport", _stateService));

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.Equals(stepContext.Context.Activity.Text, "hi", StringComparison.InvariantCultureIgnoreCase))
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", cancellationToken: cancellationToken);
            }

            return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugReport", cancellationToken: cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
