using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotEngine.Scorables
{
    public class CancelScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask task;

        public CancelScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.Equals("cancel", StringComparison.InvariantCultureIgnoreCase) ||
                    message.Text.Equals("start over", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
                      message.Text.Equals("quit", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("done", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("start again", StringComparison.InvariantCultureIgnoreCase) ||
                         message.Text.Equals("restart", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("leave", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            this.task.Reset();
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}