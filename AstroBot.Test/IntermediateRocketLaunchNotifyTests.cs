using AstroBot.CronTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    public class IntermediateRocketLaunchNotifyTests
    {
        [TestMethod]
        public void Test()
        {
            IntermediateRocketLaunchNotify.Execute()
        }
    }
}