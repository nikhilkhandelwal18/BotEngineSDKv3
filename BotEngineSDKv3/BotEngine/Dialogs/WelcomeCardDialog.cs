using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Threading;
using System.Configuration;
using BotEngine.Dialogs.ExceptionHandler;

namespace BotEngine.Dialogs
{
  [Serializable]
  public class WelcomeCardDialog : IDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    private const string BenefitsCoverage = "Benefits & Coverage";
    private const string CostShare = "Cost Share";
    private const string Claims = "Claims";
    private const string Others = "Others";
    private const string SignOut = "SignOut";
    private const string SignIn = "SignIn";
    enum Selection { BenefitsCoverage, CostShare, Claims, Others, SignOut, SignIn }

    List<Selection> options = new List<Selection> { Selection.BenefitsCoverage, Selection.CostShare, Selection.Claims, Selection.Others };
    List<string> optionDescription = new List<string> { BenefitsCoverage, CostShare, Claims, Others };


    public async Task StartAsync(IDialogContext context)
    {
      context.Wait(ConversationStart);
    }

    public async Task ConversationStart(IDialogContext context, IAwaitable<object> argument)
    {
      TokenResponse tokenResponse = await context.GetUserTokenAsync(ConnectionName);

      if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
      {
        if (!options.Contains(Selection.SignOut))
        {
          options.Add(Selection.SignOut);
          optionDescription.Add(SignOut);
        }
        if (options.Contains(Selection.SignIn))
        {
          options.Remove(Selection.SignIn);
          optionDescription.Remove(SignIn);
        }
      }
      else
      {
        if (!options.Contains(Selection.SignIn))
        {
          options.Add(Selection.SignIn);
          optionDescription.Add(SignIn);
        }
        if (options.Contains(Selection.SignOut))
        {
          options.Remove(Selection.SignOut);
          optionDescription.Remove(SignOut);
        }
      }


      PromptDialog.Choice<Selection>(context,
         ReceivedOperationChoice,
         options,
         "Which selection do you want?", descriptions: optionDescription, promptStyle: PromptStyle.Auto);

      //context.Wait(MessageReceivedOperationChoice);
    }

    private async Task ReceivedOperationChoice(IDialogContext context, IAwaitable<Selection> argument)
    {
      var prompt = await argument;
      //string promptString = prompt.ToString().ToLower().Trim();

      try
      {
        switch (prompt)
        {
          case Selection.BenefitsCoverage:
            await context.PostAsync($"I'm here to help you with any '{prompt.ToString()}' related queries");

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Text = "Hello!";
            message.TextFormat = "plain";
            message.Locale = "en-Us";

            await context.Forward(
              new ExceptionHandlerDialog<object>(new MemberInformationDialog(), displayException: false),
              AfterChildDialogIsDone,
              message,
              CancellationToken.None);


            break;
          case Selection.CostShare:
            PromptDialog.Text(context: context,
               resume: ConversationStart,
               prompt: $"Not Implemented !!!",
               retry: "I didn't understand. Please try again.");

            break;
          case Selection.Claims:
            PromptDialog.Text(context: context,
                   resume: ConversationStart,
                   prompt: $"For '{Claims}' related queries please contact <Phone number> or by email <email address>. Thank you",
                   retry: "I didn't understand. Please try again.");

            break;
          case Selection.Others:
            PromptDialog.Text(context: context,
                             resume: ConversationStart,
                             prompt: $"For '{Others}' related queries please contact <Phone number> or by email <email address>. Thank you",
                             retry: "I didn't understand. Please try again.");

            break;

          case Selection.SignOut:
            PromptDialog.Confirm(context, onSignOutCallback, "Do you want to exit?");

            break;
          case Selection.SignIn:
            //context.Done(true);

            //message = Activity.CreateMessageActivity();
            //message.Text = "signin"; //"Hello!";
            //message.TextFormat = "plain";
            //message.Locale = "en-Us";

            //await context.Forward(
            //  new ExceptionHandlerDialog<object>(new SignInDialog(), displayException: false),
            //  AfterChildDialogIsDone,
            //  message,
            //  CancellationToken.None);

            context.Call(Helper.CreateGetTokenDialog(), StartConversation);

            break;

          default:
            break;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

    }

    private async Task onSignOutCallback(IDialogContext context, IAwaitable<bool> result)
    {
      bool isExit = await result;

      if (isExit)
      {
        await context.SignOutUserAsync(ConnectionName);
        PromptDialog.Text(context: context,
                       resume: ResumeAndConfirm,
                       prompt: $"*You have been signed out susccessfully*",
                       retry: "I didn't understand. Please try again.");
      }
      else
      {
        PromptDialog.Text(context: context,
                     resume: ConversationStart,
                     prompt: $"*Continue* ?",
                     retry: "I didn't understand. Please try again.");
      }
    }

    public async Task ResumeAndConfirm(IDialogContext context, IAwaitable<string> result)
    {
      //bool confirm = await result;

      context.Done(true);

      // Create a lead record in CRM
      // CreateLeadinCRM();

    }

    private async Task AfterChildDialogIsDone(IDialogContext context, IAwaitable<object> result)
    {
      //context.Wait(ConversationStart);

      //PromptDialog.Choice<Selection>(context,
      //   ReceivedOperationChoice,
      //   options,
      //   "Which selection do you want?", descriptions: optionDescription, promptStyle: PromptStyle.Auto);

      //await context.Forward(new SignInDialog(),
      //    AfterChildDialogIsDone,
      //    context.Activity.AsMessageActivity(),
      //    CancellationToken.None);

      //context.Done(true);
      //context.Call(new WelcomeCardDialog(), OnDialogEnd);

      context.Done<string>("signout");
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
        context.Wait(ConversationStart);
      }
    }

    private async Task StartConversation(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;
      var userName = string.Empty;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {
        /********ConversationStarter*********/
        await context.PostAsync($"SignIn- Token! {tokenResponseData.Token}");
        /***********************************/


        context.ConversationData.TryGetValue<string>("UserName", out userName);
        if (string.IsNullOrEmpty(userName))
        {
          var client = new Services.GraphClient(tokenResponseData.Token);
          Microsoft.Graph.User user = await client.GetMe();
          userName = user.DisplayName;
          context.ConversationData.SetValue<string>("UserName", userName);
        }

        await context.PostAsync($"*Hey {userName}, You are already here. How may I help you with your query.*");

        //await context.PostAsync($"*you are logged in successfully*");


      }
      else
      {
        //await context.PostAsync($"*something went wrong, please try again");

        await context.Forward(new SignInDialog(), Callback, context.Activity.AsMessageActivity(), CancellationToken.None);
      }

      context.Done(true);
    }

    private async Task Callback(IDialogContext context, IAwaitable<object> result)
    {
      //context.Wait(MessageReceived);
      context.Done(true);
    }
  }

}