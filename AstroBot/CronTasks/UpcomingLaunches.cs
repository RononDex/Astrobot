using System;
using System.Globalization;
using System.Linq;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.CronTasks
{
    public static class UpcomingLaunches
    {
        public static void Execute()
        {
            var upcomingLaunces = LaunchLibrary.LaunchLibraryClient.GetUpcomingLaunches(days: 3)
                .Where(launch => DateTime.ParseExact(
                    launch.Isostart,
                    "yyyyMMddTHHmmssZ",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal) > DateTime.UtcNow);

            foreach (var wrapper in Globals.BotFramework.ApiWrappers)
            {
                foreach (var server in wrapper.GetAvailableServers())
                {
                    if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("RocketLaunchesNewsEnabled", server))
                    {
                        var channel = server
                            .ResolveChannelAsync(Globals.BotFramework.ConfigStore.GetConfigValue<string>("RocketLaunchesNewsChannel", server))
                            .GetAwaiter()
                            .GetResult();

                        if (channel != null)
                        {
                            foreach (var launch in upcomingLaunces)
                            {
                                var launchMessage = new EmbeddedMessage
                                {
                                    Title = "Upcoming launch - " + string.Join(", ", launch.Missions.Select(mission => mission.Name)),
                                    ThumbnailUrl = launch.Rocket?.ImageUrl?.ToString()
                                };

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = true,
                                    Name = "Agency",
                                    Content = launch.Lsp?.Name ?? string.Empty
                                });

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = true,
                                    Name = "Rocket",
                                    Content = launch.Rocket?.Name ?? string.Empty
                                });

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = true,
                                    Name = "Launch Window",
                                    Content = $"{launch.Windowstart} to \r\n{launch.Windowend}"
                                });

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = true,
                                    Name = "Launch pad",
                                    Content = $"{launch.Location?.Name} - {launch.Location?.Pads.FirstOrDefault()?.Name}"
                                });

                                var missionNr = 1;
                                foreach (var mission in launch.Missions)
                                {
                                    launchMessage.Fields.Add(new EmbeddedMessageField
                                    {
                                        Inline = false,
                                        Name = $"Mission {missionNr}",
                                        Content = mission.Name
                                    });

                                    launchMessage.Fields.Add(new EmbeddedMessageField
                                    {
                                        Inline = false,
                                        Name = $"Mission {missionNr} Description",
                                        Content = mission.Description ?? string.Empty
                                    });

                                    missionNr++;
                                }

                                channel.SendMessageAsync(launchMessage);
                            }
                        }
                    }
                }
            }
        }

        public static void Register()
        {
            Globals.CronDaemon.Add("0 0 * * *", Execute);
        }
    }
}