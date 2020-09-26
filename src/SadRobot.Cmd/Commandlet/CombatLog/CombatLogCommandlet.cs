using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using SadRobot.Core.Extensions;
using SadRobot.Core.Models;

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
        
        Task SplitLogDefaultAsync()
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
            
            var pipe = new Pipe();

            Task fill = Task.Run( () => FillPipeAsync(pipe.Writer, logsDirectory), token);
            Task process = Task.Run(() => ProcessPipeAsync(pipe.Reader, Console.WriteLine), token);
            
            return Task.WhenAll(fill, process);
        }

        internal async Task ProcessPipeAsync(PipeReader reader, Action<string> action = null)
        {
            var zoneType = ZoneType.None;
            var zoneId = 0;
            var difficulty = 0;
            var mapId = 0;
            var keyLevel = 0;
            var zoneName = (string) null;
            var loggingStarted = false;
            var combatStarted = false;
            var zoneTimestamp = DateTime.Now;
            
            while (true)
            {
                ReadResult result = await reader.ReadAsync(token);
                ReadOnlySequence<byte> buffer = result.Buffer;
                
                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    if (!TryReadTimestamp(ref line, out var timestamp)) continue;
                    
                    var text = line.ToStringUtf8().Trim();
                    
                    // Split the line. TODO: Optimize
                    var fields = text.Substring(18).Split(',',StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 1) continue;

                    // Event is the first field
                    var eventName = fields[0].Trim();
                    
                    switch (eventName.ToUpperInvariant())
                    {
                        case "ZONE_CHANGE":

                            // <Timestamp>  ZONE_CHANGE,Map,Sub zone name,<difficulty>

                            mapId = int.Parse(fields[1]);
                            zoneName = fields[2].Trim('\"');
                            difficulty = int.Parse(fields[3]);
                            
                            // If it's a raid, then start logging if we aren't already.
                            var value = Enum.GetName(typeof(DifficultyType), difficulty);
                            if (value != null && value.StartsWith("Raid", StringComparison.OrdinalIgnoreCase))
                            {
                                zoneTimestamp = timestamp;
                                action?.Invoke(timestamp + " Zoned into " + zoneName);
                                continue;
                            }

                            continue;
                            // If we're not in a raid or dungeon yet, then update
                            // if (zoneType == ZoneType.None)
                            // {
                            //     difficulty = int.Parse(fields[3]);
                            // }


                            break;

                        case "CHALLENGE_MODE_START":
                            // <timestamp>  CHALLENGE_MODE_START,"Freehold",1754,245,10,[10,8,12,119]
                            // <timestamp>  CHALLENGE_MODE_START,"Map Name",Map,MapChallengeMode,KeyLevel,[Affix1,Affix2,Affix3,Affix4]
                            combatStarted = false;
                            zoneName = fields[1];
                            zoneId = int.Parse(fields[2]);
                            mapId = int.Parse(fields[3]);
                            keyLevel = int.Parse(fields[4]);
                            
                            Console.WriteLine(timestamp + " KEYSTONE STARTED " + zoneName + " +"  + keyLevel);
                            continue;

                        case "CHALLENGE_MODE_END":
                            // 10/18 16:01:50.541  CHALLENGE_MODE_END,1754,1,8,1046598
                            // <timestamp> CHALLENGE_MODE_END,Map,success,medal,seconds
                            mapId = int.Parse(fields[1]);
                            var finished = int.Parse(fields[2]) == 1;
                            var medal = int.Parse(fields[3]);
                            var seconds = int.Parse(fields[4]);

                            // If it's 0 seconds, then it's a "reset" of challenge mode before we start a proper key
                            if (seconds == 0) continue;

                            if (finished)
                            {
                                Console.WriteLine(timestamp + " KEYSTONE TIMED " + zoneName + " in " + TimeSpan.FromMilliseconds(seconds) );
                            }
                            else
                            {
                                Console.WriteLine(timestamp + " KEYSTONE FAILED " + zoneName + " in " + TimeSpan.FromMilliseconds(seconds));
                            }

                            continue;

                        case "ENCOUNTER_END":
                            // 4/9 01:00:45.777  ENCOUNTER_END,DungeonEncounter,"Name",Difficulty,GroupSize,Success
                            var encounterId = int.Parse(fields[1]);
                            var encounterName = fields[2];
                            difficulty = int.Parse(fields[3]);
                            var groupSize = int.Parse(fields[4]);
                            var success = int.Parse(fields[5]) == 1;

                            // If it's a keystone, don't worry about logging bosses
                            if( difficulty == (int) DifficultyType.DungeonKeystone) continue;
                            
                            if (success)
                            {
                                Console.WriteLine(timestamp + " KILLED " + encounterName);
                            }
                            else
                            {
                                Console.WriteLine(timestamp + " WIPED " + encounterName);
                            }

                            continue;

                        case "ENCOUNTER_START":
                            // 4/9 00:55:05.380  ENCOUNTER_START,2331,"Ra-den the Despoiled",15,29,2217
                            // 4/9 00:55:05.380  ENCOUNTER_START,DungeonEncounter,"Name",Difficulty,GroupSize,Map
                            continue;

                        default:
                            continue;
                    }

                    // Print out the line
                    action?.Invoke(text);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted) break;
            }

            await reader.CompleteAsync();
        }

        internal static readonly string dateTimeFormat = "M/d HH:mm:ss.fff";

        static readonly char[] timestampBuffer = new char[18];
        internal static readonly CultureInfo usEnglish = new CultureInfo("en-US");

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
            SequencePosition? position = buffer.PositionOf( (byte)delimiter);

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

        internal async Task FillPipeAsync(PipeWriter writer, Stream stream)
        {
            await stream.CopyToAsync(writer, cancellationToken: token);
            await writer.CompleteAsync();
        }

        async Task FillPipeAsync(PipeWriter writer, DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles("WoWCombatLog*.txt",SearchOption.TopDirectoryOnly))
            {
                if (!file.Exists) continue;
                await using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                await stream.CopyToAsync(writer, token).ConfigureAwait(false);
            }

            await writer.CompleteAsync().ConfigureAwait(false);
        }

        async Task FillPipeAsync(PipeWriter writer, FileInfo fileInfo)
        {
            await FillPipeAsync(writer, fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }
    }
}