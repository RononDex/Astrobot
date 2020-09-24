using System;
using System.Linq;

namespace AstroBot.CronTasks
{
    public class UpdateLaunchLibraryCache : CronTask
    {
        public override string Name => "UpdateLaunchCache";

        public override DateTime NextExecution => LastExecution.AddHours(1);

        public override void Execute()
        {
            var newCache = LaunchLibrary.LaunchLibraryClient.GetUpcomingLaunches(20);
            Globals.UpcomingRocketLaunchesCache = newCache.Select(x => x).ToList();

            base.Execute();
        }
    }
}
