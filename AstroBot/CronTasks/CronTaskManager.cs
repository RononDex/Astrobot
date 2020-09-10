using System;
using System.Collections.Generic;
using System.Timers;

namespace AstroBot.CronTasks
{
    public static class CronTaskManager
    {
        private static readonly Timer executeTimer = new Timer(60 * 1000);

        private static readonly List<CronTask> RegisteredTasks = new List<CronTask>();

        public static void Init()
        {
            executeTimer.Elapsed += ExecutePendingTasks;
            executeTimer.AutoReset = true;
            executeTimer.Enabled = true;
            executeTimer.Start();
        }

        public static void Register(CronTask task)
        {
            RegisteredTasks.Add(task);
        }

        public static void ExecutePendingTasks(object sender, ElapsedEventArgs e)
        {
            foreach (var registeredTask in RegisteredTasks)
            {
                if (registeredTask.NextExecution < DateTimeOffset.Now)
                {
                    registeredTask.Execute();
                }
            }
        }
    }
}

