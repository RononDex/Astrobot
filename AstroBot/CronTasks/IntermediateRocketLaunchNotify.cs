using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AstroBot.CronTasks
{
    public class IntermediateRocketLaunchNotify : CronTask
    {
        private static readonly List<string> NotifiedLaunches = new List<string>();
        private static readonly List<string> NotifiedSpaceEvents = new List<string>();

        public override string Name => nameof(IntermediateRocketLaunchNotify);

        public override DateTime NextExecution => LastExecution.AddMinutes(1);

        public override async Task ExecuteAsync()
        {
            var filteredLaunches = Globals.UpcomingRocketLaunchesCache.Where(launch =>
                    launch.WindowStart > DateTime.UtcNow
                    && launch.WindowStart < DateTime.UtcNow.AddHours(1));

            var filteredSpaceEvents = Globals.UpcomingEventsCache.Where(spaceEvent =>
                    spaceEvent.EventTime > DateTime.UtcNow
                    && spaceEvent.EventTime < DateTime.UtcNow.AddHours(1));

            if (filteredLaunches.Any() || filteredSpaceEvents.Any())
            {
                var notifiedLaunchesNow = new List<string>();
                var notifiedEventsNow = new List<string>();

                foreach (var wrapper in Globals.BotFramework.ApiWrappers)
                {
                    var servers = await wrapper.GetAvailableServersAsync().ConfigureAwait(false);
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

                            var channel = await server
                                .ResolveChannelAsync(channelName)
                                .ConfigureAwait(false);

                            var roles = await server.GetAvailableUserRolesAsync().ConfigureAwait(false);
                            var roleForLaunches = roles.FirstOrDefault(serverRole => serverRole.Name == roleNameLaunches);
                            var roleForEvents = roles.FirstOrDefault(serverRole => serverRole.Name == roleNameEvents);
                            if (roleForLaunches != null && roleForEvents != null && channel != null)
                            {
                                foreach (var launch in filteredLaunches)
                                {
                                    if (NotifiedLaunches.Contains(launch.Id))
                                        continue;

                                    await channel
                                        .SendMessageAsync($"{roleForLaunches.GetMention()} Upcoming launch within the next hour!\r\n{wrapper.MessageFormatter.Bold(launch.Name)}\r\n{launch.VidURLs?.FirstOrDefault()?.Url}")
                                        .ConfigureAwait(false);

                                    if (!notifiedLaunchesNow.Contains(launch.Id))
                                    {
                                        notifiedLaunchesNow.Add(launch.Id);
                                    }
                                }

                                foreach (var spaceEvent in filteredSpaceEvents)
                                {
                                    if (NotifiedSpaceEvents.Contains(spaceEvent.Id))
                                    {
                                        continue;
                                    }

                                    await channel
                                        .SendMessageAsync($"{roleForEvents.GetMention()} Upcoming event within the next hour!\r\n{wrapper.MessageFormatter.Bold(spaceEvent.Name)}\r\n{spaceEvent.VideoUrl}")
                                        .ConfigureAwait(false);

                                    if (!notifiedEventsNow.Contains(spaceEvent.Id))
                                    {
                                        notifiedEventsNow.Add(spaceEvent.Id);
                                    }
                                }
                            }
                        }
                    }
                }

                NotifiedLaunches.AddRange(notifiedLaunchesNow);
                NotifiedSpaceEvents.AddRange(notifiedEventsNow);
            }

            await base.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
