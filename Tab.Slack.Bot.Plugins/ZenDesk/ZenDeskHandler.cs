using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tab.Slack.Bot.Integration;
using Tab.Slack.Common.Model.Events;
using Tab.Slack.Common.Model.Events.Messages;
using Tab.Slack.WebApi;
using ZendeskApi_v2;
using ZendeskApi_v2.Models.Tickets;

namespace Tab.Slack.Bot.Plugins.ZenDesk
{
    [Export(typeof(IMessageHandler))]
    public class ZenDeskHandler : MessageHandlerBase, IMessageHandler
    {
        private IZendeskApi zenDeskApi;

        [Import]
        public ISlackApi SlackApi { get; set; }

        public bool CanHandle(EventMessageBase message)
        {
            var canHandle = message.MatchesText(@"it-services");

            // TODO: make into helper method
            if (canHandle)
            {
                var textMessage = message.CastTo<MessageBase>();
                var channel = base.BotState.Channels.FirstOrDefault(c => c.Id == textMessage.Channel);

                canHandle = channel.Name == "it-services";
            }

            return canHandle;
        }

        public async Task<ProcessingChainResult> HandleMessageAsync(EventMessageBase message)
        {
            SetupApi();
            var textMessage = message.CastTo<MessageBase>();
            var user = base.BotState.Users.FirstOrDefault(u => u.Id == textMessage.User);

            var ticket = new Ticket
            {
                Requester = new Requester
                {
                    Name = user.Profile.RealName,
                    Email = user.Profile.Email
                },
                Subject = textMessage.Text,
                Comment = new Comment
                {
                    Body = textMessage.Text
                }
            };

            var ticketResponse = await this.zenDeskApi.Tickets.CreateTicketAsync(ticket);

            base.BotServices.SendMessage(textMessage.Channel, $"Thank you for your query @{user.Name}, we have created a new support ticket for your request: Ticket{ticketResponse.Ticket.Id}");

            return ProcessingChainResult.Continue;
        }

        private void SetupApi()
        {
            if (this.zenDeskApi != null)
                return;

            var url = ConfigurationManager.AppSettings["zendesk.url"];
            var user = ConfigurationManager.AppSettings["zendesk.user"];
            var apiKey = ConfigurationManager.AppSettings["zendesk.apikey"];

            this.zenDeskApi = new ZendeskApi(url, user, apiToken: apiKey);
        }
    }
}
