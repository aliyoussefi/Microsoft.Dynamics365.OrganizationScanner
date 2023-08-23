using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrganizationScannerUnitTests
{
    [TestClass]
    public class SolutionHistoryRecorderUnitTests : TestsBase
    {

        [TestMethod]
        public void TestSolutionHistoryRecorder_Timer_NoParameters_Successful()
        {

            var recorder = new Microsoft.Dynamics365.OrganizationScanner.SolutionHistoryRecorder();
            var param1 = default(TimerInfo); //null
            var param2 = new Microsoft.Azure.WebJobs.ExecutionContext
            {
                InvocationId = Guid.NewGuid()
            };

            //Act
            recorder.WriteTodaysHistory(null, null, NullLogger<Microsoft.Dynamics365.OrganizationScanner.SolutionHistoryRecorder>.Instance);
        }
    }
}
