using System;
using System.Threading.Tasks;

namespace AstroBot.CronTasks
{
    public abstract class CronTask
    {
        public abstract string Name
        {
            get;
        }

        public abstract DateTime NextExecution
        {
            get;
        }

        public virtual DateTime LastExecution
        {
            get
            {
                return Globals.BotFramework!.ConfigStore.GetConfigValue<DateTime>(key: $"{Name}_LastExecution", defaultValue: DateTime.MinValue);
            }
            private set
            {
                Globals.BotFramework!.ConfigStore.SetConfigValue(key: $"{Name}_LastExecution", value);
            }
        }

        public virtual Task ExecuteAsync()
        {
            LastExecution = DateTime.Now;
            return Task.CompletedTask;
        }
    }
}
