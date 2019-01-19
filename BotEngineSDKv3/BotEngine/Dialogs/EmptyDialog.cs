using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotEngine.Dialogs
{
  public class EmptyDialog : IDialog<object>
  {
    public async Task StartAsync(IDialogContext context)
    {
       context.Done(true);
    }
  }
}