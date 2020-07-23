using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CASCLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SadRobot.Core.Models;
using SadRobot.Functions.Casc;

namespace SadRobot.Functions
{
    public static class Function1
    {
        static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Blob("wow")]CloudBlobContainer container,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string localeString = req.Query["locale"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var localeName = localeString ?? "en-US";
            
            CASCConfig config = CASCConfig.LoadLocalStorageConfig(@"C:\Games\World of Warcraft", "wow");
            CASCHandler handler = CASCHandler.OpenStorage(config, null);

            handler.Root.LoadListFile(Path.Combine(Environment.CurrentDirectory, "listfile8x.csv"));

            var locale = WowLocales.Locales.Single(x => x.Name.Equals(localeName, StringComparison.OrdinalIgnoreCase));

            CASCFolder root = handler.Root.SetFlags(locale.Flag);

            handler.Root.MergeInstall(handler.Install);

            var dbfiles = (CASCFolder)root.Entries["dbfilesclient"];

            await container.CreateIfNotExistsAsync();

            // Apply translations to expansion / tiers
            var tiers = WowExpansion.All;
            {
                var entry = dbfiles.GetEntry("journaltier.db2");
                await using var stream = handler.OpenFile(entry.Hash);
                var reader = new WDC3Reader(stream);

                foreach (var pair in reader)
                {
                    var tier = tiers.Single(x => x.Value.TierId == pair.Key);
                    tier.Value.Name = pair.Value.GetField<string>(0);
                }
            }
            await container.GetBlockBlobReference("expansions." + localeName + ".json")
                .UploadTextAsync(JsonConvert.SerializeObject(tiers.Values, serializerSettings));

            var instanceTiers = new Dictionary<int, int>();
            {
                var entry = dbfiles.GetEntry("JournalTierXInstance.db2");
                await using var stream = handler.OpenFile(entry.Hash);
                var reader = new WDC3Reader(stream);
                foreach (var pair in reader)
                {
                    var tierId = pair.Value.GetField<int>(0);
                    var instanceId = pair.Value.GetField<int>(1);
                    instanceTiers[instanceId] = tierId;
                }
            }

            // Get all instances
            var instances = new List<Instance>();
            {
                var entry = dbfiles.GetEntry("JournalInstance.db2");
                await using var stream = handler.OpenFile(entry.Hash);
                var reader = new WDC3Reader(stream);
                foreach (var pair in reader)
                {
                    if (!instanceTiers.ContainsKey(pair.Key)) continue;
                    var tierId = instanceTiers[pair.Key];

                    var instance = new Instance
                    {
                        Id = pair.Key,
                        Name = pair.Value.GetField<string>(0),
                        Description = pair.Value.GetField<string>(1),
                        MapId = pair.Value.GetField<int>(3),
                        BackgroundImageId = pair.Value.GetField<int>(4),
                        ButtonImageId = pair.Value.GetField<int>(5),
                        ButtonSmallImageId = pair.Value.GetField<int>(6),
                        LoreImageId = pair.Value.GetField<int>(7),
                        Order = pair.Value.GetField<int>(8),
                        Flags = pair.Value.GetField<int>(9),
                        TierId = tierId
                    };
                    
                    instances.Add(instance);
                }
            }

            // Save the list of all instances
            await container.GetBlockBlobReference("data/dungeons." + localeName + ".json")
                .UploadTextAsync(JsonConvert.SerializeObject(instances, serializerSettings));

            foreach (var tier in tiers.Values)
            {
                var dungeons = new List<Instance>();

                // Get details and images for each dungeon
                foreach (var instance in instances)
                {
                    // Only encounters for this tier
                    if (instance.TierId != tier.TierId) continue;

                    // World instances are flagged with 0x2
                    if ((instance.Flags & 0x2) == 0x2) continue;

                    // Raids have an order number, dungeons have order of 0 and sorted by name
                    if (instance.Order != 0) continue;

                    dungeons.Add(instance);

                    var blobPath = "static/img/instance/" + instance.Id + "/";

                    // Get the dungeon images
                    using var loreStream = handler.OpenFile(instance.LoreImageId);
                    await container.GetBlockBlobReference(blobPath + "lg." + localeName + ".blp").UploadFromStreamAsync(loreStream);

                    using var bg = handler.OpenFile(instance.BackgroundImageId);
                    await container.GetBlockBlobReference(blobPath + "xl." + localeName + ".blp").UploadFromStreamAsync(bg);

                    using var btnLarge = handler.OpenFile(instance.ButtonImageId);
                    await container.GetBlockBlobReference(blobPath + "sm." + localeName + ".blp").UploadFromStreamAsync(btnLarge);

                    using var btnSmall = handler.OpenFile(instance.ButtonImageId);
                    await container.GetBlockBlobReference(blobPath + "xs." + localeName + ".blp").UploadFromStreamAsync(btnSmall);

                    var encounters = new List<Encounter>();
                    
                    foreach (var encounter in dbfiles.EnumerateTable("JournalEncounter", handler))
                    {
                        var instanceId = encounter.Value.GetField<int>(3);
                        if (instanceId != instance.Id) continue;

                        var journalEncounter = new Encounter
                        {
                            Id = encounter.Key,
                            Name = encounter.Value.GetField<string>(0),
                            Description = encounter.Value.GetField<string>(1),
                            MapX = encounter.Value.GetField<float>(2, 0),
                            MapY = encounter.Value.GetField<float>(2, 1),
                            InstanceId = instanceId,
                            EncounterId = encounter.Value.GetField<int>(4),
                            Order = encounter.Value.GetField<int>(5),
                            FirstSectionId = encounter.Value.GetField<int>(6),
                            UiMapId = encounter.Value.GetField<int>(7),
                            MapDisplayConditionId = encounter.Value.GetField<int>(8),
                            Flags = encounter.Value.GetField<byte>(9),
                            Difficulty = encounter.Value.GetField<byte>(10),
                            Sections = new List<JournalSection>()
                        };

                        var sections = new Dictionary<int, JournalSection>();

                        foreach (var encounterSection in dbfiles.EnumerateTable("JournalEncounterSection", handler))
                        {
                            var section = new JournalSection
                            {
                                Id = encounterSection.Key,
                                Name = encounterSection.Value.GetField<string>(0),
                                Description = encounterSection.Value.GetField<string>(1),
                                JournalEncounterId = encounterSection.Value.GetField<ushort>(2),
                                Order = encounterSection.Value.GetField<byte>(3),
                                ParentSectionId = encounterSection.Value.GetField<ushort>(4),
                                FirstChildSectionId = encounterSection.Value.GetField<ushort>(5),
                                NextSiblingSectionId = encounterSection.Value.GetField<ushort>(6),
                                SectionType = encounterSection.Value.GetField<byte>(7), // 3 = overview, 1 = creature, 2 = spell
                                IconCreatureDisplayId = encounterSection.Value.GetField<uint>(8),
                                UiModelSceneId = encounterSection.Value.GetField<int>(9),
                                SpellId = encounterSection.Value.GetField<int>(10),
                                IconFileDataId = encounterSection.Value.GetField<int>(11),
                                Flags = encounterSection.Value.GetField<ushort>(12),
                                IconFlags = encounterSection.Value.GetField<ushort>(13), // 1=tank, 2=dps, 4=healer,
                                DifficultyMask = encounterSection.Value.GetField<byte>(14),
                            };

                            if (section.JournalEncounterId != journalEncounter.Id) continue;

                            sections[section.Id] = section;
                        }

                        journalEncounter.Sections = BuildSectionTree(sections);
                        
                        encounters.Add(journalEncounter);
                    }

                    await container.GetBlockBlobReference("data/dungeon/" + instance.Id + "/encounters." + localeName + ".json")
                        .UploadTextAsync(JsonConvert.SerializeObject(encounters, serializerSettings));
                }

                // Save the list of dungeons for this tier
                await container.GetBlockBlobReference("data/" + tier.Id + "/dungeons." + localeName + ".json")
                    .UploadTextAsync(JsonConvert.SerializeObject(dungeons, serializerSettings));
            }
            
            return new OkObjectResult(tiers);
        }

        static IList<JournalSection> BuildSectionTree(Dictionary<int, JournalSection> sections, int parent = 0)
        {
            var results = sections.Values.Where(x => x.ParentSectionId == parent)
                .OrderBy(x => x.Order).ToList();

            foreach (var section in results)
            {
                section.Children = BuildSectionTree(sections, section.Id);
            }

            return results;
        }
    }

    // string Title
    // string BodyText
    // uint16 JournalEncounterID
    // uint8 OrderIndex
    // uint16 Parent SectionID
    // uint16 FirstChild SectionID
    // uint16 NextSibling SectionID
    // uint8 Type
    // uint32 Icon CreatureDisplayInfoID
    // int32 UIModelSceneID
    // int32 SpellID
    // int32 Icon FileDataID
    // uint16 Flags
    // uint16 IconFlags
    // int8 DifficultyMask



    public static class WowLocales
    {
        public static readonly IList<WowLocale> Locales = new List<WowLocale>
        {
            // Americas
            new WowLocale("en-US", LocaleFlags.enUS),
            new WowLocale("es-MX", LocaleFlags.esMX),
            new WowLocale("pt-BR", LocaleFlags.ptBR),
            // Europe
            new WowLocale("en-GB", LocaleFlags.enGB),
            new WowLocale("es-ES", LocaleFlags.esES),
            new WowLocale("fr-FR", LocaleFlags.frFR),
            new WowLocale("ru-RU", LocaleFlags.ruRU),
            new WowLocale("de-DE", LocaleFlags.deDE),
            new WowLocale("pt-PT", LocaleFlags.ptPT),
            new WowLocale("it-IT", LocaleFlags.itIT),
            // Korea
            new WowLocale("kr-KR", LocaleFlags.koKR),
            // Taiwan
            new WowLocale("zh-TW", LocaleFlags.zhTW),
            // China
            new WowLocale("zh-CN", LocaleFlags.zhCN)
        };
    }

    public class WowLocale
    {
        public WowLocale(string name, LocaleFlags flag)
        {
            Name = name;
            Flag = flag;
            Culture = CultureInfo.GetCultureInfo(name);
        }

        public string Name { get; set; }

        public LocaleFlags Flag { get; set; }

        public CultureInfo Culture { get; set; }
    }

    public static class CascExtensions
    {
        public static IEnumerable<KeyValuePair<int, WDC3Row>> EnumerateTable(this CASCFolder folder, string name, CASCHandler handler)
        {
            var entry = folder.GetEntry(name + ".db2");
            using var stream = handler.OpenFile(entry.Hash);
            var reader = new WDC3Reader(stream);
            foreach (var pair in reader)
            {
                yield return pair;
            }
        }
    }
}
