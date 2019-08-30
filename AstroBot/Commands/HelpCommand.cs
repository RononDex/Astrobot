using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AwesomeChatBot.Config;
using System.Reflection;

namespace AstroBot.Commands
{
    public class HelpCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {

        public override string Name => "Help";

        public string Description => "Displays this help";

        public string[] ExampleCalls => new[] { "Help" };

        public List<string> Regex => new List<string> { "help" };

        public async Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            var ass = Assembly.GetEntryAssembly();

            await receivedMessage.Channel.SendMessageAsync("Following commands are available in this server:");
            var configStore = receivedMessage.ApiWrapper.ConfigStore;
            var context = new IConfigurationDependency[] { receivedMessage.Channel.ParentServer };

            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(ICommandDescription)))
                {
                    var command = ass.CreateInstance(ti.FullName) as ICommandDescription;

                    // Only list commands
                    if (command is AwesomeChatBot.Commands.Command)
                    {
                        // Ignore disabled commands
                        if (!configStore.IsCommandActive(
                            command as AwesomeChatBot.Commands.Command,
                            enabledByDefault: Globals.AwesomeChatBotSettings.CommandsEnabledByDefault,
                            dependencies: context))
                        {
                            continue;
                        }

                        // Write help output for the command
                        var formatter = receivedMessage.ApiWrapper.MessageFormatter;
                        var message = $"{formatter.Bold(command.Name)}: {command.Description}\r\nExample(s):\r\n";
                        var examples = string.Empty;
                        var builder = new System.Text.StringBuilder();
                        builder.Append(examples);
                        foreach (var exampleCall in command.ExampleCalls)
                        {
                            builder.Append("@AstroBot ").Append(exampleCall).Append("\r\n");
                        }
                        examples = builder.ToString();

                        message += formatter.Quote(examples);

                        await receivedMessage.Channel.SendMessageAsync(new SendMessage(message));
                    }
                }
            }

            return true;
        }
    }
}