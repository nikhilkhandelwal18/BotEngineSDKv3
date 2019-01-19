using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;

namespace BotEngine.Dialogs
{
    [Serializable]
    public class AskMemberIdDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Text(
                context,// Current Dialog Context                
                OnmemberIdEntered,// Callback after option selection                
                "Could you please enter **member id**?",// Prompt text                 
                "Not a expected answer",// Invalid input message                
                3);
        }

        private async Task OnmemberIdEntered(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string memberIdEntered = await result;

                // Check member id

                context.Done(memberIdEntered);
            }
            catch (Exception exception)
            {
                await context.PostAsync("Ooops! something went wrong!");
                context.Done(string.Empty);
            }
        }
    }
}