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

        public string ExampleCall => "help";

        public List<string> Regex => new List<string>() {
            "test"
        };
        
        public Task<bool> ExecuteRegexCommand(RecievedMessage recievedMessage, Match regexMatch)
        {
            return Task.Factory.StartNew(() => {
                System.Reflection.Assembly ass = System.Reflection.Assembly.GetEntryAssembly();

                recievedMessage.Channel.SendMessageAsync("Following commands are available in this server:").Wait();
                var botFramework       = Globals.ServiceProvider.GetService(typeof(AwesomeChatBot.AwesomeChatBot)) as AwesomeChatBot.AwesomeChatBot;
                var context            = new IConfigurationDependency[] { recievedMessage.Channel.ParentServer };

                foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(ICommandDescription)))
                    {
                        var command = ass.CreateInstance(ti.FullName) as ICommandDescription;
                            
                        // Only list commands
                        if (command is AwesomeChatBot.Commands.Command){

                            // Ignore disabled commands
                            if (!botFramework.ConfigStore.IsCommandActive(command as AwesomeChatBot.Commands.Command, true, context))
                                continue;

                            // Write help output for the command
                            
                            recievedMessage.Channel.SendMessageAsync(new SendMessage()).Wait();
                        }
                    }  
                }

                return true;
            });
        }
    }
}