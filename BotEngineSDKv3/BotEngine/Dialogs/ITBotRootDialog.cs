using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using BotEngine.Dialogs.ExceptionHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotEngine.Dialogs
{
  [Serializable]
  public class ITBotRootDialog : IDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    private const string Option1 = "Option1";
    private const string Option2 = "Option2";
    private const string Others = "Others";
    private const string SignOut = "SignOut";
    private const string SignIn = "SignIn";
    private const string PasswordResetOption = "Password Reset";

    enum Selection { Option1, Option2, Others, SignOut, SignIn, PasswordResetOption }

    List<Selection> options = new List<Selection> { Selection.Option1, Selection.Option2, Selection.Others };
    List<string> optionDescription = new List<string> { Option1, Option2, Others };


    public async Task StartAsync(IDialogContext context)
    {
      context.Wait(MessageReceivedAsync);
    }

    public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> argument)
    {
      TokenResponse tokenResponse = await context.GetUserTokenAsync(ConnectionName);

      if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
      {
        if (!options.Contains(Selection.SignOut))
        {
          options.Add(Selection.SignOut);
          optionDescription.Add(SignOut);

          options.Add(Selection.PasswordResetOption);
          optionDescription.Add(PasswordResetOption);
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

          options.Remove(Selection.PasswordResetOption);
          optionDescription.Remove(PasswordResetOption);
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
          case Selection.Option1:
            PromptDialog.Text(context: context,
               resume: MessageReceivedAsync,
               prompt: $"Not Implemented !!!",
               retry: "I didn't understand. Please try again.");

            break;
          case Selection.Option2:
            PromptDialog.Text(context: context,
                   resume: MessageReceivedAsync,
                   prompt: $"For '{Option2}' related queries please contact <Phone number> or by email <email address>. Thank you",
                   retry: "I didn't understand. Please try again.");

            break;
          case Selection.Others:
            PromptDialog.Text(context: context,
                             resume: MessageReceivedAsync,
                             prompt: $"For '{Others}' related queries please contact <Phone number> or by email <email address>. Thank you",
                             retry: "I didn't understand. Please try again.");

            break;

          case Selection.SignOut:
            // Sign the user out from AAD
            await Signout(context);

            break;
          case Selection.SignIn:

            context.Call(CreateSignInDialog(), ListMe);

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

    public static async Task Signout(IDialogContext context)
    {
      await context.SignOutUserAsync(ConnectionName);
      await context.PostAsync($"You have been signed out.");
    }

    /// <summary>
    /// Creates a GetTokenDialog using custom strings
    /// </summary>
    private ITBotSignInDialog CreateSignInDialog()
    {
      return new ITBotSignInDialog(
          ConnectionName,
          $"Please sign in to {ConnectionName} to proceed.",
          "Sign In",
          0,
          "Hmm. Something went wrong, let's try again.");
    }

    private async Task ListMe(IDialogContext context, IAwaitable<object> tokenResponse)
    {
      var token = await tokenResponse;
      //var client = new SimpleGraphClient(token.Token);

      //var me = await client.GetMe();
      //var manager = await client.GetManager();

      //await context.PostAsync($"You are {me.DisplayName} and you report to {manager.DisplayName}.");

      await context.PostAsync($"You are Sign In.");
    }

  }
}