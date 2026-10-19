using System.Linq;
using AwesomeChatBot.Commands.Handlers;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class SlashCommandTests
    {
        [TestMethod]
        public void TestSlashCommandsPassDiscordValidation()
        {
            var slashCommandTypes = typeof(Commands.HelpCommand).Assembly.GetTypes()
                .Where(t => typeof(ISlashCommand).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            Assert.IsNotEmpty(slashCommandTypes);

            // Discord rejects the whole bulk registration if a single command is invalid, Build() runs the same checks
            foreach (var command in slashCommandTypes.Select(t => (ISlashCommand)System.Activator.CreateInstance(t)!))
            {
                var builder = new SlashCommandBuilder().WithName(command.SlashName).WithDescription(command.SlashDescription);
                foreach (var option in command.Options)
                {
                    var optionBuilder = new SlashCommandOptionBuilder()
                        .WithName(option.Name)
                        .WithDescription(option.Description)
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(option.Required);
                    foreach (var choice in option.Choices ?? [])
                    {
                        optionBuilder.AddChoice(choice.Name, (string)choice.Value);
                    }
                    builder.AddOption(optionBuilder);
                }
                builder.Build();
            }
        }
    }
}
