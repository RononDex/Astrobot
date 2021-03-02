using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class ServerEvents
    {
        internal static Task ServerAvailableAsync(Server server)
        {
            return Task.Run(() =>
            {
                Config.DefaultConfigsHelper.SetupDefaultServerConfig(server);
                if (server.ServerName.Contains("Test"))
                {
                    //new CronTasks.UpcomingLaunches().Execute();
                }
            });
        }

        internal static async Task MessageDeletedAsync(ChatMessage deletedMessage)
        {
            var greetingsChannel = Globals.BotFramework.ConfigStore.GetConfigValue<string>("LogChannel", deletedMessage.Channel.ParentServer);
            if (greetingsChannel != null)
            {
                var channel = await deletedMessage.Channel.ParentServer.ResolveChannelAsync(greetingsChannel).ConfigureAwait(false);
                var message = $"A message from user __{deletedMessage.Author.Name}__ was **deleted** in channel #{deletedMessage.Channel.Name}:\r\n-------------------\r\n{deletedMessage.Content}";
                channel?.SendMessageAsync(new SendMessage(message, deletedMessage.Attachments));
            }
        }
    }
}