using System.Collections.Generic;

namespace SadRobot.Cmd
{
    public class KeystoneNpc
    {
        /// <summary>
        /// Creature id aka NPC id
        /// </summary>
        public int Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public int TeemingCount { get; set; }

        public int Affix { get; set; }

        public int Faction { get; set; }

        public IList<SpellInfo> Spells { get; set; }
    }
}