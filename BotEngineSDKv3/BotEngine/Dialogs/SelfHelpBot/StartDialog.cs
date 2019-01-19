using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.SelfHelpBot.Dialogs
{
    [Serializable]
    public class StartDialog : IDialog<object>
    {
        //private static readonly string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];

        public StartDialog(string resourcePath)
        {
            _resourcesPath = resourcePath;
        }

        public string _resourcesPath;


        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Hey, this is the  Bot.");

            await Task.Delay(2000);

            context.Call(new RootDialog(_resourcesPath), OnDialogEnd);
        }

        private async Task OnDialogEnd(IDialogContext context, IAwaitable<object> result)
        {
            context.Call(new RootDialog(_resourcesPath), OnDialogEnd);
        }
    }
}