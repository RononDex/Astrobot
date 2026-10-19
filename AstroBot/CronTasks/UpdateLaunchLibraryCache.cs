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
            // Both requests run concurrently
            var launchesTask = LaunchLibrary.LaunchLibraryClient.GetUpcomingLaunchesAsync(10);
            var eventsTask = LaunchLibrary.LaunchLibraryClient.GetUpcomingEventsAsync(10);
            var newCacheLaunches = await launchesTask.ConfigureAwait(false);
            var newCacheEvents = await eventsTask.ConfigureAwait(false);

            Globals.UpcomingRocketLaunchesCache = newCacheLaunches ?? Globals.UpcomingRocketLaunchesCache;
            Globals.UpcomingEventsCache = newCacheEvents ?? Globals.UpcomingEventsCache;

            await base.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
