using System;
using System.Buffers;
using System.Text;

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

        public static bool TryReadTo<T>(this ReadOnlySequence<T> buffer, ref ReadOnlySpan<T> value, out ReadOnlySequence<T> result) where T : unmanaged, IEquatable<T>
        {
            var reader = new SequenceReader<T>(buffer);
            return reader.TryReadTo(out result, value);
        }
    }
}
