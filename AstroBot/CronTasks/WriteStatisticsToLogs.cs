using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using Microsoft.Extensions.Logging;

namespace AstroBot.CronTasks
{
    public class WriteStatisticsToLog : CronTask
    {
        public override string Name => "WriteStatisticsToLog";

        public override DateTime NextExecution => LastExecution.AddDays(1);

        public override async Task ExecuteAsync()
        {
            var availableServers = new List<Server>();
            foreach (var apiWrapper in Globals.BotFramework.ApiWrappers)
            {
                availableServers.AddRange(await apiWrapper.GetAvailableServersAsync());
            }

            var numberOfUsersByServer = new Dictionary<string, int>();
            foreach (var server in availableServers)
            {
                var numberOfUsers = await server.GetNumberOfUsersAsync();

                numberOfUsersByServer.Add(server.ServerID, numberOfUsers);
            }

            var message = $"";
            message += $"--------------------------------------------------\r\n";
            message += $" Bot Statistics\r\n";
            message += $"--------------------------------------------------\r\n";
            message += $"\r\n";
            message += $"Total servers available:\t{availableServers.Count}\r\n";
            message += $"Total users reachable:\t{numberOfUsersByServer.Select(a => a.Value).Sum()}\r\n";
            message += $"Server list:\r\n";

            foreach (var server in availableServers)
            {
                message += $"    Server name: {server.ServerName}\r\n";
                message += $"        Server Id: {server.ServerID}\r\n";
                message += $"        Users on Server: {numberOfUsersByServer[server.ServerID]}\r\n";
            }

            var logger = Globals.LoggerFactory.CreateLogger(nameof(WriteStatisticsToLog));
            foreach (var line in message.Split("\r\n"))
            {
                logger.Log(LogLevel.Information, line);
            }
        }
    }
}
