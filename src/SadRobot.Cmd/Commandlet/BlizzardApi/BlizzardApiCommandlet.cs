using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SadRobot.Core.Apis.BlizzardApi;
using SadRobot.Core.Apis.BlizzardApi.Models;

namespace SadRobot.Cmd.Commandlet.BlizzardApi
{
    public class CommandLineArguments
    {
        public Command Command { get; set; }

        public BlizzardRegion Region { get; set; }

        public CommandLineArguments(IReadOnlyList<string> args)
        {
            if (args == null || args.Count == 0) return;
            
            var commandArg = args[0].Trim();
            
            if (!Enum.TryParse(commandArg, true, out Command command))
            {
                Console.WriteLine($"Unknown command: ${commandArg}");
                return;
            }

            Command = command;

            foreach (var arg in args)
            {
                switch (arg.Trim().ToLowerInvariant().TrimStart('-', '/'))
                {
                    case "cn":
                        Region = BlizzardRegion.China;
                        break;
                    case "us":
                        Region = BlizzardRegion.NorthAmerica;
                        break;
                    case "eu":
                        Region = BlizzardRegion.Europe;
                        break;
                    case "kr":
                        Region = BlizzardRegion.Korea;
                        break;
                    case "tw":
                        Region = BlizzardRegion.Taiwan;
                        break;
                }
            }

            switch (Command)
            {
                case Command.None:
                    break;

                case Command.Player:
                    var playerQualifier = args[1].Split("-");
                    Realm = playerQualifier[0];
                    Character = playerQualifier[1];
                    break;

                default:
                    break;
            }
        }

        public string Character { get; set; }

        public string Realm { get; set; }
    }

    public enum Command
    {
        None = 0,
        Player = 1,
        KeystoneSeasons = 2
    }

    public class BlizzardApiCommandlet
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        readonly BlizzardApiClient client;
        readonly CommandLineArguments commandline;
        readonly CancellationToken token;

        public BlizzardApiCommandlet(string[] args, CancellationToken token)
        {
            client = new BlizzardApiClient(token);
            commandline = new CommandLineArguments(args);
            this.token = token;
        }

        public async Task Execute()
        {
            client.Region = commandline.Region == BlizzardRegion.None ? BlizzardRegion.NorthAmerica : commandline.Region;

            switch (commandline.Command)
            {
                case Command.None:
                    Console.WriteLine("Unknown command");
                    break;

                case Command.Player:
                    await GetPlayer();
                    break;

                case Command.KeystoneSeasons:
                    await GetSeasons();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        async Task GetPlayer()
        {
            var player = await client.GetJsonAsync("profile/wow/character/" + commandline.Realm + "/" + commandline.Character + "/mythic-keystone-profile", 
                BlizzardLocaleFlags.EnglishUS, BlizzardNamespace.Profile);
            await WriteJsonAsync(player, commandline.Realm.ToLowerInvariant() + "-" + commandline.Character.ToLowerInvariant() + ".json");
        }

        async Task GetSeasons()
        {
            // Get the list of seasons
            var seasons = await client.GetAsync<KeystoneSeasons>("data/wow/mythic-keystone/season/index");

            // Hydrate each season
            foreach (var season in seasons.Seasons)
            {
                var seasonModel = await client.GetAsync<KeystoneSeason>("data/wow/mythic-keystone/season/" + season.Id);

                season.EndTimestamp = seasonModel.EndTimestamp;
                season.StartTimestamp = seasonModel.StartTimestamp;

                season.Periods = new List<KeystonePeriod>(seasonModel.Periods.Count);

                foreach (var seasonModelPeriod in seasonModel.Periods)
                {
                    var period = await client.GetAsync<KeystonePeriod>("data/wow/mythic-keystone/period/" + seasonModelPeriod.Id);
                    season.Periods.Add(period);
                }

                if (seasons.CurrentSeason.Id == season.Id) seasons.CurrentSeason = season;
            }

            await WriteJsonAsync(seasons, "seasons.json");
        }



        Task WriteJsonAsync(string value, string filename)
        {
            return File.WriteAllTextAsync(filename, value, Encoding.UTF8, token);
        }

        async Task WriteJsonAsync<T>(T value, string filename)
        {
            await using var stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            await JsonSerializer.SerializeAsync(stream, value, options, token);
        }
    }
}
