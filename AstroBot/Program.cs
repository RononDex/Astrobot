using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace AstroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = JObject.Parse(File.ReadAllText("appsettings.json"));
            var discordToken = File.ReadAllText(settings["DiscordTokenPath"].Value<string>());

            // Setup DI
            var serviceProvider = new ServiceCollection()
             .AddLogging()
             .AddScoped<NLog.Logger>()
             .AddSingleton<ApiWrapper>(new DiscordWrapper(discordToken))
             .AddSingleton<AwesomeChatBot.AwesomeChatBot>()
             .BuildServiceProvider();


        }
    }
}
