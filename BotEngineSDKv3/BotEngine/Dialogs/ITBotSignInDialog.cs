using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BotEngine.Dialogs.ExceptionHandler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.Dialogs
{
  [Serializable]
  public class ITBotSignInDialog : IDialog<object>
  {
    private string _connectionName;
    private string _buttonLabel;
    private string _signInMessage;
    private int _retries;
    private string _retryMessage;
    private Guid _guid;

    public ITBotSignInDialog(string connectionName, string signInMessage, string buttonLabel, int retries = 0,
        string retryMessage = null)
    {
      _connectionName = connectionName;
      _signInMessage = signInMessage;
      _buttonLabel = buttonLabel;
      _retries = retries;
      _retryMessage = retryMessage;
    }

    public async Task StartAsync(IDialogContext context)
    {
      // First ask Bot Service if it already has a token for this user
      var token = await context.GetUserTokenAsync(_connectionName);
      if (token != null)
      {
        context.Done(new GetTokenResponse() { Token = token.Token });
      }
      else
      {
        // If Bot Service does not have a token, send an OAuth card to sign in
        //await SendOAuthLinkAsync(context, (Activity)context.Activity);
        context.Wait(MessageReceivedAsync);

      }
    }
    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
    {
      IMessageActivity message = await result;

      await context.Forward(Helper.CreateGetTokenDialog(_connectionName),
            AfterGetToken,
            message,
            CancellationToken.None);
    }

    private async Task AfterGetToken(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {
        await context.PostAsync($"*you are logged in successfully*");

        //context.Call(new MemberInformationDialog(), OnDialogEnd);

        //context.Wait(ConversationStart);
        context.Call(
          new ExceptionHandlerDialog<object>(new ITBotRootDialog(), displayException: false),
          OnDialogEnd);
      }
      else
      {
        //await context.PostAsync($"*something went wrong, please try again");
        await context.PostAsync("Click this Continue button to get your magic code.\r\n Enter the magic code to continue ");
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

  }
}