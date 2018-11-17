namespace AstroBot.Commands
{
    /// <summary>
    /// Marks the command to have a description, which is used by the "help" command
    /// </summary>
    public interface ICommandDescription
    {
        string Description { get; }

        string ExampleCall { get;  }
    }
}