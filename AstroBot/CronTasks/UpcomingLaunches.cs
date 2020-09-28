using System;
using System.Linq;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.CronTasks
{
    public class UpcomingLaunches : CronTask
    {
        public override string Name => nameof(UpcomingLaunches);

        public override DateTime NextExecution => LastExecution.Date.AddDays(1);

        public override void Execute()
        {
            var filteredLaunches = Globals.UpcomingRocketLaunchesCache
                .Where(launch =>
                    launch.WindowStart > DateTime.UtcNow
                    && launch.WindowStart < DateTime.UtcNow.Date.AddDays(4))
                .OrderBy(launch => launch.WindowStart);

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
                            foreach (var launch in filteredLaunches)
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
                                    Content = launch.Rocket?.Configuration.FullName ?? string.Empty
                                });

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = true,
                                    Name = "Launch Window",
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
                                    Content = mission.Name
                                });

                                launchMessage.Fields.Add(new EmbeddedMessageField
                                {
                                    Inline = false,
                                    Name = $"Mission Description",
                                    Content = mission.Description ?? string.Empty
                                });

                                channel.SendMessageAsync(launchMessage).GetAwaiter().GetResult();
                            }
                        }
                    }
                }
            }

            base.Execute();
        }
    }
}
