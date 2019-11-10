using AwesomeChatBot.ApiWrapper;

namespace AstroBot.Events
{
    public static class ServerEvents
    {
        public static void ServerAvailable(Server server)
        {
            Config.DefaultConfigsHelper.SetupDefaultServerConfig(server);
            if (server.ServerName.Contains("Test"))
            {
                CronTasks.UpcomingLaunches.Execute();
            }
        }
    }
}