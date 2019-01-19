using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotEngine.Dialogs
{
  [Serializable]
  public class GreetingDialog : IDialog<object>
  {
    public async Task StartAsync(IDialogContext context)
    {
      await context.PostAsync("Hi");
      await Task.CompletedTask;
    }


  }
}