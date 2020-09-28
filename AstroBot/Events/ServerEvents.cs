using System;
using System.Threading;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class ServerEvents
    {
        internal static void ServerAvailable(Server server)
        {
            Config.DefaultConfigsHelper.SetupDefaultServerConfig(server);
            if (server.ServerName.Contains("Test"))
            {
                //new CronTasks.UpcomingLaunches().Execute();
            }
        }

        internal static void MessageDeleted(ChatMessage deletedMessage)
        {
            var greetingsChannel = Globals.BotFramework.ConfigStore.GetConfigValue<string>("LogChannel", deletedMessage.Channel.ParentServer);
            if (greetingsChannel != null)
            {
                var channel = deletedMessage.Channel.ParentServer.ResolveChannelAsync(greetingsChannel).Result;
                var message = $"A message from user __{deletedMessage.Author.Name}__ was **deleted** following message in channel #{deletedMessage.Channel.Name}:\r\n```{deletedMessage.Content}```";
                channel?.SendMessageAsync(message);
            }
        }
    }
}