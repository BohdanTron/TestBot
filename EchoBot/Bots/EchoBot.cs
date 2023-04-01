// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using EchoBot.ConversationData;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly MessageCosmosService _messageCosmosService;

        public EchoBot(
            MessageCosmosService messageCosmosService,
            ConversationState conversationState,
            UserState userState)
        {
            _messageCosmosService = messageCosmosService;
            _conversationState = conversationState;
            _userState = userState;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new())
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Typing indicator
            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            var conversationData = await _userState.CreateProperty<ConversationData.ConversationData>(nameof(ConversationState))
                .GetAsync(turnContext, () => new ConversationData.ConversationData(), cancellationToken);

            if (conversationData.PromptedUserForName)
            {
                await SendStateData(turnContext, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("What's your name?", cancellationToken: cancellationToken);

                conversationData.PromptedUserForName = true;

                return;
            }

            // Suggested actions
            if (turnContext.Activity.Text == "/actions")
            {
                var reply = MessageFactory.Text("What's your favorite color?");

                reply.SuggestedActions = new SuggestedActions
                {
                    Actions = new List<CardAction>
                    {
                        new() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red", Image = "https://via.placeholder.com/20/FF0000?text=R", ImageAltText = "R" },
                        new() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow", Image = "https://via.placeholder.com/20/FFFF00?text=Y", ImageAltText = "Y" },
                        new() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue", Image = "https://via.placeholder.com/20/0000FF?text=B", ImageAltText = "B" }
                    }
                };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }

            // All messages
            else if (turnContext.Activity.Text == "/all")
            {
                var allMessages = await _messageCosmosService.GetAll();

                var response = string.Join(", ", allMessages.Select(m => m.Text));

                await turnContext.SendActivityAsync(MessageFactory.Text(response, response), cancellationToken);
            }

            // Echo
            else if (!turnContext.Activity.Text.StartsWith("/"))
            {
                var replyText = $"Echo: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

                await _messageCosmosService.Add(new Message { Text = turnContext.Activity.Text });
            }
        }

        private async Task SendStateData(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationData = await _userState.CreateProperty<ConversationData.ConversationData>(nameof(ConversationState))
                .GetAsync(turnContext, () => new ConversationData.ConversationData(), cancellationToken);

            var userProfile = await _userState.CreateProperty<UserProfile>(nameof(UserProfile))
                .GetAsync(turnContext, () => new UserProfile(), cancellationToken);

            userProfile.Name = turnContext.Activity.Text?.Trim();

            await turnContext.SendActivityAsync($"Thanks {userProfile.Name}, here's your state data:",
                cancellationToken: cancellationToken);

            conversationData.Timestamp = turnContext.Activity.Timestamp?.ToLocalTime().ToString();
            conversationData.ChannelId = turnContext.Activity.ChannelId;

            // Display state data.
            await turnContext.SendActivityAsync($"Message received at: {conversationData.Timestamp}",
                cancellationToken: cancellationToken);
            await turnContext.SendActivityAsync($"Message received from: {conversationData.ChannelId}",
                cancellationToken: cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationData = await _userState.CreateProperty<ConversationData.ConversationData>(nameof(ConversationState))
                .GetAsync(turnContext, () => new ConversationData.ConversationData(), cancellationToken);

            var welcomeText = "Hello, what's your name?";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);

                    conversationData.PromptedUserForName = true;
                }
            }
        }
    }
}
