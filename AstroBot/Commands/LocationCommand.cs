using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// Command that can get the lat / lng coordinates for any place on earth
    /// </summary>
    public class LocationCommand : AwesomeChatBot.Commands.Command, ISlashCommand, ICommandDescription
    {
        /// <summary>
        /// The name of the command
        /// </summary>
        public override string Name => "GeoLocation";

        public string Description => "Gets the Lat / Lng coordinates of a certain location";

        public string SlashName => "location";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption { Name = "place", Description = "Name of the place, e.g. Zurich", Type = SlashOptionType.String, Required = true }
        };

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="invocation"></param>
        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var geoLocation = await GeoLocation.GeoLocation.FindLocationAsync(invocation.GetOption<string>("place")).ConfigureAwait(false);

            if (geoLocation == null)
            {
                await invocation.RespondAsync("I don't know any place on earth with the that name").ConfigureAwait(false);
                return;
            }

            await invocation.RespondAsync($"I found the following location:\r\n" +
                                          Globals.ApiWrapper.MessageFormatter.Quote($"Name:   {geoLocation.Name}\r\n" +
                                          $"Lat:    {geoLocation.Lat}\r\n" +
                                          $"Lng:    {geoLocation.Lng}")).ConfigureAwait(false);
        }
    }
}
