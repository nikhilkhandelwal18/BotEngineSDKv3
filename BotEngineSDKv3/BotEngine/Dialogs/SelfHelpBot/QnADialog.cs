using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.SelfHelpBot.Dialogs
{
  [Serializable]
  public class QnADialog : IDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    public async Task StartAsync(IDialogContext context)
    {
      context.Wait(MessageReceivedAsync);
    }

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
    {
      string question = ((await result) as Activity).Text;

      ///********ConversationStarter*********/
      //var message = await result;

      ////We need to keep this data so we know who to send the message to. Assume this would be stored somewhere, e.g. an Azure Table
      //ConversationStarter.toId = ((IMessageActivity)message).From.Id;
      //ConversationStarter.toName = ((IMessageActivity)message).From.Name;
      //ConversationStarter.fromId = ((IMessageActivity)message).Recipient.Id;
      //ConversationStarter.fromName = ((IMessageActivity)message).Recipient.Name;
      //ConversationStarter.serviceUrl = ((IMessageActivity)message).ServiceUrl;
      //ConversationStarter.channelId = ((IMessageActivity)message).ChannelId;
      //ConversationStarter.conversationId = ((IMessageActivity)message).Conversation.Id;

      //await context.PostAsync($"Hello QnADialog! {ConversationStarter.toId} {ConversationStarter.fromId} {ConversationStarter.channelId} {ConversationStarter.conversationId}");
      
      ///***********************************/



      await context.Forward(new BotEngine.Dialogs.SelfHelpBot.MemberInformationDialog(),
                            this.OnDialogEnd,
                            context.Activity.AsMessageActivity(),
                            CancellationToken.None);

      //context.Call(new ExceptionHandlerDialog<object>(new MemberInformationDialog(), displayException: false),
      //                    OnDialogEnd);
    }

    private async Task StartConversation(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {
        await context.PostAsync($"*you are logged in successfully*");

        await context.Forward(
       new BotEngine.Dialogs.SelfHelpBot.MemberInformationDialog(),
       this.OnDialogEnd,
       context.Activity.AsMessageActivity(),
       CancellationToken.None);
      }
      else
      {
        await context.PostAsync($"*something went wrong, please try again");
      }
    }

    private async Task OnDialogEnd(IDialogContext context, IAwaitable<object> result)
    {
      var status = (await result) as string;

      if (null != status && status.Equals("signout", StringComparison.InvariantCultureIgnoreCase))
      {
        context.Done(true);
      }
      else
      {
        context.Wait(MessageReceivedAsync);
      }
    }

    public async Task SignOut(IDialogContext context, IAwaitable<object> result)
    {
      await context.SignOutUserAsync(ConnectionName);
      await context.PostAsync($"*You have been signed out susccessfully*");
      context.Done<string>("signout");
    }
  }
}