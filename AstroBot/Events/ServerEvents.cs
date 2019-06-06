using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public class ServerEvents
    {
        public static void ServerAvailable(Server server)
        {
            Config.DefaultConfigsHelper.SetupDefaultServerConfig(server);
        }
    }
}