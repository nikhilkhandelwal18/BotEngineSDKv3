using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace BotEngine.SelfHelpBot.Services
{
    public static class SelfBotHelper
    {
        public static HeroCard GetEmployeeInformationCard()

        {
            return new HeroCard
            {
                Title = "For Employee Info",
                Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.OpenUrl, title: "Click here", value: "https://myBotEngine.sharepoint.com/sites/spark/"),
                     // new CardAction(ActionTypes.OpenUrl, title: "Guide on how to reset your passowrd", value: "https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true", displayText:"Please refer to section 5 of the document."),
                    },
            };

        }

        public static HeroCard UserGreetingCard(bool isNew, string _resourcePath)
        {
            var card = new HeroCard
            {
                Title = isNew ? "Hey, this is the  IT Bot. How can I help you today?" : "What would you like to do next?",
                Images = new List<CardImage> { new CardImage($"{_resourcePath}/Images/AllWays.jpg") },
                Buttons = new List<CardAction>()
                                {
                                    new CardAction(ActionTypes.ImBack, title: "Password Reset", value: "Password Reset"),
                                    new CardAction(ActionTypes.ImBack, title: "Unlock Account", value: "Unlock Account"),
                                    new CardAction(ActionTypes.ImBack, title: "Rebranding Experience", value: "Rebranding"),
                                    new CardAction(ActionTypes.ImBack, title: "Get Employee Information", value: "Get Employee Information"),
                                    new CardAction(ActionTypes.ImBack, title: "Register with Okta", value: "Register Okta"),
                                    new CardAction(ActionTypes.ImBack, title: "Exit", value: "Exit"),
                                },
            };

            return card;
        }

        public static HeroCard PasswordResetCard()
        {
            return new HeroCard
            {
                Title = "Reset Password",
                Buttons = new List<CardAction>()
                                {
                                    new CardAction(ActionTypes.OpenUrl, title: "Reset password", value: "https://BotEngine.okta.com/signin/forgot-password"),
                                    new CardAction(ActionTypes.OpenUrl, title: "Guide", value: "https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true", displayText:"Please refer to section 5 of the document."),
                                },
            };
        }

        public static HeroCard RegisterWithOktaCard()
        {
            return new HeroCard
            {
                Title = "Register with Okta",
                Buttons = new List<CardAction>()
                                {
                                    new CardAction(ActionTypes.OpenUrl, title: "Click here", value: "https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true"),
                                },
            };

        }

        public static HeroCard UnlockAccountCard()
        {
            return new HeroCard
            {
                Title = "Unlock Account",
                Buttons = new List<CardAction>()
                                {
                                    new CardAction(ActionTypes.OpenUrl, title: "Unlock Account", value: "https://BotEngine.okta.com/signin/unlock"),
                                    new CardAction(ActionTypes.OpenUrl, title: "Guide", value: "https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true", displayText:"Please refer to section 5 of the document."),
                                },
            };

        }

        public static string UnlockAccountHyperLink()
        {
            string unlockAccount = string.Empty;

            unlockAccount = $"Click [here](https://BotEngine.okta.com/signin/unlock) to unlock your account. \n Here's a [Guide](https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true) to help you with the process. \n\n Remember this will only work if you have previously Registered with Okta. If you have not, please contact **email id**.";
            return unlockAccount;
        }

        public static string PasswordResetHyperLink()
        {
            string passwordReset = string.Empty;

            passwordReset = $"Click [here](https://BotEngine.okta.com/signin/forgot-password) to Reset your Password. \n Let's hope you don’t forget it anytime soon ! \n Here's a [Guide](https://myBotEngine.sharepoint.com/:w:/r/sites/Spark/ITSelfHelp/_layouts/15/Doc.aspx?sourcedoc=%7B00E5F6AE-4409-4A63-BCDF-3C30D15B4F2C%7D&file=OKTA%20Enrollment%20User%20Guide.docx&action=default&mobileredirect=true) to help you with the process. \n\n Remember this will only work if you have previously Registered with Okta. If you have not, please contact **email id**.";
            return passwordReset;
        }
    }
}