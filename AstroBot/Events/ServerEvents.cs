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
    }
}