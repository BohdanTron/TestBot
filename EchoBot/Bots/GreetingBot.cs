using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Models;
using EchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoBot.Bots
{
    public class GreetingBot : ActivityHandler
    {
        private readonly StateService _stateService;

        public GreetingBot(StateService stateService)
        {
            _stateService = stateService;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var conversationData = await _stateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData(), cancellationToken);

            var userProfile = await _stateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
            if (!string.IsNullOrWhiteSpace(userProfile.Name))
            {
                await turnContext.SendActivityAsync($"Hi {userProfile.Name}. How can I help you today?", cancellationToken: cancellationToken);
            }
            else
            {
                if (conversationData.PromptedUserForName)
                {
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. How can I help you today?", cancellationToken: cancellationToken);

                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    await turnContext.SendActivityAsync("What's your name?", cancellationToken: cancellationToken);

                    conversationData.PromptedUserForName = true;
                }
            }

            await _stateService.UserProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);
            await _stateService.ConversationDataAccessor.SetAsync(turnContext, conversationData, cancellationToken);

            await _stateService.UserState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }
    }
}
