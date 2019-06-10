using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class UserServerEvents
    {
        public static void UserJoinedServer(User user, Server server)
        {
            var greetingsChannel = Globals.BotFramework.ConfigStore.GetConfigValue<string>("GreetNewUsersChannel", server);
            if (Globals.BotFramework.ConfigStore.GetConfigValue<bool>("GreetNewUsers", server)
                && greetingsChannel != null)
            {
                var channel = server.ResolveChannelAsync(greetingsChannel).Result;
                var message = Globals.BotFramework.ConfigStore.GetConfigValue<string>("GreetNewUsersMessage", server);
                _ = channel?.SendMessageAsync($"{message.Replace("@UserMention", user.GetMention())}");
            }
        }
    }
}