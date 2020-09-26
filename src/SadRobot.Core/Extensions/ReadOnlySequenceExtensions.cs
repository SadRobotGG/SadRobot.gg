using System;
using System.Buffers;
using System.Text;
using System.Xml;

namespace SadRobot.Core.Extensions
{
    public static class ReadOnlySequenceExtensions
    {
        public static string ToString(this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment) return encoding.GetString(buffer.First.Span);

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    encoding.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        public static string ToStringAscii(this ReadOnlySequence<byte> buffer)
        {
            return ToString(buffer, Encoding.ASCII);
        }
        public static string ToStringUtf8(this ReadOnlySequence<byte> buffer)
        {
            return ToString(buffer, Encoding.UTF8);
        }

        public static bool TryReadTo<T>(this ReadOnlySequence<T> buffer, ref ReadOnlySpan<T> value, out ReadOnlySequence<T> result) where T : unmanaged, IEquatable<T>
        {
            var reader = new SequenceReader<T>(buffer);
            return reader.TryReadTo(out result, value);
        }

        static readonly string whitespace = " \t\r";

        public static ReadOnlySequence<byte> TrimStart(this ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            var advanced = reader.AdvancePastAny((byte)' ', (byte)'\t', (byte)'\r');
            return reader.Sequence.Slice(reader.Position);
        }

        public static ReadOnlySequence<byte> TrimEnd(this ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            
            // Start at the end of the string
            reader.Advance(reader.Length);

            while (reader.CurrentSpan[reader.CurrentSpanIndex-1] == (byte)' ' || reader.CurrentSpan[reader.CurrentSpanIndex-1] == (byte)'\t' || reader.CurrentSpan[reader.CurrentSpanIndex-1] == (byte)'\r')
            {
                if (reader.Position.GetInteger() <= 0) break;
                reader.Rewind(1);
            }

            return reader.Sequence.Slice(0, reader.Position);
        }
    }
}
