using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tab.Slack.Bot.Integration;
using Tab.Slack.Common.Model.Events;
using ZendeskApi_v2;
using ZendeskApi_v2.Models.Tickets;

namespace Tab.Slack.Bot.Plugins.ZenDesk
{
    public class ZenDeskHandler : MessageHandlerBase, IMessageHandler
    {
        private IZendeskApi zenDeskApi;

        public bool CanHandle(EventMessageBase message)
        {
            return message.MatchesText(@"(can|could|would) (I|you) please.+\?");
        }

        public async Task<ProcessingChainResult> HandleMessageAsync(EventMessageBase message)
        {
            // TODO: figure out config approach
            this.zenDeskApi = new ZendeskApi("url", "username", apiToken: "token");

            var ticket = new Ticket
            {
                Requester = new Requester
                {
                    Name = "name of slack user",
                    Email = "email of slack user"
                },
                Subject = "message text",
                Comment = new Comment
                {
                    Body = "message text"
                }
            };

            await this.zenDeskApi.Tickets.CreateTicketAsync(ticket);

            return ProcessingChainResult.Continue;
        }
    }
}
