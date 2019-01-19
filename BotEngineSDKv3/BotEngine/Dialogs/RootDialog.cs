using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Configuration;

namespace BotEngine.Dialogs
{
  [Serializable]
  public class RootDialog : IDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    private const string ShowBenefitsOption = "Show Benefits";
    private const string SignOutOption = "Sign Out";

    [NonSerialized]
    Timer t;

    public async Task StartAsync(IDialogContext context)
    {
      context.Wait(MessageReceivedAsync);
    }

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
    {
      //context.Call(CreateGetTokenDialog(), ShowOptions);

      var message = await result;

      //We need to keep this data so we know who to send the message to. Assume this would be stored somewhere, e.g. an Azure Table
      ConversationStarter.toId = ((IMessageActivity)message).From.Id;
      ConversationStarter.toName = ((IMessageActivity)message).From.Name;
      ConversationStarter.fromId = ((IMessageActivity)message).Recipient.Id;
      ConversationStarter.fromName = ((IMessageActivity)message).Recipient.Name;
      ConversationStarter.serviceUrl = ((IMessageActivity)message).ServiceUrl;
      ConversationStarter.channelId = ((IMessageActivity)message).ChannelId;
      ConversationStarter.conversationId = ((IMessageActivity)message).Conversation.Id;

      //We create a timer to simulate some background process or trigger
      t = new Timer(new TimerCallback(timerEvent));
      t.Change(5000, Timeout.Infinite);

      var url = HttpContext.Current.Request.Url;
      //We now tell the user that we will talk to them in a few seconds
      await context.PostAsync("Hello! In a few seconds I'll send you a message proactively to demonstrate how bots can initiate messages. You can also make me send a message by accessing: " +
              url.Scheme + "://" + url.Host + ":" + url.Port + "/api/CustomWebApi");
      context.Wait(MessageReceivedAsync);
    }

    public void timerEvent(object target)
    {

      t.Dispose();
      ConversationStarter.Resume(ConversationStarter.conversationId, ConversationStarter.channelId); //We don't need to wait for this, just want to start the interruption here
    }


    private readonly List<string> rootOptions = new List<string>()
        {
            ShowBenefitsOption
        };

    private async Task ShowOptions(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      PromptDialog.Choice(
          context,// Current Dialog Context                
          OnOptionSelected,// Callback after option selection                
          rootOptions,// Available Options                
          "What you would like to do today?",// Prompt text                 
          "Not a valid option",// Invalid input message                
          3,// How many times retry                
          PromptStyle.Auto,// Display Style
          null);
    }

    private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
    {
      try
      {
        string optionSelected = await result;

        switch (optionSelected)
        {
          case ShowBenefitsOption:
            context.Call(CreateGetTokenDialog(), ShowBenefits);
            break;
          case SignOutOption:
            await Signout(context);
            break;
        }
      }
      catch (TooManyAttemptsException exception)
      {
        await context.PostAsync("Ooops! Too many attemps. You can try again!");
      }
    }

    private GetTokenDialog CreateGetTokenDialog()
    {
      return new GetTokenDialog(ConnectionName, $"Please sign in to proceed.", "Sign In", 2, "Hmm. Something went wrong, let's try again.");
    }

    public static async Task ShowBenefits(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;
      await context.PostAsync($"You have selected {ShowBenefitsOption}.");
    }

    public static async Task Signout(IDialogContext context)
    {
      await context.SignOutUserAsync(ConnectionName);
      await context.PostAsync($"You have been signed out.");
    }
  }
}