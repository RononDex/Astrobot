using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace AstroBot.CronTasks
{
    public static class CronTaskManager
    {
        private static readonly Timer executeTimer = new Timer(
                new TimerCallback(ExecutePendingTasks),
                state: null,
                dueTime: 60 * 1000,
                period: 60 * 1000);

        private static readonly List<CronTask> RegisteredTasks = new List<CronTask>();

        public static void Register(CronTask task)
        {
            RegisteredTasks.Add(task);
        }

        public static void ExecutePendingTasks(object state)
        {
            foreach (var registeredTask in RegisteredTasks)
            {
                if (registeredTask.NextExecution < DateTime.Now)
                {
                    var logger = Globals.LoggerFactory.CreateLogger(nameof(CronTaskManager));
                    logger.Log(LogLevel.Information, $"Executing task {registeredTask.Name}");
                    registeredTask.ExecuteAsync().GetAwaiter().GetResult();
                }
            }
        }
    }
}

