using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BotEngine.Dialogs.ExceptionHandler;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BotEngine
{
  [BotAuthentication]
  public class CommunicationsController : ApiController
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

        await Conversation.SendAsync(activity, () => new ExceptionHandlerDialog<object>(new Dialogs.SignInDialog(), displayException: false));

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

        //System.Diagnostics.Debug.WriteLine("ConversationUpdate");
        //ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));
        //Activity reply = message.CreateReply("Hello from my simple Bot!" + GreetingFromBot());
        //connector.Conversations.ReplyToActivityAsync(reply);

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
            }
          }
        }

      }
      else if (message.Type == ActivityTypes.ContactRelationUpdate)
      {
        // Handle add/remove from contact lists
        // Activity.From + Activity.Action represent what happened
      }
      else if (message.Type == ActivityTypes.Ping)
      {
      }
      else if (message.Type == ActivityTypes.Event)
      {
        // Send TokenResponse Events along to the Dialog stack
        if (message.IsTokenResponseEvent())
        {
          await Conversation.SendAsync(message, () => new Dialogs.SignInDialog());
        }
        //else
        //{
        //  var reply = message.CreateReply();
        //  reply.Text = GreetingFromBot(); //$"Welcome {newMember.Name}!";
        //  var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
        //  await client.Conversations.ReplyToActivityAsync(reply);
        //}
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

      return $"Hi \U0001F600, {greeting}, I'm CSR Bot at your service. How can I help you today ?";
    }

  }
}