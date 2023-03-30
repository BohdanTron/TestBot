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
            if (string.Equals(turnContext.Activity.Text, "wait", StringComparison.CurrentCultureIgnoreCase))
            {
                await turnContext.SendActivitiesAsync(new IActivity[]
                {
                    new Activity { Type = ActivityTypes.Typing },
                    new Activity { Type = "delay", Value = 3000 },
                    MessageFactory.Text("Finished typing", "Finished typing")
                }, cancellationToken);
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
