using System;
using System.Buffers;
using System.Globalization;
using System.Text;
using SadRobot.Cmd.Commandlet.CombatLog;
using Xunit;

namespace SadRobot.Tests.CombatLog
{
    public class CombatLogSplitterTests
    {
        [Fact]
        public void TryParseTimestamp()
        {
            var expected = new DateTime(DateTime.Now.Year, 1, 20, 16, 24, 19, 21);
            var line = "1/20 16:24:19.021  COMBAT_LOG_VERSION,14,ADVANCED_LOG_ENABLED,1,BUILD_VERSION,8.3.0,PROJECT_ID,1";
            
            var bytes = Encoding.UTF8.GetBytes(line);

            var sequence = new ReadOnlySequence<byte>(bytes.AsMemory());

            // Execute
            Assert.True(CombatLogCommandlet.TryReadTimestamp(ref sequence, out var result));

            // Verify
            Assert.Equal(expected, result);
        }

        [Fact]
        public void TimestampFormat()
        {
            Assert.Equal(new DateTime(DateTime.Now.Year, 10, 17, 9,37,17,482),
                DateTime.ParseExact("10/17 09:37:17.482", CombatLogCommandlet.dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowTrailingWhite));

            // Single digit month, with trailing space
            Assert.Equal(new DateTime(DateTime.Now.Year, 1, 20, 16, 24, 19, 21),
                DateTime.ParseExact("1/20 16:24:19.021 ", CombatLogCommandlet.dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowTrailingWhite));
        }

        [Fact]
        public void EventMarker()
        {
            var line = "10/17 09:44:54.769  ENCOUNTER_START,2093,\"Skycap'n Kragg\",8,5,1754";

            var sequence = new ReadOnlySequence<byte>();
        }
    }
}
