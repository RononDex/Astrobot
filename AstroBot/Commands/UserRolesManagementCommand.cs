using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class UserRolesManagementCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public List<string> Regex => new List<string>
        {
            "give( me(( the)? role)?)? (?'GiveRoleName'.*)",
            "remove(( the)? role)?(from me)? (?'RemoveRoleName'.*)"
        };

        public override string Name => "UserRolesManagement";

        public string Description => "Allows users to self-assign roles";

        public string[] ExampleCalls => new[]
        {
            "give me role [RoleName]",
            "remove role [RoleName]"
        };

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                if (regexMatch.Groups["GiveRoleName"].Success)
                {
                    var roleNameToAdd = regexMatch.Groups["GiveRoleName"].Value;
                    var availableRolesToSelfAssign = receivedMessage.ApiWrapper.ConfigStore
                        .GetConfigValue<string>("UserSelfAssignableRoles", defaultValue: null, receivedMessage.Channel.ParentServer)?
                        .Split(";");

                    if (availableRolesToSelfAssign?.Contains(roleNameToAdd, System.StringComparer.Ordinal) != true)
                    {
                        receivedMessage.Channel.SendMessageAsync($"The role \"{roleNameToAdd}\" does not exist or I have no permission to assign it to you!");
                        return true;
                    }

                    receivedMessage.Author.AddRole(roleNameToAdd);
                    receivedMessage.Channel.SendMessageAsync($"Done! You are now assigned the role {roleNameToAdd}").Wait();
                }

                if (regexMatch.Groups["RemoveRoleName"].Success)
                {
                    var roleNameToRemove = regexMatch.Groups["RemoveRoleName"].Value;

                    var availableRolesToSelfAssign = receivedMessage.ApiWrapper.ConfigStore
                        .GetConfigValue<string>("UserSelfAssignableRoles", defaultValue: null, receivedMessage.Channel.ParentServer)?
                        .Split(";");

                    if (availableRolesToSelfAssign?.Contains(roleNameToRemove, System.StringComparer.Ordinal) != true)
                    {
                        receivedMessage.Channel.SendMessageAsync($"The role \"{roleNameToRemove}\" does not exist or I have no permission to remove it from you!");
                        return true;
                    }

                    receivedMessage.Author.RemoveRole(roleNameToRemove);
                    receivedMessage.Channel.SendMessageAsync($"Done! I have removed the role {roleNameToRemove} from you!").Wait();
                }

                return true;
            });
        }
    }
}