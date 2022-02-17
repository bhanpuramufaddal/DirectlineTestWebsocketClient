using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using WebSocketSharp;
using Activity = Microsoft.Bot.Connector.DirectLine.Activity;
using System.Collections.Generic;

namespace NotifytriggerConsole
{

    class Program
    {
        static async Task Main(string[] args)
        {
            //string botId = "ValuenceTechBot";
            string directLineSecret = "insert your directline secret";
            string fromUser = "DirectLineSampleClientUser";
            var tokenResponse = await new DirectLineClient(directLineSecret).Tokens.GenerateTokenForNewConversationAsync();
            var directLineClient1 = new DirectLineClient(tokenResponse.Token);
            //var directLineClient2 = new DirectLineClient(tokenResponse.Token);
            //Console.WriteLine(tokenResponse.Token);
            var conversation1 = await directLineClient1.Conversations.StartConversationAsync();
            //var conversation2 = await directLineClient2.Conversations.StartConversationAsync();
            
            //Console.WriteLine(conversation.StreamUrl);
            var webSocketClient1 = new WebSocket(conversation1.StreamUrl);
            //var webSocketClient2 = new WebSocket(conversation2.StreamUrl);

            IDictionary<string,IList<Activity>> Response = new Dictionary<string,IList<Activity>>();

            webSocketClient1.OnMessage += (object sender, MessageEventArgs e) => {
                string botId = "ValuenceTechBot";

                // Occasionally, the Direct Line service sends an empty message as a liveness ping. Ignore these messages.
                if (string.IsNullOrWhiteSpace(e.Data))
                {
                    return;
                }

                var activitySet = JsonConvert.DeserializeObject<ActivitySet>(e.Data);
                var activities = from x in activitySet.Activities
                                 where x.From.Id == botId
                                 select x;
                if (activities.First().ReplyToId == null) { return; }
                IList<Activity> ActivityList = activities.ToList();
                Response.Add(ActivityList.First().ReplyToId, ActivityList);

            };

            // You have to specify TLS version to 1.2 or connection will be failed in handshake.
            webSocketClient1.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            webSocketClient1.Connect();


            string input1 = "Command Watch";
            string input2 = "command getrequests";

            var userMessage1 = new Activity
            {
                From = new ChannelAccount(fromUser),
                Text = input1,
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment>()
            };

            var userMessage2 = new Activity
            {
                From = new ChannelAccount(fromUser),
                Text = input2,
                Type = ActivityTypes.Message
            };

         
           var response1 = await directLineClient1.Conversations.PostActivityAsync(conversation1.ConversationId, userMessage1);
           var response2 = await directLineClient1.Conversations.PostActivityAsync(conversation1.ConversationId, userMessage2);

           Console.WriteLine(JsonConvert.SerializeObject(Response));
        }
    }
}
