using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class UserServerEvents
    {
        public static async Task UserJoinedServerAsync(User user, Server server)
        {
            var greetingsChannel = Globals.BotFramework.ConfigStore.GetConfigValue<string>("GreetNewUsersChannel", server);
            if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("GreetNewUsers", server)
                && greetingsChannel != null)
            {
                var channel = await server.ResolveChannelAsync(greetingsChannel).ConfigureAwait(false);
                var message = Globals.BotFramework.ConfigStore.GetConfigValue<string>("GreetNewUsersMessage", server);
                if (channel != null)
                {
                    await channel.SendMessageAsync($"{message.Replace("@UserMention", user.GetMention())}").ConfigureAwait(false);
                }
            }
        }
    }
}