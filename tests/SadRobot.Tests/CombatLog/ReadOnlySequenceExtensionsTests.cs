using System.Buffers;
using System.Collections.Generic;
using System.Text;
using SadRobot.Core.Extensions;
using SadRobot.Tests.TestHelpers.Buffers;
using Xunit;

namespace SadRobot.Tests.CombatLog
{
    public class ReadOnlySequenceExtensionsTests
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

        [Fact]
        public void ProcessLine_FastPath()
        {
            var test = @"This is the first line
This is the second line
This is the third line, it's quite a bit bigger than the other lines.

This is the fifth line, coming after an empty line";

            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(test));

            var results = new List<string>();

            sequence.ProcessLine(s => results.Add(s.ToUtf8String()));

            Assert.Equal(4, results.Count);
            Assert.Equal("This is the first line", results[0]);
            Assert.Equal("This is the second line", results[1]);
            Assert.Equal("This is the third line, it's quite a bit bigger than the other lines.", results[2]);
            Assert.Equal("", results[3]);
        }

        [Fact]
        public void ProcessLine_FastPath_ReadToEnd()
        {
            var test = @"This is the first line
This is the second line
This is the third line, it's quite a bit bigger than the other lines.

This is the fifth line, coming after an empty line";

            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(test));

            var results = new List<string>();

            sequence.ProcessLine(s => results.Add(s.ToUtf8String()), true);

            Assert.Equal(5, results.Count);
            Assert.Equal("This is the first line", results[0]);
            Assert.Equal("This is the second line", results[1]);
            Assert.Equal("This is the third line, it's quite a bit bigger than the other lines.", results[2]);
            Assert.Equal("", results[3]);
            Assert.Equal("This is the fifth line, coming after an empty line", results[4]);
        }

        [Fact]
        public void ProcessLine_MultiSequence_ReadToEnd()
        {
            var segment1 = @"This is the first line
This is the second line
This is the third line, it's quite a bit bigger than the other lines.

This is the fifth line, coming after an empty line, but it spans into a ";

            var segment2 = @"line in the second sequence.
This is the second line of the second sequence that spans";

            var segment3 = @" into the third sequence.
This is the second line of the third sequence";

            var sequence = SequenceFactory.CreateUtf8(segment1, segment2, segment3);

            var results = new List<string>();

            sequence.ProcessLine(s => results.Add(s.ToUtf8String()), true);

            Assert.Equal(7, results.Count);
            Assert.Equal("This is the first line", results[0]);
            Assert.Equal("This is the second line", results[1]);
            Assert.Equal("This is the third line, it's quite a bit bigger than the other lines.", results[2]);
            Assert.Equal("", results[3]);
            Assert.Equal("This is the fifth line, coming after an empty line, but it spans into a line in the second sequence.", results[4]);
            Assert.Equal("This is the second line of the second sequence that spans into the third sequence.", results[5]);
            Assert.Equal("This is the second line of the third sequence", results[6]);
        }

        [Fact]
        public void ProcessLine_MultiSequence()
        {
            var segment1 = @"This is the first line
This is the second line
This is the third line, it's quite a bit bigger than the other lines.

This is the fifth line, coming after an empty line, but it spans into a ";

            var segment2 = @"line in the second sequence.
This is the second line of the second sequence that spans";

            var segment3 = @" into the third sequence.
This is the second line of the third sequence";

            var sequence = SequenceFactory.CreateUtf8(segment1, segment2, segment3);
            
            var results = new List<string>();

            sequence.ProcessLine(s => results.Add(s.ToUtf8String()));

            Assert.Equal(6, results.Count);
            Assert.Equal("This is the first line", results[0]);
            Assert.Equal("This is the second line", results[1]);
            Assert.Equal("This is the third line, it's quite a bit bigger than the other lines.", results[2]);
            Assert.Equal("", results[3]);
            Assert.Equal("This is the fifth line, coming after an empty line, but it spans into a line in the second sequence.", results[4]);
            Assert.Equal("This is the second line of the second sequence that spans into the third sequence.", results[5]);
        }
    }
}