using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BotEngine.SelfHelpBot
{
  [BotAuthentication]
  public class SelfBotController : ApiController
  {
    /// <summary>
    /// POST: api/Communications
    /// Receive a message from a user and reply to it
    /// </summary>
    public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
    {
      if (activity.Type == ActivityTypes.Message)
      {
        ConnectorClient connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
        Activity isTypingReply = activity.CreateReply("bot is typing...");
        isTypingReply.Type = ActivityTypes.Typing;
        await connector.Conversations.ReplyToActivityAsync(isTypingReply);


        ///****Conversation Tokens****/
        //var reply = activity.CreateReply();
        //reply.Text = "HI FROM BOT";


        //await connector.Conversations.ReplyToActivityAsync(reply);
        ///**************************/

        string resourcesPath = this.Url.Request.RequestUri.AbsoluteUri.Replace(@"api/SelfBot", "");




        //await Conversation.SendAsync(activity, () => new Dialogs.QnADialog());
        await Conversation.SendAsync(activity, () => new BotEngine.Dialogs.ExceptionHandler.ExceptionHandlerDialog<object>(new Dialogs.QnADialog(), displayException: true));

      }
      else
      {
        await HandleSystemMessage(activity);
      }
      var response = Request.CreateResponse(HttpStatusCode.OK);
      return response;
    }

    private async Task<Activity> HandleSystemMessage(Activity message)
    {
      if (message.Type == ActivityTypes.DeleteUserData)
      {
        // Implement user deletion here
        // If we handle user deletion, return a real message
      }
      else if (message.Type == ActivityTypes.ConversationUpdate)
      {
        // Handle conversation state changes, like members being added and removed
        // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
        // Not available in all channels

        IConversationUpdateActivity update = message;
        var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
        if (update.MembersAdded != null && update.MembersAdded.Any())
        {
          foreach (var newMember in update.MembersAdded)
          {
            if (newMember.Id != message.Recipient.Id)
            {
              var reply = message.CreateReply();
              reply.Text = GreetingFromBot(); //$"Welcome {newMember.Name}!";
              await client.Conversations.ReplyToActivityAsync(reply);

              reply.Text = Environment.NewLine + "- How to Change Password ?" +
                          Environment.NewLine + "- How do I Unlock account ?" +
                          Environment.NewLine + "- What is Rebranding?" +
                          Environment.NewLine + "- Contact Information for _<User>_.";
              await client.Conversations.ReplyToActivityAsync(reply);

              reply.Text = "At anytime type **\"Help\"** for more information.";
              await client.Conversations.ReplyToActivityAsync(reply);

              reply.Text = "Type **\"Login\"** for login.";
              await client.Conversations.ReplyToActivityAsync(reply);
            }
          }
        }
      }
      else if (message.Type == ActivityTypes.ContactRelationUpdate)
      {
        // Handle add/remove from contact lists
        // Activity.From + Activity.Action represent what happened
        var reply = message.CreateReply();
        reply.Text = GreetingFromBot(); //$"Welcome {newMember.Name}!";
        var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
        await client.Conversations.ReplyToActivityAsync(reply);



      }
      else if (message.Type == ActivityTypes.Event)
      {
        // Send TokenResponse Events along to the Dialog stack
        if (message.IsTokenResponseEvent())
        {
          await Conversation.SendAsync(message, () => new Dialogs.QnADialog());
        }
        else
        {
          var reply = message.CreateReply();
          reply.Text = GreetingFromBot(); //$"Welcome {newMember.Name}!";
          var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
          await client.Conversations.ReplyToActivityAsync(reply);
        }
      }

      return null;
    }

    private string GreetingFromBot()
    {
      string greeting = string.Empty;
      if (DateTime.Now.Hour < 12)
      {
        greeting = "Good Morning";
      }
      else if (DateTime.Now.Hour < 17)
      {
        greeting = "Good Afternoon";
      }
      else
      {
        greeting = "Good Evening";
      }

      // \U0001F642
      return $"Hi {greeting}, Welcome to IT Chatbot. I can answer questions such as - ";

    }
  }


}