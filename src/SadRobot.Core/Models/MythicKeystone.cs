using System.Collections.Generic;

namespace SadRobot.Core.Models
{
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
}