using System.Collections.Generic;

namespace SadRobot.Core.Models
{
    public enum WowInternalLocales
    {
        EnglishUs = 0,
        Korean = 1,
        French = 2,
        German = 3,
        Chinese = 4,
        Spanish = 5,
        Taiwanese = 6,
        Mexican = 7,
        Russian = 8,
        BrazilianPortugese = 10,
        Italian = 11
    }

    public class WowExpansion
    {
        public static readonly WowExpansion Classic = new WowExpansion(0, 68, "Classic", "classic");
        public static readonly WowExpansion BurningCrusade = new WowExpansion(1, 70, "Burning Crusade", "bc");
        public static readonly WowExpansion Wrath = new WowExpansion(2, 72, "Wrath of the Lich King", "wotlk");
        public static readonly WowExpansion Cataclysm = new WowExpansion(3, 73, "Cataclysm", "cata");
        public static readonly WowExpansion Pandaria = new WowExpansion(4, 74, "Mists of Pandaria", "mop");
        public static readonly WowExpansion Draenor = new WowExpansion(5, 124, "Warlords of Draenor", "wod");
        public static readonly WowExpansion Legion = new WowExpansion(6, 395, "Legion", "legion");
        public static readonly WowExpansion Bfa = new WowExpansion(7, 396, "Battle for Azeroth", "bfa");
        public static readonly WowExpansion Shadowlands = new WowExpansion(8, 499, "Shadowlands", "sl");

        public static readonly IDictionary<int, WowExpansion> All = new Dictionary<int, WowExpansion>
        {
            {Classic.Id, Classic},
            {BurningCrusade.Id, BurningCrusade},
            {Wrath.Id, Wrath},
            {Cataclysm.Id, Cataclysm},
            {Pandaria.Id, Pandaria},
            {Draenor.Id, Draenor},
            {Legion.Id, Legion},
            {Bfa.Id, Bfa},
            {Shadowlands.Id, Shadowlands}
        };

        public WowExpansion() { }

        WowExpansion(int id, int tierId, string name, string slug)
        {
            Id = id;
            TierId = tierId;
            Name = name;
            Slug = slug;
        }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tier Id in the Dungeon Journal
        /// </summary>
        public int TierId { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL slug for short, friendly URLs
        /// </summary>
        public string Slug { get; set; }
    }

    public class JournalSection
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        public ushort JournalEncounterId { get; set; }
        public byte Order { get; set; }
        public ushort ParentSectionId { get; set; }
        public ushort FirstChildSectionId { get; set; }
        public ushort NextSiblingSectionId { get; set; }
        public byte SectionType { get; set; }
        public uint IconCreatureDisplayId { get; set; }
        public int UiModelSceneId { get; set; }
        public int SpellId { get; set; }
        public int IconFileDataId { get; set; }
        public ushort Flags { get; set; }
        public ushort IconFlags { get; set; }
        public byte DifficultyMask { get; set; }
        public IList<JournalSection> Children { get; set; }
    }



    public class Encounter
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float MapX { get; set; }
        public float MapY { get; set; }
        public int InstanceId { get; set; }
        public int EncounterId { get; set; }
        public int Order { get; set; }
        public int FirstSectionId { get; set; }
        public int UiMapId { get; set; }
        public int MapDisplayConditionId { get; set; }
        public int Flags { get; set; }
        public int Difficulty { get; set; }
        public IList<JournalSection> Sections { get; set; }
    }

    public class Instance
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MapId { get; set; }
        public int BackgroundImageId { get; set; }
        public int ButtonImageId { get; set; }
        public int ButtonSmallImageId { get; set; }
        public int LoreImageId { get; set; }
        public int Order { get; set; }
        public int TierId { get; set; }
        public int Flags { get; set; }
        public IList<Encounter> Encounters { get; set; }
    }

    public class MythicKeystone
    {
        public MythicKeystone()
        {
        }

        public MythicKeystone(int id, int expansion, int mapId, int mdtId, int bronze, int silver, int gold, int scenarioId, string name, params DungeonCriteria[] criteria)
        {
            Id = id;
            Expansion = expansion;
            MapId = mapId;
            MdtId = mdtId;
            BronzeTimer = bronze;
            SilverTimer = silver;
            GoldTimer = gold;
            ScenarioId = scenarioId;
            Name = name;
            Critera = new List<DungeonCriteria>(criteria);
        }

        public int MdtId { get; set; }

        public int Id { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }
        
        public int Expansion { get; set; }

        public int MapId { get; set; }

        public int ScenarioId { get; set; }

        /// <summary>
        /// The criteria is a set mobs and objectives to complete the keystone.
        /// <remarks>
        /// The criteria can change depending on faction (only in the case of Boralus), the keystone (i.e. Kara or Mechagon are one map split into two dungeons),
        /// or affix (i.e. Teeming forces extra count, and changes the count weighting of mobs).
        /// </remarks>
        /// </summary>
        public IList<DungeonCriteria> Critera { get; set; }
        
        /// <summary>
        /// The list of available mobs in the dungeon, including any adjustments
        /// for affix (Teeming), faction (Boralus), seasonal affix (e.g. Awakened mini bosses)
        /// </summary>
        public IList<DungeonCreature> Mobs { get; set; }

        public int BronzeTimer { get; set; }

        public int SilverTimer { get; set; }

        public int GoldTimer { get; set; }
        public IList<DungeonBoss> Bosses { get; set; }
    }

    public class DungeonBoss
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Encounter { get; set; }

        public int Faction { get; set; }
    }

    public class DungeonCreature
    {
        /// <summary>
        /// The NPC id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Set to 1 for Alliance or 2 for Horde, or 0 (default) for all factions
        /// </summary>
        public int Faction { get; set; }
        
        public int Count { get; set; }

        public int TeemingCount { get; set; }
        public string Name { get; set; }
    }

    public class DungeonCriteria
    {
        public DungeonCriteria(int scenarioStepId, int criteriaTreeId, string name, int affix = 0, int faction = 0)
        {
            ScenarioStepId = scenarioStepId;
            CriteriaTreeId = criteriaTreeId;
            Name = name;
            Affix = affix;
            Faction = faction;
        }

        /// <summary>
        /// This maps to the ID in ScenarioStep
        /// </summary>
        public int ScenarioStepId { get; set; }

        /// <summary>
        /// This maps to ID in CriteriaTree, which we can traverse to find all the instance mobs and bosses
        /// </summary>
        public int CriteriaTreeId { get; set; }

        /// <summary>
        /// The criteria set name e.g. "8.0 Dungeon - The Underrot - Challenge"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The affix this criteria maps to; 0 normally, or 5 for Teeming
        /// </summary>
        public int Affix { get; set; }

        /// <summary>
        /// The faction this criteria maps to; 0 for both (default), 1 for Alliance and 2 for Horde. Currently
        /// only applicable for Siege of Boralus.
        /// </summary>
        public int Faction { get; set; }

        /// <summary>
        /// The enemy forces count required to complete
        /// </summary>
        public int EnemyForcesCount { get; set; }
    }
}
