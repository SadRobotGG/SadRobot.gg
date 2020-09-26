using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SadRobot.Cmd.Commandlet.CombatLog;
using SadRobot.Core.Extensions;
using Xunit;

namespace SadRobot.Tests.CombatLog
{
    public class CombatLogParserTests
    {
        [Fact]
        public void Test()
        {
            var encoding = Encoding.UTF8;

            var stream = new MemoryStream(encoding.GetBytes("1/20 16:24:19.021  COMBAT_LOG_VERSION,14,ADVANCED_LOG_ENABLED,1,BUILD_VERSION,8.3.0,PROJECT_ID,1,\"Test string, with 'comma'\",last value"));

            var reader = new StreamReader(stream, encoding, false);
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                
                if (line == null) break;

                if (!DateTime.TryParseExact(line.AsSpan(0, 18), CombatLogCommandlet.dateTimeFormat.AsSpan(), CombatLogCommandlet.usEnglish,
                    DateTimeStyles.AllowTrailingWhite, out var timestamp))
                {
                    continue;
                }

                var fields = new List<string>();

                // Fields start at character 20 (after timestamp)
                var span = line.AsSpan().Slice(19);
                
                var j = 0;
                var inString = false;
                for (int i = 0; i < span.Length; i++)
                {
                    if (span[i] == ',' && !inString)
                    {
                        var value = span[j..i];
                        
                        // Trim off any quotes
                        if (value[0] == '\"') value = value[1..];
                        if (value[^1] == '\"' && value.Length > 1) value = value[..^1];

                        fields.Add(value.ToString());

                        j = i + 1;
                    }
                    else if (span[i] == '\"')
                    {
                        inString = !inString;
                    }
                }

                fields.Add(span.Slice(j).ToString());
                
                Assert.Equal(10, fields.Count);
                Assert.Equal("COMBAT_LOG_VERSION", fields[0]);
                Assert.Equal("14", fields[1]);
                Assert.Equal("ADVANCED_LOG_ENABLED", fields[2]);
                Assert.Equal("1", fields[3]);
                Assert.Equal("BUILD_VERSION", fields[4]);
                Assert.Equal("8.3.0", fields[5]);
                Assert.Equal("PROJECT_ID", fields[6]);
                Assert.Equal("1", fields[7]);
                Assert.Equal("Test string, with 'comma'", fields[8]);
                Assert.Equal("last value", fields[9]);
            }
        }
    }

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
        public async Task TryParseLine()
        {
            var cmd = new CombatLogCommandlet(new string []{}, CancellationToken.None);

            var pipe = new Pipe(PipeOptions.Default);
            var sb = new StringBuilder();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(@"
4/9 00:55:05.380  ENCOUNTER_START,2331,""Ra-den the Despoiled"",15,29,2217
4/9 01:00:45.777  ENCOUNTER_END,2331,""Ra-den the Despoiled"",15,29,1
4/8 22:27:10.839  WORLD_MARKER_PLACED,2217,0,-1783.76,8.62
4/8 21:00:06.684  ZONE_CHANGE,1762,Kings' Rest,23
4/8 21:39:38.859  ZONE_CHANGE,1642,Zuldazar,0
4/8 22:16:42.542  COMBAT_LOG_VERSION,14,ADVANCED_LOG_ENABLED,1,BUILD_VERSION,8.3.0,PROJECT_ID,1
4/8 22:16:42.542  ZONE_CHANGE,2217,Ny'alotha, the Waking City,15
4/8 23:52:01.555  COMBAT_LOG_VERSION,14,ADVANCED_LOG_ENABLED,1,BUILD_VERSION,8.3.0,PROJECT_ID,1
4/8 23:52:01.555  ZONE_CHANGE,2217,Ny'alotha, the Waking City,15
4/9 01:02:28.695  ZONE_CHANGE,1642,Zuldazar,0
4/11 18:26:51.187  CHALLENGE_MODE_END,1864,0,0,0
4/11 18:26:51.249  SPELL_AURA_REMOVED,Player-3729-09873409,""Bumebumbo-Saurfang"",0x512,0x0,Player-3729-09873409,""Bumebumbo-Saurfang"",0x512,0x0,296138,""The Well of Existence"",0x8,BUFF
4/11 18:26:51.562  CHALLENGE_MODE_START,""Shrine of the Storm"",1864,252,15,[9,5,3,120]
4/11 19:23:22.615  CHALLENGE_MODE_END,1864,1,15,3452107
4/11 19:34:15.832  CHALLENGE_MODE_END,1771,0,0,0
4/11 19:34:15.844  CHALLENGE_MODE_START,""Tol Dagor"",1771,246,16,[9,5,3,120]
4/11 19:53:55.783  CHALLENGE_MODE_END,1771,0,0,0
4/11 19:53:55.799  CHALLENGE_MODE_START,""Tol Dagor"",1771,246,15,[9,5,3,120]
4/11 20:26:55.927  CHALLENGE_MODE_END,1771,1,15,2001377
4/29 19:51:13.741  SPELL_CAST_SUCCESS,Player-3723-0AF4D09B,""Whupwhup-Barthilas"",0x511,0x0,Vehicle-0-3766-1763-20553-122963-0000293048,""Rezan"",0x10a48,0x0,275779,""Judgment"",0x2,Player-3723-0AF4D09B,0000000000000000,747034,794192,16389,1469,11393,53126,0,20000,20000,0,-786.37,2241.71,935,2.4893,472
4/29 19:51:14.853  ENCOUNTER_START,2086,""Rezan"",23,5,1763
"));

            Task fill = cmd.FillPipeAsync(pipe.Writer, stream);
            Task process = cmd.ProcessPipeAsync(pipe.Reader, line => sb.AppendLine(line));
            await Task.WhenAll(fill, process);

            // Verify
            Assert.Equal(@"4/9 00:55:05.380  ENCOUNTER_START,2331,""Ra-den the Despoiled"",15,29,2217
4/9 01:00:45.777  ENCOUNTER_END,2331,""Ra-den the Despoiled"",15,29,1
4/8 21:00:06.684  ZONE_CHANGE,1762,Kings' Rest,23
4/8 21:39:38.859  ZONE_CHANGE,1642,Zuldazar,0
4/8 22:16:42.542  ZONE_CHANGE,2217,Ny'alotha, the Waking City,15
4/8 23:52:01.555  ZONE_CHANGE,2217,Ny'alotha, the Waking City,15
4/9 01:02:28.695  ZONE_CHANGE,1642,Zuldazar,0
4/11 18:26:51.187  CHALLENGE_MODE_END,1864,0,0,0
4/11 18:26:51.562  CHALLENGE_MODE_START,""Shrine of the Storm"",1864,252,15,[9,5,3,120]
4/11 19:23:22.615  CHALLENGE_MODE_END,1864,1,15,3452107
4/11 19:34:15.832  CHALLENGE_MODE_END,1771,0,0,0
4/11 19:34:15.844  CHALLENGE_MODE_START,""Tol Dagor"",1771,246,16,[9,5,3,120]
4/11 19:53:55.783  CHALLENGE_MODE_END,1771,0,0,0
4/11 19:53:55.799  CHALLENGE_MODE_START,""Tol Dagor"",1771,246,15,[9,5,3,120]
4/11 20:26:55.927  CHALLENGE_MODE_END,1771,1,15,2001377
4/29 19:51:14.853  ENCOUNTER_START,2086,""Rezan"",23,5,1763
", sb.ToString());
        }

        [Fact]
        public void TryReadDelimitedField()
        {
            var expected = "COMBAT_LOG_VERSION";

            var line = "1/20 16:24:19.021  COMBAT_LOG_VERSION,14,ADVANCED_LOG_ENABLED,1,BUILD_VERSION,8.3.0,PROJECT_ID,1";
            
            var sequence = new ReadOnlySequence<byte>( Encoding.UTF8.GetBytes(line)).Slice(18);

            // Execute
            Assert.True(CombatLogCommandlet.TryReadDelimitedField(ref sequence, out var result, ','));
            
            // Verify
            Assert.Equal(expected, result.TrimStart().ToStringUtf8());
        }

        [Fact]
        public void EventMarker()
        {
            var line = "10/17 09:44:54.769  ENCOUNTER_START,2093,\"Skycap'n Kragg\",8,5,1754";

            var sequence = new ReadOnlySequence<byte>();


        }


    }
    
    public class ReadOnlySequenceExtensions
    {
        [Fact]
        public void TrimStart()
        {
            var expected = "this should be trimmed ";

            var sequence = new ReadOnlySequence<byte>( Encoding.UTF8.GetBytes("   this should be trimmed "));

            var trimmed = sequence.TrimStart();

            Assert.Equal(expected, trimmed.ToStringUtf8());
        }

        [Fact]
        public void TrimEnd()
        {
            var expected = "   the end should be trimmed";

            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes("   the end should be trimmed \t "));

            var trimmed = sequence.TrimEnd();

            Assert.Equal(expected, trimmed.ToStringUtf8());
        }
    }
}
