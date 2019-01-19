using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using BotEngine.Dialogs.ExceptionHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.Dialogs
{

  [Serializable]
  public class SignInDialog : IDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    public string GreetingFromBot()
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

    //public async Task StartAsync(IDialogContext context)
    //{
    //  //await context.PostAsync("Welcome to Auth Bot!");

    //  //await context.PostAsync(GreetingFromBot());

    //  // First ask Bot Service if it already has a token for this user
    //  var token = await context.GetUserTokenAsync(ConnectionName);
    //  if (token != null)
    //  {
    //    //context.Call(new WelcomeCardDialog(), OnDialogEnd);
    //    //await context.Forward(new WelcomeCardDialog(), OnDialogEnd, context.Activity.AsMessageActivity());

    //    await context.Forward(
    //      new ExceptionHandlerDialog<object>(new WelcomeCardDialog(), displayException: false), 
    //      OnDialogEnd, 
    //      context.Activity.AsMessageActivity());
    //  }
    //  else
    //  {
    //    context.Wait(MessageReceivedAsync);
    //  }



    //}

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
    {
      //IMessageActivity message = await result;

      //await context.Forward(Helper.CreateGetTokenDialog(ConnectionName),
      //      AfterGetToken,
      //      message,
      //      CancellationToken.None);
      context.Call(Helper.CreateGetTokenDialog(), StartConversation);
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
          new ExceptionHandlerDialog<object>(new WelcomeCardDialog(), displayException: false),
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


    public Task StartAsync(IDialogContext context)
    {
      //context.Call(Helper.CreateGetTokenDialog(), StartConversation);

      context.Wait(MessageReceivedAsync);

      return Task.CompletedTask;
    }
    private async Task StartConversation(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;
      var userName = string.Empty;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {
        ///********ConversationStarter*********/
        //await context.PostAsync($"SignIn- Token! {tokenResponseData.Token}");
        ///***********************************/

        context.ConversationData.TryGetValue<string>("UserName", out userName);
        if (string.IsNullOrEmpty(userName))
        {
          var client = new Services.GraphClient(tokenResponseData.Token);
          Microsoft.Graph.User user = await client.GetMe();
          userName = user.DisplayName;
          context.ConversationData.SetValue<string>("UserName", userName);
        }

        await context.PostAsync($"*Hey {userName}, You are already here. How may I help you with your query.*");
        await context.Forward(new WelcomeCardDialog(), OnDialogEnd, context.Activity.AsMessageActivity(), CancellationToken.None);
      }
      else
      {
        await context.Forward(new SignInDialog(), Callback, context.Activity.AsMessageActivity(), CancellationToken.None);
      }
    }

    private async Task Callback(IDialogContext context, IAwaitable<object> result)
    {
      context.Done(true);
    }
  }
}