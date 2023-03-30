// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly MessageCosmosService _messageCosmosService;

        public EchoBot(MessageCosmosService messageCosmosService) => 
            _messageCosmosService = messageCosmosService;

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Typing indicator
            await turnContext.SendActivityAsync(new Activity { Type = ActivityTypes.Typing }, cancellationToken);

            // Suggested actions
            if (turnContext.Activity.Text == "/suggested-actions")
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
            

            if (!turnContext.Activity.Text.StartsWith("/"))
            {
                var replyText = $"Echo: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

                await _messageCosmosService.Add(new Message { Text = turnContext.Activity.Text });
            }

            if (turnContext.Activity.Text == "/all")
            {
                var allMessages = await _messageCosmosService.GetAll();

                var response = string.Join(", ", allMessages.Select(m => m.Text));

                await turnContext.SendActivityAsync(MessageFactory.Text(response, response), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome (from docker container now)!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
