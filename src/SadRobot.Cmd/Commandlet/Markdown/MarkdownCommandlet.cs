using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SadRobot.Core.Models;

namespace SadRobot.Cmd.Commandlet.Markdown
{
    class MarkdownCommandlet
    {
        readonly CancellationToken token;
        readonly CommandLineArguments args;
        readonly DirectoryInfo contentDir;

        public MarkdownCommandlet(string[] commandline, in CancellationToken token)
        {
            this.token = token;
            args = new CommandLineArguments(commandline);

            contentDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "content\\wow"));
            if (!contentDir.Exists) contentDir.Create();
        }

        public async Task GenerateKeystoneDungeonMarkdown()
        {
            var tiers = await Program.ReadJson<IList<WowExpansion>>("tiers.json");
            var instances = await Program.ReadJson<IList<Instance>>("instances.json");
            var keystones = await Program.ReadJson<IList<MythicKeystone>>("keystones.json");

            var dir = contentDir.CreateSubdirectory("keystone");
            
            foreach (var tier in tiers)
            {
                var tierDir = new DirectoryInfo(Path.Combine(dir.FullName, tier.Slug));

                foreach (var keystone in keystones)
                {
                    // Get matching dungeon
                    var instance = instances.Single(x => x.MapId == keystone.MapId);

                    // Skip if it's for the wrong tier
                    if (instance.TierId != tier.TierId) continue;

                    // Create the keystone content directory
                    var keystoneDir = new DirectoryInfo(Path.Combine(tierDir.FullName, keystone.Slug));
                    if (!keystoneDir.Exists) keystoneDir.Create();

                    // Create the index
                    var sb = new StringBuilder();

                    sb.AppendLine("---")
                        .AppendLine("title: \"" + keystone.Name + "\"")
                        .AppendLine("description: \"" + instance.Description + "\"")
                        .AppendLine("instance: " + instance.Id)
                        .AppendLine("imgXl: " + instance.BackgroundImageId)
                        .AppendLine("imgLg: " + instance.LoreImageId)
                        .AppendLine("imgSm: " + instance.ButtonImageId)
                        .AppendLine("imgXs: " + instance.ButtonSmallImageId)
                        .AppendLine("map: " + instance.MapId)
                        .AppendLine("---")
                        .AppendLine()
                        .AppendLine("# " + keystone.Name)
                        ;

                    await File.WriteAllTextAsync(Path.Combine(keystoneDir.FullName, "_index.md"), sb.ToString(), Encoding.UTF8, token);

                    // Create a page for each encounter
                    foreach (var encounter in instance.Encounters.OrderBy(x => x.Order))
                    {
                        var esb = new StringBuilder();

                        esb.AppendLine("---")
                            .AppendLine($"title: \"{encounter.Name}\"")
                            .AppendLine($"description: \"" + encounter.Description.Replace("\"", "\\\"") + $"\"")
                            .AppendLine($"encounterId: \"{encounter.EncounterId}\"")
                            .AppendLine($"uiMapId: \"{encounter.UiMapId}\"")
                            .AppendLine("weight: " + encounter.Order)
                            .AppendLine("---")
                            .AppendLine()
                            .AppendLine($"## {encounter.Name}")
                            .AppendLine()
                            .AppendLine($"{encounter.Description}");

                        if (encounter.Sections.Count > 0) esb.AppendLine("");

                        foreach (var section in encounter.Sections)
                        {
                            esb.AppendLine($"* {section.Name}");
                        }

                        var slug = string.IsNullOrWhiteSpace(encounter.Slug) ? Program.GetSlug(encounter.Name) : encounter.Slug;

                        await File.WriteAllTextAsync(Path.Combine(keystoneDir.FullName, slug + ".md"), esb.ToString(), Encoding.UTF8, token);
                    }
                }
            }
        }

        public async Task Execute()
        {
            switch (args.Command)
            {
                case Command.KeystoneSeasons:

                    var keystoneDir = contentDir.CreateSubdirectory("keystone");

                    foreach (var season in KeystoneSeasons.All.Values)
                    {
                        var sb = new StringBuilder();

                        sb.AppendLine("---")
                            .AppendLine($"title: \"{season.Name}\"")
                            .AppendLine($"rioId: \"{season.RioSlug}\"")
                            .AppendLine($"startDate: \"{season.StartTime}\"")
                            .AppendLine($"endDate: \"{season.EndTime}\"")
                            .AppendLine($"expansion: \"{season.Expansion.Id}\"")
                            .AppendLine("---")
                            .AppendLine()
                            .AppendLine("# " + season.Name);

                        var seasonDir = keystoneDir.CreateSubdirectory(season.Expansion.Slug + "\\" + season.Slug);

                        await File.WriteAllTextAsync(Path.Combine(seasonDir.FullName, "_index.md"), sb.ToString(), Encoding.UTF8, token);
                    }

                    break;
            }
        }
    }
    public enum Command
    {
        None = 0,
        KeystoneSeasons = 1,
        KeystoneDungeons = 2
    }

    public class CommandLineArguments
    {
        public Command Command { get; set; }

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
                    default:
                        break;
                }
            }

            switch (Command)
            {
                case Command.None:
                    break;

                default:
                    break;
            }
        }
    }

}
