using SadRobot.Core.Models;
using Xunit;

namespace SadRobot.Tests
{
    public class KeystoneSeasonsTests
    {
        [Fact]
        public void VerifySeasons()
        {
            // Legion

            // 7.2.0
            Assert.Equal("2017-03-28 15:00:00Z", KeystoneSeasons.Season720.StartTime.Value.ToString("u"));
            Assert.Equal("2017-06-13 14:59:59Z", KeystoneSeasons.Season720.EndTime.Value.ToString("u"));
            Assert.Equal(720, KeystoneSeasons.Season720.Id);
            Assert.Equal("season-7.2.0", KeystoneSeasons.Season720.RioSlug);
        }
    }
}