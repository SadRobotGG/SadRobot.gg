using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SadRobot.Cmd.Commandlet.CombatLog
{
    static class CombatLogEvents
    {
        public static readonly byte[] EncounterStart = Encoding.UTF8.GetBytes("ENCOUNTER_START");
        public static readonly byte[] EncounterEnd = Encoding.UTF8.GetBytes("ENCOUNTER_END");
        public static readonly byte[] ZoneChange = Encoding.UTF8.GetBytes("ZONE_CHANGE");
        public static readonly byte[] KeystoneStart = Encoding.UTF8.GetBytes("CHALLENGE_MODE_START");
        public static readonly byte[] KeystoneEnd = Encoding.UTF8.GetBytes("CHALLENGE_MODE_END");
    }

    public class CombatLogCommandlet
    {
        readonly CommandLineArguments commandline;
        readonly CancellationToken token;

        public CombatLogCommandlet(string[] args, CancellationToken token)
        {
            commandline = new CommandLineArguments(args);
            this.token = token;
        }

        public async Task Execute()
        {
            switch (commandline.Command)
            {
                case Command.None:
                case Command.Split:
                    await SplitLogDefaultAsync();
                    break;
            }
        }
        
        async Task SplitLogDefaultAsync()
        {
            string installPath;
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var key = hklm.OpenSubKey(@"SOFTWARE\Blizzard Entertainment\World of Warcraft"))
            {
                installPath = (string)key?.GetValue("InstallPath") ?? throw new FileNotFoundException("Couldn't find the World of Warcraft install information");
            }
            if (string.IsNullOrWhiteSpace(installPath)) throw new FileNotFoundException("Couldn't locate the install path");

            var installDirectory = new DirectoryInfo(installPath);
            if (!installDirectory.Exists) throw new FileNotFoundException($"WoW doesn't seem to be installed at {installPath}", installPath);

            var logsDirectory = new DirectoryInfo(Path.Combine(installDirectory.FullName, "Logs"));
            if (!logsDirectory.Exists) throw new FileNotFoundException($"No log directory found", logsDirectory.FullName);

            var combatLog = new FileInfo(Path.Combine(logsDirectory.FullName, "WoWCombatLog.txt"));
            if (!combatLog.Exists) return;

            var pipe = new Pipe(PipeOptions.Default);

            Task fill = FilePipeAsync(pipe.Writer, combatLog);
            Task process = ProcessPipeAsync(pipe.Reader);

            await Task.WhenAll(fill, process);
        }

        async Task ProcessPipeAsync(PipeReader reader)
        {
            var zoneType = ZoneType.None;

            while (true)
            {
                ReadResult result = await reader.ReadAsync(token);
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    if (!TryReadTimestamp(ref line, out var timestamp)) continue;

                    // Check for combat start / stop markers
                    
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted) break;
            }

            await reader.CompleteAsync();
        }

        internal static readonly string dateTimeFormat = "M/d HH:mm:ss.fff";

        static readonly char[] timestampBuffer = new char[18];
        static readonly CultureInfo usEnglish = new CultureInfo("en-US");

        internal static bool TryReadTimestamp(ref ReadOnlySequence<byte> buffer, out DateTime dateTime)
        {
            dateTime = default;

            // The timestamp is 16 to 18 chars, followed by up to two spaces
            if (buffer.Length < 16) return false;

            // Get the first 18 characters (which should contain the timestamp, and perhaps some trailing whitespace)
            var timestampSequence = buffer.Slice(0, 18);
            var timestampSpan = timestampBuffer.AsSpan();
            Encoding.UTF8.GetChars(timestampSequence.FirstSpan, timestampSpan);

            // Parse the timestamp
            return DateTime.TryParseExact(timestampSpan, dateTimeFormat.AsSpan(), usEnglish, DateTimeStyles.AllowTrailingWhite, out dateTime);
        }

        internal static bool TryReadField(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> field)
        {
            return TryReadDelimitedField(ref buffer, out field, ',');
        }
        
        internal static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            return TryReadDelimitedField(ref buffer, out line, '\n');
        }

        internal static bool TryReadDelimitedField(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> field, char delimiter)
        {
            SequencePosition? position = buffer.PositionOf((byte)delimiter);

            if (position == null)
            {
                field = default;
                return false;
            }

            // Skip the field + the delimiter.
            field = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        async Task FilePipeAsync(PipeWriter writer, FileInfo fileInfo)
        {
            await using var combatLog = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await combatLog.CopyToAsync(writer, cancellationToken: token);
            await writer.CompleteAsync();
        }
    }
}