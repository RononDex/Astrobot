using System.Linq;
using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class UserServerEvents
    {
        public static void UserJoinedServer(User user, Server server)
        {
            var channels = server.GetAllChannelsAsync().Result;
            channels.First().SendMessageAsync($"Welcome {user.Name}");
        }
    }
}