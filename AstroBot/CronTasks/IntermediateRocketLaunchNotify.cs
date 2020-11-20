using System;
using System.Collections.Generic;
using System.Linq;

namespace AstroBot.CronTasks
{
    public class IntermediateRocketLaunchNotify : CronTask
    {
        private static readonly List<string> NotifiedLaunches = new List<string>();
        private static readonly List<string> NotifiedSpaceEvents = new List<string>();

        public override string Name => nameof(IntermediateRocketLaunchNotify);

        public override DateTime NextExecution => LastExecution.AddMinutes(5);

        public override void Execute()
        {
            var filteredLaunches = Globals.UpcomingRocketLaunchesCache.Where(launch =>
                    launch.WindowStart > DateTime.UtcNow
                    && launch.WindowStart < DateTime.UtcNow.AddHours(1));

            var filteredSpaceEvents = Globals.UpcomingEventsCache.Where(spaceEvent =>
                    spaceEvent.EventTime > DateTime.UtcNow
                    && spaceEvent.EventTime < DateTime.UtcNow.AddHours(1));

            if (filteredLaunches.Any() || filteredSpaceEvents.Any())
            {
                foreach (var wrapper in Globals.BotFramework.ApiWrappers)
                {
                    var servers = wrapper.GetAvailableServers();
                    foreach (var server in servers)
                    {
                        if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("RocketLaunchesNewsEnabled", server))
                        {
                            var roleNameLaunches = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesIntermediateTagRole", server);
                            if (string.IsNullOrEmpty(roleNameLaunches))
                            {
                                continue;
                            }

                            var roleNameEvents = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesIntermediateTagRoleEvents", server);
                            if (string.IsNullOrEmpty(roleNameEvents))
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
                            var roleForLaunches = roles.FirstOrDefault(serverRole => serverRole.Name == roleNameLaunches);
                            var roleForEvents = roles.FirstOrDefault(serverRole => serverRole.Name == roleNameEvents);
                            if (roleForLaunches != null && roleNameEvents != null && channel != null)
                            {

                                foreach (var launch in filteredLaunches)
                                {
                                    if (NotifiedLaunches.Contains(launch.Id))
                                        continue;

                                    channel.SendMessageAsync($"{roleForLaunches.GetMention()} Upcoming launch within the next hour!\r\n{wrapper.MessageFormatter.Bold(launch.Name)}\r\n{launch.VidURLs?.FirstOrDefault()?.Url}");

                                    if (string.Equals(servers.Last().ServerID, server.ServerID, StringComparison.Ordinal))
                                    {
                                        NotifiedLaunches.Add(launch.Id);
                                    }
                                }

                                foreach (var spaceEvent in filteredSpaceEvents)
                                {
                                    if (NotifiedSpaceEvents.Contains(spaceEvent.Id))
                                        continue;

                                    channel.SendMessageAsync($"{roleForEvents.GetMention()} Upcoming event within the next hour!\r\n{wrapper.MessageFormatter.Bold(spaceEvent.Name)}\r\n{spaceEvent.VideoUrl}");

                                    if (string.Equals(servers.Last().ServerID, server.ServerID, StringComparison.Ordinal))
                                    {
                                        NotifiedSpaceEvents.Add(spaceEvent.Id);
                                    }
                                }
                            }
                        }
                    }

                }
            }

            base.Execute();
        }
    }
}
