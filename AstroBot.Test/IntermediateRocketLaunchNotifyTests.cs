using System.Threading.Tasks;
using AstroBot.CronTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class IntermediateRocketLaunchNotifyTests
    {
        [TestMethod]
        [Ignore("Needs Globals.BotFramework with a logged in discord wrapper, run manually against a test bot")]
        public async Task Test()
        {
            var task = new IntermediateRocketLaunchNotify();
            await task.ExecuteAsync();
        }
    }
}
