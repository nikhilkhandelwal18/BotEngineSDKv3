using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Graph;
using BotEngine.SelfHelpBot.Services;
using BotEngine.QnA;
using BotEngine.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BotEngine.Dialogs.SelfHelpBot
{
  [Serializable]
  [LuisModel(Constants.LUIS_MEMBER_INFORMATION_APP_ID, Constants.LUIS_MEMBER_INFORMATION_SUBSCRIPTION_KEY)]

  public class MemberInformationDialog : LuisDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    private string Name { get; set; }
    private string Question { get; set; }

    #region "None Intent & QnA Information"
    [LuisIntent("")]
    [LuisIntent("none")]
    [LuisIntent("None")]
    public async Task None(IDialogContext context, LuisResult luisResult)
    {
      //await context.PostAsync($"I'm Sorry I don't know what you mean.");

      Question = luisResult.Query;

      Regex regex = new Regex(@"^[0-9]*$");

      if (regex.IsMatch(Question))
      {
        //true
        context.Done(true);
      }
      else
      {
        //ask for signin before call to QnA
        context.Call(Helper.CreateGetTokenDialog(), GetQnAInformation);
      }

      //context.Wait(this.MessageReceived);
    }


    private async Task GetQnAInformation(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;
      var userName = string.Empty;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {

        context.UserData.TryGetValue<string>("UserName", out userName);
        if (string.IsNullOrEmpty(userName))
        {
          var client = new GraphClient(tokenResponseData.Token);
          User user = await client.GetMe();
          userName = user.DisplayName;
          context.UserData.SetValue<string>("UserName", userName);
          await context.PostAsync($"Hello {userName}");
        }


        if (!string.IsNullOrWhiteSpace(Question))
        {
          #region "QnA Maker"
          List<string> answers = QuestionAnswerMaker.GetAnswer(Question);


          if (answers != null && answers.Count > 0)
          {
            string greetMessage = "Hey, thank you for trying out the new  IT Bot.";
            //string answer = answers[0];
            string answer = string.Empty;
            foreach (string ans in answers)
            {
              answer = ans;

              if (answer.Contains(greetMessage))
              {
                // answer.Replace("Hey,", $"Hey {context.Activity.From.Name},");
              }

              else if (answer.Contains("No good match found in KB."))
              {
                //answer = "Looks like I was unable to help you. Please contact **email** for further assistance."; 
                // \U0001F641

                answer = $"Sorry, I could not find answer to your query. Please contact [O365Support@BotEngine.org](outlook:O365Support@BotEngine.org) for further assistance.";
              }

              answer = answer.Replace("\\n\\n", Environment.NewLine);
              await context.PostAsync(answer);
            }

          }
          else
          {
            await context.PostAsync($"I'm Sorry I don't know what you mean.");
          }
          #endregion
        }
        else
        {
          await context.PostAsync($"*Can you please ask question again.*");
        }
      }
      else
      {
        //await context.PostAsync($"*something went wrong, please try again");
        await context.Forward(new MemberInformationDialog(), Callback, context.Activity.AsMessageActivity(), CancellationToken.None);
      }
      context.Done(true);
    }
    #endregion

    #region "Greeting Intent"
    [LuisIntent("GreetingIntent")]
    public async Task Greeting(IDialogContext context, LuisResult luisResult)
    {
      context.Call(new GreetingDialog(), Callback);
    }

    private async Task Callback(IDialogContext context, IAwaitable<object> result)
    {
      //context.Wait(MessageReceived);
      context.Done(true);
    }

    #endregion

    #region "To Know Contact Information Intent"

    [LuisIntent("ToKnowContact")]
    public async Task MemberInformation(IDialogContext context, LuisResult luisResult)
    {
      string name = string.Empty;

      EntityRecommendation nameEntityRec = null;

      if (luisResult.TryFindEntity("builtin.personName", out nameEntityRec))
      {
        Name = nameEntityRec.Entity;
      }
      else if (luisResult.TryFindEntity("FulllName", out nameEntityRec))
      {
        Name = nameEntityRec.Entity;
      }

      //context.Call(Helper.CreateGetTokenDialog(ConnectionName), GetMemberInformation);
      context.Call(Helper.CreateGetTokenDialog(), GetMemberInformation);

    }

    private async Task GetMemberInformation(IDialogContext context, IAwaitable<GetTokenResponse> tokenResponse)
    {
      var tokenResponseData = await tokenResponse;
      var client = new GraphClient(tokenResponseData.Token);
      var userName = string.Empty;

      if (null != tokenResponseData && !string.IsNullOrEmpty(tokenResponseData.Token))
      {
        context.UserData.TryGetValue<string>("UserName", out userName);
        if (string.IsNullOrEmpty(userName))
        {
          User user = await client.GetMe();
          userName = user.DisplayName;
          context.UserData.SetValue<string>("UserName", userName);
          await context.PostAsync($"Hello {userName}");
        }



        if (!string.IsNullOrWhiteSpace(Name))
        {
          IList<Person> answers = await client.GetPeople(Name);
          Name = string.Empty;

          if (context.Activity.ChannelId == "skypeforbusiness")
          {
            foreach (Person answer in answers)
            {
              string contact = string.Empty;
              contact += $"{answer.DisplayName}  {answer.ScoredEmailAddresses} {answer.JobTitle}  {Environment.NewLine}";
              await context.PostAsync(contact);
            }
          }
          else
          {
            var message = context.MakeMessage();
            message.Attachments = new List<Microsoft.Bot.Connector.Attachment>();
            message.AttachmentLayout = "carousel";

            foreach (Person answer in answers)
            {
              //ReceiptCard contactCard = new ReceiptCard()
              //{
              //    Items = new List<ReceiptItem>(),
              //    Title = "Contact Information"
              //};

              //contactCard.Items.Add(new ReceiptItem()
              //{
              //    Title = $"{answer.DisplayName} {Environment.NewLine} {answer.Mail}"
              //});

              //message.Attachments.Add(contactCard.ToAttachment());



              //HeroCard heroCard = new HeroCard
              //{
              //  Title = $"{answer.DisplayName}",
              //  Subtitle = $"{ answer.JobTitle}",
              //  Text = $"{answer.Mail}"
              //};

              HeroCard heroCard = new HeroCard
              {
                Title = $"{answer.DisplayName} {answer.JobTitle}",
                Subtitle = $"{ answer.JobTitle}",
                Text = $"{answer.ScoredEmailAddresses}"                
              };
              message.Attachments.Add(heroCard.ToAttachment());

            }
            await context.PostAsync(message);
          }
        }
        else
        {
          await context.PostAsync($"*Contact Information for <user>*");
        }
      }
      else
      {
        //await context.PostAsync($"*something went wrong, please try again");
        await context.Forward(new MemberInformationDialog(), Callback, context.Activity.AsMessageActivity(), CancellationToken.None);
      }
      context.Done(true);
    }

    #endregion

    #region "Sign Out Intent"
    [LuisIntent("SignOut")]
    public async Task SignOut(IDialogContext context, LuisResult luisResult)
    {
      await context.SignOutUserAsync(ConnectionName);
      await context.PostAsync($"*You have been signed out susccessfully*");
      await context.PostAsync($"*It's my pleasure to help you. Have a nice day ahead. See you soon.*");
      //context.Done<string>("signout");
    }
    #endregion

    #region "Sign In Intent"
    [LuisIntent("SignIn")]
    public async Task SignIn(IDialogContext context, LuisResult luisResult)
    {
      #region MyRegion
      //var userName = string.Empty;
      //var tokenResponse = await context.GetUserTokenAsync(ConnectionName);
      //if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
      //{
      //  context.ConversationData.TryGetValue<string>("UserName", out userName);
      //  if (string.IsNullOrEmpty(userName))
      //  {
      //    var client = new GraphClient(tokenResponse.Token);
      //    User user = await client.GetMe();
      //    userName = user.DisplayName;
      //    context.ConversationData.SetValue<string>("UserName", userName);
      //  }

      //  await context.PostAsync($"*Hey {userName}, You are already here. How may I help you with your query.*");
      //  context.Done(true);
      //}
      //else
      //{

      //  await context.Forward(Helper.CreateGetTokenDialog(ConnectionName),
      //                             StartConversation,
      //                             context.Activity.AsMessageActivity(),
      //                             CancellationToken.None);
      //}

      ////context.Call(new GreetingDialog(), null);
      ///


      #endregion


      ///********ConversationStarter*********/
      //ConversationStarter.toId = ((IMessageActivity)context.Activity).From.Id;
      //ConversationStarter.toName = ((IMessageActivity)context.Activity).From.Name;
      //ConversationStarter.fromId = ((IMessageActivity)context.Activity).Recipient.Id;
      //ConversationStarter.fromName = ((IMessageActivity)context.Activity).Recipient.Name;
      //ConversationStarter.serviceUrl = ((IMessageActivity)context.Activity).ServiceUrl;
      //ConversationStarter.channelId = ((IMessageActivity)context.Activity).ChannelId;
      //ConversationStarter.conversationId = ((IMessageActivity)context.Activity).Conversation.Id;

      //await context.PostAsync($"SignIn! {ConversationStarter.toId} {ConversationStarter.fromId} {ConversationStarter.channelId} {ConversationStarter.conversationId}");
      

      ///***********************************/


      context.Call(Helper.CreateGetTokenDialog(), StartConversation);
    }

    //private GetTokenDialog CreateGetTokenDialog()
    //{
    //  return new GetTokenDialog(
    //      ConnectionName,
    //      $"Please sign in to {ConnectionName} to proceed.",
    //      "Sign In",
    //      0,
    //      "Hmm. Something went wrong, let's try again.");
    //}

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
          var client = new GraphClient(tokenResponseData.Token);
          User user = await client.GetMe();
          userName = user.DisplayName;
          context.ConversationData.SetValue<string>("UserName", userName);
        }

        await context.PostAsync($"*Hey {userName}, You are already here. How may I help you with your query.*");
        
        //await context.PostAsync($"*you are logged in successfully*");
        

      }
      else
      {
        //await context.PostAsync($"*something went wrong, please try again");

        await context.Forward(new MemberInformationDialog(), Callback, context.Activity.AsMessageActivity(), CancellationToken.None);
      }

      context.Done(true);
    }

    #endregion

    //private const string ChangePasswordHelp = "How to Change Password ?";
    //private const string UnlockAccountHelp = "How do I Unlock account ?";
    //private const string UserDetail = "Contact Information for <User>.";
    //private const string RebrandingQuestion = "What is Rebranding?";

    //enum Selection { ChangePassword, UnlockAccount, UserDetail, RebrandingQuestion }

    //List<Selection> options = new List<Selection> { Selection.ChangePassword, Selection.UnlockAccount, Selection.UserDetail, Selection.RebrandingQuestion };
    //List<string> optionDescription = new List<string> { ChangePasswordHelp, UnlockAccountHelp, UserDetail, RebrandingQuestion };

    #region "Help Intent"


    [LuisIntent("HelpIntent")]
    public async Task Help(IDialogContext context, LuisResult luisResult)
    {
      //PromptDialog.Choice<Selection>(context,
      //        ReceivedOperationChoice,
      //        options,
      //        "I can answer questions such as - ", descriptions: optionDescription, promptStyle: PromptStyle.PerLine);

      string helpText = "I can answer questions such as - " +
                          Environment.NewLine + "- How to Change Password ?" +
                          Environment.NewLine + "- How do I Unlock account ?" +
                          Environment.NewLine + "- What is Rebranding?" +
                          Environment.NewLine + "- Contact Information for _<User>_." + Environment.NewLine;

      await context.PostAsync(helpText);
      context.Done(true);
    }


    #endregion

    #region "Change Password Intent"

    [LuisIntent("ChangePasswordIntent")]
    public async Task ChangePassword(IDialogContext context, LuisResult luisResult)
    {
      /*
      await context.PostAsync("ChangePassword Not Implemented");
      context.Done(true);
      */

      /*
      var message = context.MakeMessage();
      message.AttachmentLayout = "carousel";
      message.Attachments = new List<Microsoft.Bot.Connector.Attachment>
              {
                  SelfBotHelper.PasswordResetCard().ToAttachment()
              };
      await context.PostAsync(message);
      context.Done(true);
      */
      await context.PostAsync(SelfBotHelper.PasswordResetHyperLink());
      context.Done(true);

    }

    #endregion

    #region "Unlock Account Intent"

    [LuisIntent("UnlockAccountIntent")]
    public async Task UnlockAccount(IDialogContext context, LuisResult luisResult)
    {
      /*
      await context.PostAsync("UnlockAccount Not Implemented");
      context.Done(true);
      */
      /*
      var message = context.MakeMessage();
      message.AttachmentLayout = "carousel";
      message.Attachments = new List<Microsoft.Bot.Connector.Attachment>
              {
                  SelfBotHelper.UnlockAccountCard().ToAttachment()
              };
      await context.PostAsync(message);
      context.Done(true);
      */
      await context.PostAsync(SelfBotHelper.UnlockAccountHyperLink());
      context.Done(true);
    }

    #endregion
  }
}