using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BotEngine.Dialogs.ExceptionHandler;
using BotEngine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;

namespace BotEngine.Dialogs
{
  public static class Helper
  {
    private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

    public static GetTokenDialog CreateGetTokenDialog(string connectionName)
    {
      //return new GetTokenDialog(connectionName,
      //    $"Please sign in to proceed.",
      //    "Connect", 2,
      //    "Something went wrong, let's try again.");

      //return new GetTokenDialog(connectionName,
      //          $"I can help you with more information once you login",
      //          "Connect", 2,
      //          "Something went wrong, let's try again.");

      return new GetTokenDialog(connectionName,
        $"I can help you with more information once you login",
        "Connect", 2,
        "Something went wrong, let's try again.");

    }

    public static GetTokenDialog CreateGetTokenDialog()
    {
      return new GetTokenDialog(
          ConnectionName,
          $"I can help you with more information once you login",
          "Sign In");
    }

    public static ThumbnailCard GetMemberCard(Member member)
    {
      var thumbnailCard = new ThumbnailCard()
      {
        Title = $"{member.fullname}",
        Subtitle = $"Age - {member.age} Years ({member.gender})",
        Text = $"Active Plan - {member.activePlan.PlanDescription}",
        Images = null
      };

      return thumbnailCard;
    }

    public static Member GetMemberInformation(string memberId)
    {
      Member member = null;

      try
      {
        JObject memberJSON = SendApiRequest($"GetMemberDetails/{memberId}", null);
        member = JsonConvert.DeserializeObject<Member>(memberJSON.ToString());

        //member = new Member()
        //{
        //    Id = memberId,
        //    fullname = "Akshay Deshmukh",
        //    age = 30,
        //    gender = "male",
        //    activePlan = new Plan()
        //    {
        //        PlanId = "4567",
        //        PlanDescription = "opt 4"
        //    }
        //};
      }
      catch (Exception)
      {
        member = null;
      }
      return member;
    }

    public static JObject SendApiRequest(string apiEndPoint, string data = null)
    {
      //string emailAddress = string.Empty;
      //string accessToken = string.Empty;

      JObject jobjResponse = null;

      try
      {
        //emailAddress = userContext.emailId;
        //accessToken = userContext.accessToken;

        using (var client = new HttpClient())
        {


          client.BaseAddress = new Uri(Convert.ToString(ConfigurationManager.AppSettings["ApiUrl"]));
          client.DefaultRequestHeaders.Accept.Clear();
          client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

          //client.DefaultRequestHeaders.Add("X-Adal-User", emailAddress);
          //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

          HttpResponseMessage httpResponseMessage = null;

          if (string.IsNullOrEmpty(data))
          {
            httpResponseMessage = client.GetAsync(apiEndPoint).Result;
          }
          else
          {
            var stringContent = new StringContent(data, System.Text.Encoding.UTF8, "application/json");

            httpResponseMessage = client.PostAsync(apiEndPoint, stringContent).Result;
          }


          if (httpResponseMessage.IsSuccessStatusCode)
          {
            string responseJSON = httpResponseMessage.Content.ReadAsStringAsync().Result;
            jobjResponse = JObject.Parse(responseJSON);
          }
        }
      }
      catch (Exception exception)
      {
        jobjResponse = null;
      }
      return jobjResponse;
    }

    public static Cost GetCostDetails(string enrollId, string costType)
    {
      Cost cost = null;

      try
      {
        JObject costJSON = SendApiRequest($"GetMemberCopays/{enrollId}", null);
        cost = JsonConvert.DeserializeObject<Cost>(costJSON.ToString());

        //cost = new Cost()
        //{
        //    Type = "Copay",
        //    Value = new List<string>()
        //    {
        //        "in network = $3",
        //        "out of network  = $6"
        //    }
        //};
      }
      catch (Exception exception)
      {
        cost = null;
      }
      return cost;
    }


    public static BenefitsInformation GetBenefitsInformation(string planId, string keyword)
    {
      BenefitsInformation benefitsInformation = null;

      try
      {
        JObject memberJSON = SendApiRequest($"GetAnswerToUserQuery?id={planId}&keywords={keyword}", null);
        benefitsInformation = JsonConvert.DeserializeObject<BenefitsInformation>(memberJSON.ToString());
      }
      catch (Exception exception)
      {

      }
      return benefitsInformation;
    }

    public static List<Attachment> GetBenefitsCard(List<Benefit> benefits)
    {
      List<Attachment> benefitsCards = null;

      if (null != benefits || 0 < benefits.Count)
      {
        benefitsCards = new List<Attachment>();

        foreach (Benefit benefit in benefits)
        {
          benefitsCards.Add(new HeroCard()
          {
            Title = benefit.Id,
            Text = benefit.BenefitDescription
          }.ToAttachment());
        }
      }
      return benefitsCards;
    }

    public static HeroCard GetCostCard(Cost cost)
    {
      HeroCard receiptCard = null;

      try
      {
        receiptCard = new HeroCard()
        {
          Title = $"{cost.CostType} will be",
          Text = string.Join($"{Environment.NewLine}", cost.Values.Select(x => x))
        };

      }
      catch (Exception exception)
      {
        receiptCard = null;
      }
      return receiptCard;
    }
  }
}