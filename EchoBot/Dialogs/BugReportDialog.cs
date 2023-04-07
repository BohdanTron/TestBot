using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Models;
using EchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace EchoBot.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        public BugReportDialog(string dialogId, StateService stateService)
            : base(dialogId)
        {
            _stateService = stateService;

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStep,
                CallbackTimeStep,
                PhoneNumberStep,
                BugStep,
                SummaryStep
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidator));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidator));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> DescriptionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter a description for your report")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> CallbackTimeStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter time"),
                    RetryPrompt = MessageFactory.Text("The value entered must be between 9 am and 5 pm")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["callbackTime"] =
                Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault()?.Value);

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter a phone number that we can call you back"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid phone number")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> BugStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = stepContext.Result.ToString();

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter the type of the bug."),
                    Choices = ChoiceFactory.ToChoices(new List<string> {"Security", "Crash", "Performance", "Other"})
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["bug"] = ((FoundChoice)stepContext.Result).Value;

            var userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context,
                () => new UserProfile(), cancellationToken);

            userProfile.Description = (string)stepContext.Values["description"];
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = (string)stepContext.Values["phoneNumber"];
            userProfile.Bug = (string)stepContext.Values["bug"];

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here's the summary of your bug report"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Description: {userProfile.Description}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time: {userProfile.CallbackTime}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number: {userProfile.PhoneNumber}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug: {userProfile.Bug}"), cancellationToken);

            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private Task<bool> PhoneNumberValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var result = Regex.Match(promptContext.Recognized.Value,
                    @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$").Success;

                return Task.FromResult(result);
            }

            return Task.FromResult(false);
        }

        private Task<bool> CallbackTimeValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var value = promptContext.Recognized.Value.First();
                var selectedDate = Convert.ToDateTime(value.Value);

                var start = new TimeSpan(9, 0, 0);
                var end = new TimeSpan(17, 0, 0);

                if (selectedDate.TimeOfDay >= start && selectedDate.TimeOfDay <= end)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
