using System;
using System.Collections.Generic;
using System.Linq;

namespace AstroBot.CronTasks
{
    public class IntermediateRocketLaunchNotify : CronTask
    {
        private static readonly List<string> NotifiedLaunches = new List<string>();

        public override string Name => nameof(IntermediateRocketLaunchNotify);

        public override DateTime NextExecution => LastExecution.AddMinutes(5);

        public override void Execute()
        {
            var filteredLaunches = Globals.UpcomingRocketLaunchesCache.Where(launch =>
                    launch.WindowStart > DateTime.Now
                    && launch.WindowStart < DateTime.UtcNow.AddHours(1));

            if (filteredLaunches.Any())
            {
                foreach (var launch in filteredLaunches)
                {
                    if (NotifiedLaunches.Contains(launch.Id))
                        continue;

                    foreach (var wrapper in Globals.BotFramework.ApiWrappers)
                    {
                        foreach (var server in wrapper.GetAvailableServers())
                        {
                            if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("RocketLaunchesNewsEnabled", server))
                            {
                                var roleName = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesIntermediateTagRole", server);
                                if (string.IsNullOrEmpty(roleName))
                                {
                                    continue;
                                }

                                var channelName = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesNewsChannel", server);
                                if (string.IsNullOrEmpty(channelName))
                                {
                                    continue;
                                }

                                var channel = server
                                    .ResolveChannelAsync(channelName)
                                    .GetAwaiter()
                                    .GetResult();

                                var roles = server.GetAvailableUserRolesAsync().GetAwaiter().GetResult();
                                var roleObj = roles.FirstOrDefault(serverRole => serverRole.Name == roleName);
                                if (roleObj != null && channel != null)
                                {
                                    channel.SendMessageAsync($"{roleObj.GetMention()} Upcoming launch within the next hour!\r\n{wrapper.MessageFormatter.Bold(launch.Name)}\r\n{launch.VidURLs?.FirstOrDefault()?.Url}");
                                }
                            }
                        }
                    }

                    NotifiedLaunches.Add(launch.Id);
                }
            }

            base.Execute();
        }
    }
}
