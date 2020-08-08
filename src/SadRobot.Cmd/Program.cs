using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CASCLib;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using SadRobot.Cmd.Commandlet;
using SadRobot.Cmd.Commandlet.BlizzardApi;
using SadRobot.Cmd.Commandlet.Markdown;
using SadRobot.Core.Models;

namespace SadRobot.Cmd
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            using var cancellationToken = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            { 
                // ReSharper disable AccessToDisposedClosure
                if (!cancellationToken.IsCancellationRequested) cancellationToken.Cancel();
                // ReSharper restore AccessToDisposedClosure
            };
            
            var cmd = args.Length == 0 ? "markdown" : args[0].Trim().ToLowerInvariant();
            var search = args.Length < 2 ? "18452" : args[1].Trim();

            var commandletArgs = args.Skip(1).ToArray();

            switch (cmd)
            {
                case "blizzard":
                    var commandlet = new BlizzardApiCommandlet(commandletArgs, cancellationToken.Token);
                    await commandlet.Execute();
                    break;

                case "caches":
                    await ScanCache.Execute();
                    break;

                case "search":
                    await SearchDatabase.Execute(search);
                    break;

                case "sql":
                    ExportToSql.Execute();
                    break;
                    
                case "mdt":
                    //using (var file = File.OpenRead(@"D:\Work\RoboKiwi.gg\src\RoboKiwi.Core\Models\MethodDungeonTools.json"))
                    var json = await File.ReadAllTextAsync(@"D:\Work\RoboKiwi.gg\src\RoboKiwi.Core\Models\MethodDungeonTools.json");
                    {
                        //dynamic model = await JsonSerializer.DeserializeAsync<dynamic>(file, options, CancellationToken.None);

                        dynamic model = JsonConvert.DeserializeObject(json);

                        foreach (var dungeon in model.global.dungeonEnemies)
                        {
                            if (dungeon == null) continue;

                            foreach (var enemy in dungeon)
                            {
                                if (enemy == null) continue;

                                Console.WriteLine(enemy.name);
                            }
                        }
                    }
                    break;

                case "journal":
                    await ExtractJournal();
                    break;

                case "markdown":
                    var markdown = new MarkdownCommandlet(commandletArgs, cancellationToken.Token);
                    await markdown.Execute();
                    break;

                case "json":
                    await GenerateJson();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(args));
            }
        }

        static async Task GenerateJson()
        {
            // Get the base model
            var model = MythicKeystoneDungeons.All;

            // Load the MDT data
            var mdtJson = await File.ReadAllTextAsync(@"D:\Work\RoboKiwi.gg\src\RoboKiwi.Core\Models\MethodDungeonTools.json");
            
            dynamic mdt = JsonConvert.DeserializeObject(mdtJson);

            await using var connection = new SqlConnection(Configuration.ConnectionString);
            await connection.OpenAsync();

            const string treeQuery = @"SELECT ct.*, c.Asset, c.Type, npc.Name_lang, je.ID AS JournalID, jec.CreatureDisplayInfoID AS DisplayID FROM CriteriaTree ct 
	LEFT JOIN Criteria c ON c.ID = ct.CriteriaID
	LEFT JOIN Creature npc ON npc.ID = c.Asset
    LEFT JOIN JournalEncounter je ON je.DungeonEncounterID = c.Asset AND c.Type = 165
    LEFT JOIN journalencountercreature jec ON jec.JournalEncounterID = je.ID AND jec.OrderIndex=0
WHERE Parent=@Id
ORDER BY OrderIndex";


            // SELECT ct.*, c.Type, c.Asset, npc.Name_lang, je.ID AS JournalID, jec.CreatureDisplayInfoID AS DisplayID FROM CriteriaTree ct
            //
            // LEFT JOIN Criteria c ON c.ID = ct.CriteriaID
            //
            // LEFT JOIN Creature npc ON npc.ID = c.Asset
            //
            // LEFT JOIN JournalEncounter je ON je.DungeonEncounterID = c.Asset AND c.Type = 165
            //
            // LEFT JOIN journalencountercreature jec ON jec.JournalEncounterID = je.ID AND jec.OrderIndex = 0
            // WHERE Parent = 65642
            // ORDER BY OrderIndex


            foreach (var keystone in model)
            {
                if( string.IsNullOrWhiteSpace(keystone.Slug) ) keystone.Slug = GetSlug(keystone.Name);

                var mdtEnemies = mdt.global.dungeonEnemies[keystone.MdtId - 1];

                // This will hold all possible NPCs
                var npcs = new Dictionary<int, KeystoneNpc>();
                var bosses = new Dictionary<int, KeystoneNpc>();
                
                // Traverse the criteria tree to get mobs
                foreach (var criteria in keystone.Critera)
                {
                    var rows = connection.ExecuteQuery(treeQuery,
                        new KeyValuePair<string, object>("Id", criteria.CriteriaTreeId)).ToList();

                    foreach (var row in rows)
                    {
                        if ((int) row["Operator"] == 9)
                        {
                            // Enemy forces
                            criteria.EnemyForcesCount = (int) row["Amount"];

                            // Get all valid mobs
                            var mobs = connection.ExecuteQuery(treeQuery, new KeyValuePair<string, object>("Id", row["ID"])).ToList();

                            foreach (var mobRow in mobs)
                            {
                                var id = (int) mobRow["Asset"];

                                // Get existing mob
                                if(!npcs.TryGetValue(id, out var mob))
                                {
                                    mob = new KeystoneNpc
                                    {
                                        Name = (string)mobRow["Description_lang"],
                                        Id = id,
                                        Faction = criteria.Faction
                                    };
                                }
                                else
                                {
                                    // If the mob has been flagged for a different faction but also exists for
                                    // the current faction, then it must exist for both (0)
                                    if (criteria.Faction != 0 && mob.Faction != criteria.Faction) mob.Faction = 0;
                                }

                                if (mob.Faction == -1) mob.Faction = 0;

                                var mdtNpc = GetNpcById(mdtEnemies, id);
                                
                                var count = (int)mobRow["Amount"];
                                if (criteria.Affix == 5)
                                {
                                    mob.TeemingCount = count;
                                }
                                else
                                {
                                    mob.Count = count;
                                }

                                mob.Name = mdtNpc?.name ?? mob.Name;

                                npcs[mob.Id] = mob;
                            }

                            continue;
                        }

                        // Bosses
                        {
                            var id = (int) row["Asset"];
                            var type = (int) row["Type"];

                            // If the type is 165, then the asset id refers
                            // to a dungeon encounter id instead of an NPC id
                            var mdtBoss = type == 165 ? GetNpcByEncounterId(mdtEnemies, (int)row["JournalID"]) : GetNpcById(mdtEnemies, id);

                            if (mdtBoss == null)
                            {
                                var displayId = (int?) row["DisplayID"];
                                if (displayId != null && displayId != 0)
                                {
                                    mdtBoss = GetNpcByDisplayId(mdtEnemies, displayId.Value);
                                }

                                if( mdtBoss == null) throw new Exception("Couldn't find boss");
                            }

                            if (!bosses.TryGetValue(id, out var boss))
                            {
                                boss = new KeystoneNpc
                                {
                                    Name = mdtBoss?.name ?? (string)row["Description_lang"],
                                    Count = 0,
                                    Id = mdtBoss.id,
                                    Faction = criteria.Faction
                                };
                            }
                            
                            if (criteria.Faction != 0 && boss.Faction != criteria.Faction) boss.Faction = 0;

                            bosses[boss.Id] = boss;
                        }
                    }
                }

                keystone.Mobs = (from npc in npcs.Values select new DungeonCreature
                {
                    Faction = npc.Faction,
                    Count = npc.Count,
                    TeemingCount = npc.TeemingCount,
                    Id = npc.Id,
                    Name = npc.Name
                }).ToList();

                keystone.Bosses = (from boss in bosses.Values select new DungeonBoss
                {
                    Faction = boss.Faction,
                    Name = boss.Name,
                    //Encounter = boss.EncounterId,
                    Id = boss.Id
                }).ToList();
            }

            await DumpJson("challenge.json", model);
        }

        static dynamic GetNpcById(dynamic npcs, int id)
        {
            foreach (var npc in npcs)
            {
                if (npc.id == id) return npc;
            }

            return null;
        }

        static dynamic GetNpcByEncounterId(dynamic npcs, int id)
        {
            foreach (var npc in npcs)
            {
                if (npc.encounterID == id) return npc;
            }

            return null;
        }
        static dynamic GetNpcByDisplayId(dynamic npcs, int id)
        {
            foreach (var npc in npcs)
            {
                if (npc.displayId == id) return npc;
            }

            return null;
        }


        static LocaleFlags GetLocale(string locale)
        {
            switch (locale.ToLowerInvariant())
            {
                case "en-us":
                    return LocaleFlags.enUS;
                default:
                    throw new ArgumentOutOfRangeException(nameof(locale));
            }
        }

        static IList<JournalSection> BuildSectionTree(IEnumerable<JournalSection> sections, int parent = 0)
        {
            var results = sections.Where(x => x.ParentSectionId == parent)
                .OrderBy(x => x.Order).ToList();

            foreach (var section in results)
            {
                section.Children = BuildSectionTree(sections, section.Id);
            }

            return results;
        }
        
        static async Task ExtractJournal(string localeName = "en-US")
        {
            CASCConfig config = CASCConfig.LoadLocalStorageConfig(@"C:\Games\World of Warcraft", "wow");
            CASCHandler handler = CASCHandler.OpenStorage(config, null);

            handler.Root.LoadListFile(Path.Combine(Environment.CurrentDirectory, "listfile8x.csv"));

            var locale = GetLocale(localeName);

            CASCFolder root = handler.Root.SetFlags(locale);

            handler.Root.MergeInstall(handler.Install);

            var dbfiles = (CASCFolder) root.Entries["dbfilesclient"];
            
            // Apply translations to expansion / tiers
            var tiers = WowExpansion.All;
            foreach (var pair in dbfiles.EnumerateTable("JournalTier", handler))
            {
                var tier = tiers.Single(x => x.Value.TierId == pair.Key);
                tier.Value.Name = pair.Value.GetField<string>(0);
            }

            var instanceTiers = new Dictionary<int, int>();
            {
                foreach (var pair in dbfiles.EnumerateTable("JournalTierXInstance", handler))
                {
                    var tierId = pair.Value.GetField<int>(0);
                    var instanceId = pair.Value.GetField<int>(1);
                    instanceTiers[instanceId] = tierId;
                }
            }

            // All keystone dungeons
            Console.WriteLine();
            Console.WriteLine("Loading challenge dungeons...");
            var keystones = new List<MythicKeystone>();
            foreach (var pair in dbfiles.EnumerateTable("MapChallengeMode", handler))
            {
                var keystone = new MythicKeystone();
                keystone.Id = pair.Key;
                keystone.Slug = GetSlug(pair.Value.GetField<string>(0));
                keystone.Name = pair.Value.GetField<string>(0);
                keystone.MapId = pair.Value.GetField<ushort>(2);
                keystone.Flags = pair.Value.GetField<byte>(3);
                
                // keystone.ExpansionId = pair.Value.GetField<uint>(4);
                keystone.ScenarioId = 0;


                keystone.BronzeTimer = pair.Value.GetField<ushort>(4, 0);
                keystone.SilverTimer = pair.Value.GetField<ushort>(4, 1);
                keystone.GoldTimer = pair.Value.GetField<ushort>(4, 2);

                keystones.Add(keystone);
            }

            // Get all instances
            Console.WriteLine();
            Console.WriteLine("Loading instances...");
            var instances = (from pair in dbfiles.EnumerateTable("JournalInstance", handler)
                where instanceTiers.ContainsKey(pair.Key)
                select new Instance
                {
                    Id = pair.Key,
                    Slug = GetSlug(pair.Value.GetField<string>(0)),
                    Name = pair.Value.GetField<string>(0),
                    Description = pair.Value.GetField<string>(1),
                    MapId = pair.Value.GetField<int>(3),
                    BackgroundImageId = pair.Value.GetField<int>(4),
                    ButtonImageId = pair.Value.GetField<int>(5),
                    ButtonSmallImageId = pair.Value.GetField<int>(6),
                    LoreImageId = pair.Value.GetField<int>(7),
                    Order = pair.Value.GetField<int>(8),
                    Flags = pair.Value.GetField<int>(9),
                    TierId = instanceTiers[pair.Key]
                }).ToList();

            // All encounters
            Console.WriteLine();
            Console.WriteLine("Loading encounters...");
            var encounters = (from pair in dbfiles.EnumerateTable("JournalEncounter", handler)
                select new Encounter
                {
                    Id = pair.Key,
                    Slug = "",
                    Name = pair.Value.GetField<string>(0),
                    Description = pair.Value.GetField<string>(1),
                    MapX = pair.Value.GetField<float>(2, 0),
                    MapY = pair.Value.GetField<float>(2, 1),
                    InstanceId = pair.Value.GetField<int>(3),
                    EncounterId = pair.Value.GetField<int>(4),
                    Order = pair.Value.GetField<int>(5),
                    FirstSectionId = pair.Value.GetField<int>(6),
                    UiMapId = pair.Value.GetField<int>(7),
                    MapDisplayConditionId = pair.Value.GetField<int>(8),
                    Flags = pair.Value.GetField<byte>(9),
                    Difficulty = pair.Value.GetField<byte>(10),
                    Sections = new List<JournalSection>()
                }).ToList();

            // All sections
            Console.WriteLine();
            Console.WriteLine("Loading encounter sections...");
            var sections = (from pair in dbfiles.EnumerateTable("JournalEncounterSection", handler)
                select new JournalSection
                {
                    Id = pair.Key,
                    Name = pair.Value.GetField<string>(0),
                    Description = pair.Value.GetField<string>(1),
                    JournalEncounterId = pair.Value.GetField<ushort>(2),
                    Order = pair.Value.GetField<byte>(3),
                    ParentSectionId = pair.Value.GetField<ushort>(4),
                    FirstChildSectionId = pair.Value.GetField<ushort>(5),
                    NextSiblingSectionId = pair.Value.GetField<ushort>(6),
                    SectionType = pair.Value.GetField<byte>(7), // 3 = overview, 1 = creature, 2 = spell
                    IconCreatureDisplayId = pair.Value.GetField<uint>(8),
                    UiModelSceneId = pair.Value.GetField<int>(9),
                    SpellId = pair.Value.GetField<int>(10),
                    IconFileDataId = pair.Value.GetField<int>(11),
                    Flags = pair.Value.GetField<ushort>(12),
                    IconFlags = pair.Value.GetField<ushort>(13), // 1=tank, 2=dps, 4=healer,
                    DifficultyMask = pair.Value.GetField<byte>(14),
                }).ToList();

            // Build the tree
            Console.WriteLine();
            Console.WriteLine("Building tree..");
            foreach (var instance in instances)
            {
                instance.Encounters = new List<Encounter>();

                foreach (var encounter in encounters.Where(x => x.InstanceId == instance.Id))
                {
                    encounter.Sections = BuildSectionTree(sections.Where(x => x.JournalEncounterId == encounter.Id));
                    instance.Encounters.Add(encounter);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Reading in edits...");

            var existingKeystones = await ReadJson<IList<MythicKeystone>>("keystones.json");
            var existingInstances = await ReadJson<IList<Instance>>("instances.json");

            // Update keystone slugs
            foreach (var existingKeystone in existingKeystones)
            {
                var keystone = keystones.SingleOrDefault(x => x.Id == existingKeystone.Id);
                if (keystone == null) continue;
                keystone.Slug = existingKeystone.Slug;
            }

            // Update instance and encounter slugs
            foreach (var existingInstance in existingInstances)
            {
                var instance = instances.SingleOrDefault(x => x.Id == existingInstance.Id);
                if (instance == null) continue;
                instance.Slug = existingInstance.Slug;

                foreach (var existingEncounter in existingInstance.Encounters)
                {
                    var encounter = instance.Encounters.SingleOrDefault(x => x.Id == existingEncounter.Id);
                    if (encounter == null) continue;
                    encounter.Slug = existingEncounter.Slug;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Exporting..");
            
            await DumpJson("keystones.json", keystones);
            await DumpJson("tiers.json", tiers.Values);
            await DumpJson("instances.json", instances);
            
            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        static async Task DumpJson(string filename, object value)
        {
            var json = JsonConvert.SerializeObject(value, SerializationHelpers.JsonSettings);
            await File.WriteAllTextAsync(Path.Combine(Environment.CurrentDirectory, filename), json, SerializationHelpers.Utf8NoBom);
        }

        internal static async Task<T> ReadJson<T>(string filename)
        {
            var json = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, filename));
            return JsonConvert.DeserializeObject<T>(json, SerializationHelpers.JsonSettings);
        }

        static readonly Regex slugRegex = new Regex("[^a-zA-Z0-9\\-]");

        internal static string GetSlug(string name)
        {
            return slugRegex.Replace(name.ToLowerInvariant().Replace(" ", "-"), "");
        }
    }
}