using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using BotEngine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace BotEngine.Dialogs
{
  [Serializable]
  [LuisModel(Constants.LUIS_MEMBER_INFORMATION_APP_ID, Constants.LUIS_MEMBER_INFORMATION_SUBSCRIPTION_KEY)]
  public class MemberInformationDialog : LuisDialog<object>
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    private Member Member = null;

    private string Benefit = string.Empty;
    private string CostType = string.Empty;

    //public MemberInformationDialog()
    //{

    //}

    //public async Task StartAsync(IDialogContext context)
    //{
    //  context.Wait(MessageReceived);
    //}


    [LuisIntent("")]
    [LuisIntent("none")]
    [LuisIntent("None")]
    public async Task None(IDialogContext context, LuisResult luisResult)
    {
      //await context.PostAsync($"Let me search it in knowledge base");

      // Get answer from QnA Maker



      await context.PostAsync($"Sorry , I did not understand");
      context.Wait(this.MessageReceived);
    }

    [LuisIntent("Member.Information")]
    public async Task Member_Information(IDialogContext context, LuisResult luisResult)
    {
      if (null == Member)
      {
        context.Call(new AskMemberIdDialog(), OnNewMemberId);
      }
      else
      {
        var message = context.MakeMessage();
        message.Attachments = new List<Attachment>
                    {
                        Helper.GetMemberCard(Member).ToAttachment()
                    };
        message.AttachmentLayout = "carousel";
        await context.PostAsync(message);
      }
    }

    private async Task OnNewMemberId(IDialogContext context, IAwaitable<object> result)
    {
      try
      {
        var memberId = (await result) as string;

        //await context.PostAsync(memberId);

        Member = Helper.GetMemberInformation(memberId);

        if (null != Member)
        {
          var message = context.MakeMessage(); message.Attachments = new List<Attachment>
                    {
                        Helper.GetMemberCard(Member).ToAttachment()
                    };
          message.AttachmentLayout = "carousel";

          await context.PostAsync(message);

          //new msg
          await context.PostAsync("Thank you. We are able to find you. Please let me know which 'Benefits & Coverage' you are looking for and know more.");
        }
        else
        {
          Member = null;
          await context.PostAsync("Sorry, we can't find you. Please enter valid one.");
        }
      }
      catch (Exception exception)
      {
        await context.PostAsync(exception.Message);
      }
      context.Wait(this.MessageReceived);
    }

    [LuisIntent("Member.Update")]
    public async Task NewMember(IDialogContext context, LuisResult luisResult)
    {
      string memberId = string.Empty;
      EntityRecommendation memberIdEntityRec = null;

      //await context.PostAsync("started");

      if (luisResult.TryFindEntity("MemberId", out memberIdEntityRec))
      {
        memberId = memberIdEntityRec.Entity;
      }

      //await context.PostAsync(memberId);

      // Check for member id
      if (string.IsNullOrEmpty(memberId))
      {
        context.Call(new AskMemberIdDialog(), OnNewMemberId);
      }
      else
      {
        Member = Helper.GetMemberInformation(memberId);

        if (null == Member)
        {
          await context.PostAsync("Member Id is **not valid**.");
        }
        else
        {
          var message = context.MakeMessage(); message.Attachments = new List<Attachment>
                    {
                        Helper.GetMemberCard(Member).ToAttachment()
                    };
          message.AttachmentLayout = "carousel";

          await context.PostAsync(message);
        }
      }
      context.Wait(this.MessageReceived);
    }

    [LuisIntent("Conversation.End")]
    public async Task Conversation_End(IDialogContext context, LuisResult luisResult)
    {
      PromptDialog.Confirm(context, onEndMessage, "Do you want to exit?");
    }

    private async Task onEndMessage(IDialogContext context, IAwaitable<bool> result)
    {
      bool isExit = await result;

      if (isExit)
      {
        //var message = context.MakeMessage();
        //message.Attachments = new List<Attachment>
        //            {
        //                new HeroCard(){
        //                    Title = $"Thank you! have a good day \U0001F44D",
        //                    Buttons = new List<CardAction>(){
        //                        new CardAction(){
        //                            Type = "imBack",
        //                            Title ="Let's Start",
        //                            Value="hello"
        //                        }
        //                        //new CardAction(){
        //                        //    Type = "postBack",
        //                        //    Title = "Sign Out",
        //                        //    Value="Sign Out",

        //                        //}

        //                    }
        //                }.ToAttachment()
        //            };
        //message.AttachmentLayout = "carousel";

        //await context.PostAsync(message);

        context.Done(true);

        Member = null;
        Benefit = null;
        CostType = null;
      }
      else
      {
        //await context.PostAsync($"*Search for new member by typing **member <member id>**");        

        await context.Forward(new WelcomeCardDialog(),
             AfterChildDialogIsDone,
             context.Activity.AsMessageActivity(),
             System.Threading.CancellationToken.None);

        context.Wait(this.MessageReceived);
      }

    }

    [LuisIntent("Member.Benefits")]
    public async Task Member_Benefits(IDialogContext context, LuisResult luisResult)
    {
      if (null == Member || null == Member.activePlan)
      {
        await context.PostAsync($"No member or plan found");
      }
      else
      {
        EntityRecommendation benefitRecommendation;


        if (luisResult.TryFindEntity("Benefit", out benefitRecommendation))
        {
          Benefit = benefitRecommendation.Entity;
        }

        if (string.IsNullOrEmpty(Benefit))
        {
          await context.PostAsync($"Sorry I did not understand service you are looking for, We have recorded your query. Can you please try with some other words ?");
        }
        else
        {
          EntityRecommendation costRecommendation;

          if (luisResult.TryFindEntity("CostType", out costRecommendation))
          {
            CostType = costRecommendation.Entity;
          }

          BenefitsInformation benefitsInformation = Helper.GetBenefitsInformation(Member.activePlan.PlanId, Benefit);

          if (benefitsInformation.BenefitsCovered)
          {
            await context.PostAsync($"Member is **covered** for {Benefit}{Environment.NewLine} *please find below benefits*");

            var bmessage = context.MakeMessage();
            bmessage.Attachments =
            Helper.GetBenefitsCard(benefitsInformation.Benefits);
            bmessage.AttachmentLayout = "carousel";
            await context.PostAsync(bmessage);

            if (string.IsNullOrEmpty(CostType))
            {
              PromptDialog.Choice(context,
                  CostSelected,
                  new List<string>() {
                        "CoPay",
                        "CoInsurance",
                        "Deductible"
                      },
                  "What do you want to know?", "What do you want to know?", 3);
            }
            else
            {
              Cost cost = Helper.GetCostDetails(Member.activePlan.EnrollId, CostType);
              var message = context.MakeMessage();
              message.Attachments = new List<Attachment>
                    {
                        Helper.GetCostCard(cost).ToAttachment()
                    };
              message.AttachmentLayout = "carousel";

              await context.PostAsync(message);
              CostType = string.Empty;

              context.Wait(this.MessageReceived);

            }
          }
          else
          {
            await context.PostAsync($"{Benefit} is not covered");
            CostType = string.Empty;

            context.Wait(this.MessageReceived);
          }
        }
      }

    }

    private async Task CostSelected(IDialogContext context, IAwaitable<string> result)
    {
      CostType = await result;

      Cost cost = Helper.GetCostDetails(Member.activePlan.EnrollId, CostType);
      var message = context.MakeMessage();
      message.Attachments = new List<Attachment>
                    {
                        Helper.GetCostCard(cost).ToAttachment()
                    };
      message.AttachmentLayout = "carousel";

      await context.PostAsync(message);

      CostType = string.Empty;

      context.Wait(MessageReceived);
    }

    [LuisIntent("SignOut")]
    public async Task SignOut(IDialogContext context, LuisResult luisResult)
    {
      await context.SignOutUserAsync(ConnectionName);
      await context.PostAsync($"*You have been signed out susccessfully*");
      context.Done<string>("signout");
    }

    private async Task AfterChildDialogIsDone(IDialogContext context, IAwaitable<object> result)
    {
      context.Done<object>(new object());
      //context.Wait(MessageReceived);
    }
  }
}