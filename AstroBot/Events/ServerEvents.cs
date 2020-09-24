using System.Threading;
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
                //new CronTasks.UpcomingLaunches().Execute();
            }
        }
    }
}