using System.Collections.Generic;
using System.Linq;
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

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (regexMatch.Groups["GiveRoleName"].Success)
            {
                var roleNameToAdd = regexMatch.Groups["GiveRoleName"].Value;
                var availableRolesToSelfAssign = receivedMessage.ApiWrapper.ConfigStore
                    .GetConfigValue<string>("UserSelfAssignableRoles", defaultValue: null, receivedMessage.Channel.ParentServer)?
                    .Split(";");

                if (availableRolesToSelfAssign?.Contains(roleNameToAdd, System.StringComparer.Ordinal) != true)
                {
                    await receivedMessage.Channel.SendMessageAsync($"The role \"{roleNameToAdd}\" does not exist or I have no permission to assign it to you!").ConfigureAwait(false);
                    return true;
                }
                if (receivedMessage.Author.Roles.Any(x => string.Equals(x.Name, roleNameToAdd, System.StringComparison.Ordinal)))
                {
                    await receivedMessage.Channel.SendMessageAsync($"You have no role of name \"{roleNameToAdd}\" assigned").ConfigureAwait(false);
                    return true;
                }

                await receivedMessage.Author.AddRoleAsync(roleNameToAdd).ConfigureAwait(false);
                await receivedMessage.Channel.SendMessageAsync($"Done! You are now assigned the role {roleNameToAdd}").ConfigureAwait(false);
            }

            if (regexMatch.Groups["RemoveRoleName"].Success)
            {
                var roleNameToRemove = regexMatch.Groups["RemoveRoleName"].Value;

                if (!receivedMessage.Author.Roles.Any(x => string.Equals(x.Name, roleNameToRemove, System.StringComparison.Ordinal)))
                {
                    await receivedMessage.Channel.SendMessageAsync($"You have no role of name \"{roleNameToRemove}\" assigned").ConfigureAwait(false);
                    return true;
                }

                await receivedMessage.Author.RemoveRoleAsync(roleNameToRemove).ConfigureAwait(false);
                await receivedMessage.Channel.SendMessageAsync($"The role \"{roleNameToRemove}\" has been removed!").ConfigureAwait(false);
            }

            if (regexMatch.Groups["RemoveRoleName"].Success)
            {
                var roleNameToRemove = regexMatch.Groups["RemoveRoleName"].Value;

                var availableRolesToSelfAssign = receivedMessage.ApiWrapper.ConfigStore
                    .GetConfigValue<string>("UserSelfAssignableRoles", defaultValue: null, receivedMessage.Channel.ParentServer)?
                    .Split(";");

                if (availableRolesToSelfAssign?.Contains(roleNameToRemove, System.StringComparer.Ordinal) != true)
                {
                    await receivedMessage.Channel.SendMessageAsync($"The role \"{roleNameToRemove}\" does not exist or I have no permission to remove it from you!").ConfigureAwait(false);
                    return true;
                }

                await receivedMessage.Author.RemoveRoleAsync(roleNameToRemove).ConfigureAwait(false);
                await receivedMessage.Channel.SendMessageAsync($"Done! I have removed the role {roleNameToRemove} from you!").ConfigureAwait(false);
            }

            return true;
        }
    }
}