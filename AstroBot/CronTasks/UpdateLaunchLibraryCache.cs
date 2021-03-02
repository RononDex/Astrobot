using System;
using System.Threading.Tasks;

namespace AstroBot.CronTasks
{
    public class UpdateLaunchLibraryCache : CronTask
    {
        public override string Name => "UpdateLaunchCache";

        public override DateTime NextExecution => LastExecution.AddHours(1).AddMinutes(30);

        public override async Task ExecuteAsync()
        {
            var newCacheLaunches = LaunchLibrary.LaunchLibraryClient.GetUpcomingLaunches(10);
            var newCacheEvents = LaunchLibrary.LaunchLibraryClient.GetUpcomingEvents(10);

            Globals.UpcomingRocketLaunchesCache = newCacheLaunches ?? Globals.UpcomingRocketLaunchesCache;
            Globals.UpcomingEventsCache = newCacheEvents ?? Globals.UpcomingEventsCache;

            await base.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
