using System;
using System.Linq;
using System.Threading.Tasks;
using AstroBot.LaunchLibrary;
using AstroBot.Utilities.Extensions;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.CronTasks
{
    public class UpcomingLaunches : CronTask
    {
        public override string Name => nameof(UpcomingLaunches);

        public override DateTime NextExecution => LastExecution.Date.AddDays(1);

        public override async Task ExecuteAsync()
        {
            var filteredLaunches = Globals.UpcomingRocketLaunchesCache
                .Where(launch =>
                    launch.WindowStart > DateTime.UtcNow
                    && launch.WindowStart < DateTime.UtcNow.Date.AddDays(4))
                .OrderBy(launch => launch.WindowStart);

            var filteredEvents = Globals.UpcomingEventsCache
                .Where(spaceEvent =>
                    spaceEvent.EventTime > DateTime.UtcNow
                    && spaceEvent.EventTime < DateTime.UtcNow.Date.AddDays(4))
                .OrderBy(spaceEvent => spaceEvent.EventTime);

            foreach (var wrapper in Globals.BotFramework.ApiWrappers)
            {
                foreach (var server in await wrapper.GetAvailableServersAsync().ConfigureAwait(false))
                {
                    if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("RocketLaunchesNewsEnabled", server))
                    {
                        var channelName = Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesNewsChannel", server);

                        if (string.IsNullOrEmpty(channelName))
                        {
                            continue;
                        }

                        var channel = await server
                            .ResolveChannelAsync(channelName)
                            .ConfigureAwait(false);

                        if (channel != null)
                        {
                            var sortedMessageList = filteredLaunches.Select(launch => launch as object).ToList();
                            sortedMessageList.AddRange(filteredEvents);
                            sortedMessageList = sortedMessageList
                                .OrderBy(message => message is Event
                                    ? (message as Event)!.EventTime
                                    : (message as Launch)!.WindowStart)
                                .ToList();

                            foreach (var message in sortedMessageList)
                            {
                                if (message is Launch launch)
                                {
                                    var launchMessage = CreateLaunchMessage(launch);

                                    await channel.SendMessageAsync(launchMessage).ConfigureAwait(false);
                                    await Task.Delay(100).ConfigureAwait(false); // Give discord api some time to post the message (it appears to be kinda slow)
                                }

                                if (message is Event spaceEvent)
                                {
                                    var spaceEventMessage = CreateSpaceEventEventMessage(spaceEvent);

                                    await channel.SendMessageAsync(spaceEventMessage).ConfigureAwait(false);
                                    await Task.Delay(100).ConfigureAwait(false); // Give discord api some time to post the message (it appears to be kinda slow)
                                }
                            }
                        }
                    }
                }
            }

            await base.ExecuteAsync().ConfigureAwait(false);
        }

        private static EmbeddedMessage CreateSpaceEventEventMessage(Event spaceEvent)
        {
            var eventMessage = new EmbeddedMessage
            {
                Title = $"Upcoming event - {spaceEvent.Name}",
                ThumbnailUrl = spaceEvent.FeatureImgUrl,
            };

            eventMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Type of event",
                Content = spaceEvent.Type?.Name,
            });

            eventMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Time of event UTC",
                Content = spaceEvent.EventTime.ToString(),
            });

            eventMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Location",
                Content = spaceEvent.Location,
            });

            eventMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = false,
                Name = "Description",
                Content = spaceEvent.Description.ShortenTo(1024),
            });

            return eventMessage;
        }

        private static EmbeddedMessage CreateLaunchMessage(Launch launch)
        {
            var launchMessage = new EmbeddedMessage
            {
                Title = "Upcoming launch - " + string.Join(", ", launch.Name),
                ThumbnailUrl = launch.Image
            };

            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Agency",
                Content = launch.LaunchServiceProvider?.Name ?? string.Empty
            });

            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Rocket",
                Content = launch.Rocket?.Configuration?.FullName ?? string.Empty
            });

            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Launch Window UTC",
                Content = $"{launch.WindowStart} to \r\n{launch.WindowEnd}"
            });

            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = true,
                Name = "Launch pad",
                Content = $"{launch.Pad?.Location?.Name} - {launch.Pad?.Name}"
            });

            var mission = launch.Mission;
            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = false,
                Name = $"Mission",
                Content = mission?.Name
            });

            launchMessage.Fields.Add(new EmbeddedMessageField
            {
                Inline = false,
                Name = $"Mission Description",
                Content = mission?.Description?.ShortenTo(1024) ?? string.Empty
            });
            return launchMessage;
        }
    }
}
