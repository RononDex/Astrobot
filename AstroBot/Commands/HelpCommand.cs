using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using AwesomeChatBot.Config;

namespace AstroBot.Commands
{
    public class HelpCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {

        public override string Name => "Help";

        public string Description => "Displays this help";

        public string[] ExampleCalls => new [] { "Help" };

        public List<string> Regex => new List<string>() {
            "help"
        };
        
        public Task<bool> ExecuteRegexCommand(RecievedMessage recievedMessage, Match regexMatch)
        {
            return Task.Factory.StartNew(() => {
                System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();

                recievedMessage.Channel.SendMessageAsync("Following commands are available in this server:").Wait();
                var configStore        = recievedMessage.ApiWrapper.ConfgiStore;
                var context            = new IConfigurationDependency[] { recievedMessage.Channel.ParentServer };

                foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(ICommandDescription)))
                    {
                        var command = ass.CreateInstance(ti.FullName) as ICommandDescription;
                            
                        // Only list commands
                        if (command is AwesomeChatBot.Commands.Command){

                            // Ignore disabled commands
                            if (!configStore.IsCommandActive(command as AwesomeChatBot.Commands.Command, true, context))
                                continue;

                            // Write help output for the command
                            var formatter = recievedMessage.ApiWrapper.MessageFormatter;
                            var message = $"{formatter.Bold(command.Name)}: {command.Description}\r\nExample(s):\r\n";
                            var examples = string.Empty;
                            foreach (var exampleCall in command.ExampleCalls){
                                examples += "@AstroBot " + exampleCall + "\r\n";
                            }

                            message += formatter.Quote(examples);

                            recievedMessage.Channel.SendMessageAsync(new SendMessage(message)).Wait();
                        }
                    }  
                }

                return true;
            });
        }
    }
}