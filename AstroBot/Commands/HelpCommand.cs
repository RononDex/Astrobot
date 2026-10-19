using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AwesomeChatBot.Config;
using System.Reflection;

namespace AstroBot.Commands
{
    public class HelpCommand : AwesomeChatBot.Commands.Command, ISlashCommand, ICommandDescription
    {

        public override string Name => "Help";

        public string Description => "Displays this help";

        public string SlashName => "help";

        public string SlashDescription => "Lists all commands of AstroBot";

        public IReadOnlyList<SlashCommandOption> Options => Array.Empty<SlashCommandOption>();

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var ass = Assembly.GetEntryAssembly();
            var formatter = Globals.ApiWrapper.MessageFormatter;

            await invocation.RespondAsync("Following commands are available in this server:").ConfigureAwait(false);
            var configStore = Globals.BotFramework!.ConfigStore;
            var context = new IConfigurationDependency?[] { invocation.Channel?.ParentServer };
            var message = "";

            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(ICommandDescription)))
                {
                    var command = ass.CreateInstance(ti.FullName) as ICommandDescription;

                    // Only list slash commands
                    if (command is AwesomeChatBot.Commands.Command && command is ISlashCommand slashCommand)
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
                        var curMessage = $"{formatter.Bold(command.Name)}: {command.Description}\r\nUsage:\r\n";
                        var builder = new System.Text.StringBuilder();
                        builder.Append('/').Append(slashCommand.SlashName);
                        foreach (var option in slashCommand.Options)
                        {
                            builder.Append(' ').Append(option.Name).Append(option.Required ? ":<...>" : ":[...]");
                        }
                        builder.Append("\r\n");

                        curMessage += formatter.Quote(builder.ToString());

                        if (message.Length + curMessage.Length > 2000)
                        {
                            await invocation.RespondAsync(message).ConfigureAwait(false);
                            message = string.Empty;
                        }

                        message += curMessage;
                    }
                }
            }

            await invocation.RespondAsync(message).ConfigureAwait(false);
        }
    }
}
