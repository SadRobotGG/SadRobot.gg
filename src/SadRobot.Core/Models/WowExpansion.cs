using System.Collections.Generic;

namespace SadRobot.Core.Models
{
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
}
