using Xunit;
using Xunit.Abstractions;

namespace OSharp.Net.Tests
{
    public class NetHelperTests
    {
        private readonly ITestOutputHelper log;

        public NetHelperTests(ITestOutputHelper log)
        {
            this.log = log;
        }

        [Fact()]
        public void PingTest()
        {
            bool flag = NetHelper.Ping("localhost");
            log.WriteLine($"flag: {flag}");
            Assert.True(flag);
        }

        [Fact(Skip = "Linux Error")]
        public void IsInternetConnectedTest()
        {
            bool flag = NetHelper.IsInternetConnected();
            log.WriteLine($"flag: {flag}");
            Assert.True(flag);
        }
    }
}