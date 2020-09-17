using AstroBot.CronTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class IntermediateRocketLaunchNotifyTests
    {
        [TestMethod]
        public void Test()
        {
            var task = new IntermediateRocketLaunchNotify();
            task.Execute();
        }
    }
}
