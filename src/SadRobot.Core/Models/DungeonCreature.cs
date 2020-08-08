namespace SadRobot.Core.Models
{
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
}