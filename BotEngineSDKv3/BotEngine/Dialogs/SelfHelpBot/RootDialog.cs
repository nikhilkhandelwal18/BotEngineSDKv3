using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using BotEngine.SelfHelpBot.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace BotEngine.SelfHelpBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        //private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

        public RootDialog(string resourcePath)
        {
            _resourcesPath = resourcePath;
        }

        public string _resourcesPath;

        private const string PasswordResetOption = "Password Reset";
        private const string UnlockAccountOption = "Unlock Account";
        private const string RebrandingExperienceOption = "Rebranding Experience";
        private const string GetEmployeeInformationOption = "Get Employee Information";
        private const string RegisterOktaOption = "Register with Okta";

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hello! I'll send you a message proactively to demonstrate how bots can initiate messages.");

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

        public class QnAMakerResult
        {
            [JsonProperty(PropertyName = "answers")]
            public List<Result> Answers { get; set; }
        }

        public class Result
        {
            [JsonProperty(PropertyName = "answer")]
            public string Answer { get; set; }

            [JsonProperty(PropertyName = "questions")]
            public List<string> Questions { get; set; }

            [JsonProperty(PropertyName = "score")]
            public double Score { get; set; }
        }

        private string GetAnswer(string query)
        {
            string responseString = string.Empty;

            try
            {
                var knowledgebaseId = Convert.ToString("bdadde1b-deee-4321-9a08-df7b8c801b82", CultureInfo.InvariantCulture);

                //Build the URI
                var builder = new UriBuilder(string.Format(Convert.ToString($"https://botqnamakerdemo.azurewebsites.net/qnamaker/knowledgebases/{knowledgebaseId}/generateAnswer", CultureInfo.InvariantCulture), knowledgebaseId));

                //Add the question as part of the body
                var postBody = string.Format("{{\"question\": \"{0}\"}}", query);

                //Send the POST request
                using (WebClient client = new WebClient())
                {
                    //Set the encoding to UTF8
                    client.Encoding = System.Text.Encoding.UTF8;

                    //Add the subscription key header
                    var qnamakerSubscriptionKey = Convert.ToString("63a32ec5-7201-4ac3-8fb6-823bf795302f", CultureInfo.InvariantCulture);
                    client.Headers.Add("Authorization", $"EndpointKey {qnamakerSubscriptionKey}");
                    client.Headers.Add("Content-Type", "application/json");
                    responseString = client.UploadString(builder.Uri, postBody);
                }
                QnAMakerResult result = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
                return result.Answers[0].Answer;
            }
            catch (Exception exception)
            {

            }
            return string.Empty;
        }

        private readonly List<string> rootOptions = new List<string>()
        {
            PasswordResetOption,
            UnlockAccountOption,
            RebrandingExperienceOption,
            GetEmployeeInformationOption,
            RegisterOktaOption
        };

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                var message = context.MakeMessage();
                message.AttachmentLayout = "carousel";

                switch (optionSelected)
                {
                    case PasswordResetOption:
                        message.Attachments = new List<Attachment>
                    {
                        SelfBotHelper.PasswordResetCard().ToAttachment()
                    };
                        await context.PostAsync(message);
                        context.Done(true);
                        break;

                    case UnlockAccountOption:
                        message.Attachments = new List<Attachment>
                    {
                        SelfBotHelper.UnlockAccountCard().ToAttachment()
                    };
                        await context.PostAsync(message);
                        context.Done(true);

                        break;

                    case RebrandingExperienceOption:

                        PromptDialog.Text(context,
                            onQuestionAsked,
                            $"Please type what issue you are facing with the Rebranding Experience.", $"Please type what issue you are facing with the Rebranding Experience.", 5);
                        break;

                    case GetEmployeeInformationOption:
                        message.Attachments = new List<Attachment>
                    {
                        SelfBotHelper.GetEmployeeInformationCard().ToAttachment()
                    };
                        await context.PostAsync(message);
                        context.Done(true);

                        break;

                    case RegisterOktaOption:
                        message.Attachments = new List<Attachment>
                    {
                        SelfBotHelper.RegisterWithOktaCard().ToAttachment()
                    };
                        await context.PostAsync(message);
                        context.Done(true);

                        break;
                }
            }

            catch (TooManyAttemptsException exception)
            {
                await context.PostAsync("Ooops! Too many attemps. Please try again!");
            }
        }

        private async Task onQuestionAsked(IDialogContext context, IAwaitable<string> result)
        {
            string question = await result;

            string answer = GetAnswer(question);

            if (answer.Equals("No good match found in KB."))
            {
                answer = string.Empty;
                answer = "Seems like I am unable to help you with your problem.\n Why don't you try rephrasing your question.\n If I am still unable to help you, why don't you try contacting : xyz";
            }

            await context.PostAsync(answer);

            PromptDialog.Confirm(context,
                onRebrandingConfirm,
                "Do you have more questions?");
        }

        private async Task onRebrandingConfirm(IDialogContext context, IAwaitable<bool> result)
        {
            bool rebrandingConfirm = await result;

            if (rebrandingConfirm)
            {
                PromptDialog.Text(context,
                               onQuestionAsked,
                               $"Please type what issue you are facing with the Rebranding Experience.", $"Please type what issue you are facing with the Rebranding Experience.", 3);
            }
            else
            {
                context.Done(true);
            }
        }
    }
}