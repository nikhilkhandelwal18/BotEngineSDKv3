using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BotEngine.Dialogs;
using BotEngine.Dialogs.ExceptionHandler;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BotEngine.Controllers
{
  [BotAuthentication]
  public class MessagesController : ApiController
  {
    public MessagesController()
    {
      System.Diagnostics.Debug.WriteLine("Ctor Messages");
    }

    public string GetTest()
    {
      return "Healthy Running";
    }

    /// <summary>
    /// POST: api/Messages
    /// Receive a message from a user and reply to it
    /// </summary>
    public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
    {
      if (activity.Type == ActivityTypes.Message)
      {
        await Conversation.SendAsync(activity, () => new ExceptionHandlerDialog<object>(new RootDialog(), displayException: false));
      }
      else
      {
        HandleSystemMessage(activity);
      }
      var response = Request.CreateResponse(HttpStatusCode.OK);
      return response;
    }

    private Activity HandleSystemMessage(Activity message)
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

        //// Note: Add introduction here: It sends msg two times
        //ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
        //Activity reply = message.CreateReply("Hello from my simple Bot!");
        //connector.Conversations.ReplyToActivityAsync(reply);

        //// Note: Add introduction here:
        //IConversationUpdateActivity update = message;
        //var client = new ConnectorClient(new Uri(message.ServiceUrl), new MicrosoftAppCredentials());
        //if (update.MembersAdded != null && update.MembersAdded.Any())
        //{
        //  foreach (var newMember in update.MembersAdded)
        //  {
        //    if (newMember.Id != message.Recipient.Id)
        //    {
        //      var reply = message.CreateReply();
        //      reply.Text = $"Welcome {newMember.Name}!";
        //      client.Conversations.ReplyToActivityAsync(reply);
        //    }
        //  }
        //}
      }
      else if (message.Type == ActivityTypes.ContactRelationUpdate)
      {
        // Handle add/remove from contact lists
        // Activity.From + Activity.Action represent what happened
      }
      else if (message.Type == ActivityTypes.Typing)
      {
        // Handle knowing tha the user is typing
      }
      else if (message.Type == ActivityTypes.Ping)
      {
      }

      return null;
    }
  }
}