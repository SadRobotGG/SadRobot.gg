using System;
using System.Buffers;
using System.Runtime.CompilerServices;
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
        public static string ToStringUtf8(this ReadOnlySequence<byte> buffer)
        {
            return ToString(buffer, Encoding.UTF8);
        }

        public static string ToUtf8String(this ReadOnlySpan<byte> span)
        {
            return ToString(span, Encoding.UTF8);
        }

        public static string ToString(this ReadOnlySpan<byte> span, Encoding encoding)
        {
            return encoding.GetString(span);
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

        public delegate void ReadOnlySpanAction<T>(ReadOnlySpan<T> span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessLine(this ReadOnlySequence<byte> buffer, ReadOnlySpanAction<byte> action, bool readToEnd = false)
        {
            const byte newline = (byte) '\n';
            const byte carriageReturn = (byte) '\r';

            if (buffer.IsSingleSegment)
            {
                var span = buffer.FirstSpan;
                while (span.Length > 0)
                {
                    var newLine = span.IndexOf(newline);
                    
                    if (newLine == -1 && !readToEnd) break;

                    // If there is no newline found, just read to the end.
                    var line = span.Slice(0, newLine == -1 ? span.Length : newLine);
                    
                    // If we did find a newline character, then make sure we count it as a consume character
                    var consumed = line.Length;
                    if (newLine > -1) consumed++;

                    // Trim carriage return from the end
                    if (line[^1] == carriageReturn) line = line[..^1];

                    action(line);
                    
                    span = span.Slice(consumed);
                    buffer = buffer.Slice(consumed);
                }
            }
            else
            {
                var sequenceReader = new SequenceReader<byte>(buffer);

                while (!sequenceReader.End)
                {
                    while (sequenceReader.TryReadTo(out ReadOnlySpan<byte> line, newline))
                    {
                        if (line[^1] == carriageReturn) line = line[..^1]; // Trim trailing carriage return
                        action(line);
                    }

                    if (readToEnd) action(sequenceReader.UnreadSpan);

                    buffer = buffer.Slice(sequenceReader.Position);
                    sequenceReader.Advance(buffer.Length);
                }
            }
        }
    }
}
