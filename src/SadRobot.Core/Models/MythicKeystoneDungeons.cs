using System.Collections.Generic;

namespace SadRobot.Core.Models
{
    public static class MythicKeystoneDungeons
    {
        public static readonly IList<KeystoneAffix> Affixes = CreateAffixes();

        static IList<KeystoneAffix> CreateAffixes()
        {
            return new[]
            {
                new KeystoneAffix{Id = 1, Name = "Overflowing", Level =7, SpellId = 221772},
                new KeystoneAffix{Id = 2, Name = "Skittish", Level = 7},
                new KeystoneAffix{Id = 3, Name = "Volcanic", Level = 7, SpellId = 209862},
                new KeystoneAffix{Id = 4, Name = "Necrotic", Level = 7, SpellId = 209858},
                new KeystoneAffix{Id = 5, Name = "Teeming", Level = 4},
                new KeystoneAffix{Id = 6, Name = "Raging", Level = 4, SpellId = 178658},
                new KeystoneAffix{Id = 7, Name = "Bolstering", Level = 4, SpellId = 209859},
                new KeystoneAffix{Id = 8, Name = "Sanguine", Level = 4, SpellId = 226510},
                new KeystoneAffix{Id = 9, Name = "Tyrannical"},
                new KeystoneAffix{Id = 10, Name = "Fortified"},
                new KeystoneAffix{Id = 11, Name = "Bursting"},
                new KeystoneAffix{Id = 13, Name = "Explosive"},
                new KeystoneAffix{Id = 14, Name = "Quaking"},
                new KeystoneAffix{Id = 15, Name = "Relentless"},
                new KeystoneAffix{Id = 16, Name = "Infested"},
                new KeystoneAffix{Id = 117, Name = "Reaping"},
                new KeystoneAffix{Id = 119, Name = "Beguiling"},
                new KeystoneAffix{Id = 120, Name = "Awakened"},
            };
        }

        public static readonly IList<MythicKeystone> All = CreateDungeons();
        
        public static IList<MythicKeystone> CreateDungeons()
        {
            return new[]
            {
                // new MythicKeystone(200, 1477, 2700, 2160, 1620, 1046, "Halls of Valor", 2235, 46965, "7.0 Dungeon - Valhallas - Challenge"),
                //
                // 1046, Halls of Valor, 2389, 47415, 7.0 Dungeon - Valhallas - Challenge(More Trash)
                // 1166, Black Rook Hold, 2788, 50476, 7.0 Dungeon - Black Rook - Challenge
                // 1166, Black Rook Hold, 2789, 50599, 7.0 Dungeon - Black Rook - Challenge(More Trash)
                // 1169, Eye of Azshara, 2800, 51163, 7.0 Dungeon - Eye of Azshara -Challenge
                // 1169, Eye of Azshara, 2801, 51246, 7.0 Dungeon - Eye of Azshara -Challenge(More Trash)
                // 1172, Darkheart Thicket, 2806, 51687, 7.0 Dungeon - Darkheart Thicket - Challenge
                // 1172, Darkheart Thicket, 2807, 51710, 7.0 Dungeon - Darkheart Thicket - Challenge(More Trash)
                // 1173, Vault of the Wardens, 2810, 51856, 7.0 Dungeon - Vault of the Wardens - Challenge
                // 1173, Vault of the Wardens, 2811, 51880, 7.0 Dungeon - Vault of the Wardens - Challenge(More Trash)
                // 1174, Neltharion's Lair,2813,52252,7.0 Dungeon - Neltharion's Lair -Challenge
                // 1174, Neltharion's Lair,2814,52278,7.0 Dungeon - Neltharion's Lair -Challenge(More Trash)
                // 1175, Maw of Souls, 2815, 52308, 7.0 Dungeon - Maw of Souls -Challenge
                // 1175, Maw of Souls, 2816, 52327, 7.0 Dungeon - Maw of Souls -Challenge(More Trash)
                // 1177, The Arcway, 2819, 52402, 7.0 Dungeon - The Arcway - Challenge
                // 1177, The Arcway, 2820, 52427, 7.0 Dungeon - The Arcway - Challenge(More Trash)
                // 1178, Court of Stars, 2821, 52452, 7.0 Dungeon - Court of Stars -Challenge
                // 1178, Court of Stars, 2822, 52471, 7.0 Dungeon - Court of Stars -Challenge(More Trash)
                // 1308, Upper Return to Karazhan, 3211, 57789,
                // 7.0 Return to Karazhan - Upper Return to Karazhan - Challenge
                // 1308, Upper Return to Karazhan, 3212, 57810,
                // 7.0 Return to Karazhan - Upper Return to Karazhan - Challenge(More Trash)
                // 1309, Lower Return to Karazhan, 3213, 57831, 7.0 Return to Karazhan - Lower Return to Karazhan Challenge
                // 1309, Lower Return to Karazhan, 3214, 57866,
                // 7.0 Return to Karazhan - Lower Return to Karazhan Challenge(More Trash)
                // 1335, Cathedral of Eternal Night, 3291, 58691, 7.0 Dungeon - Cathedral of Eternal Night - Challenge
                // 1335, Cathedral of Eternal Night, 3292, 58715,
                // 7.0 Dungeon - Cathedral of Eternal Night - Challenge(More Trash)
                // 1428, Seat of the Triumvirate, 3536, 60909, 7.0 Dungeon - Seat of the Triumvirate - Challenge
                // 1428, Seat of the Triumvirate, 3537, 60933,
                // 7.0 Dungeon - Seat of the Triumvirate - Challenge(More Trash)
                
                new MythicKeystone(244, 7, 1763, 15, 1800, 1440, 1080, 1528, "Atal'Dazar",
                    new DungeonCriteria(3744, 65600, "8.0 Dungeon - City of Gold Exterior - Challenge", 0),
                    new DungeonCriteria(3745, 65621, "8.0 Dungeon - City of Gold Exterior - Challenge (More Trash)", 5)),

                new MythicKeystone(246, 7, 1771, 23, 2160, 1728, 1296, 1529, "Tol Dagor",
                    new DungeonCriteria(3746, 65724, "8.0 Prison Dungeon -Kul Tiras Prison -Challenge"),
                    new DungeonCriteria(3747, 65745, "8.0 Prison Dungeon -Kul Tiras Prison -Challenge(More Trash)", 5)),

                new MythicKeystone(245, 7, 1754, 16, 1980, 1584,1188,1534, "Freehold",
                    new DungeonCriteria(3751, 65642, "8.0 Dungeon - Outlaw Town - Challenge"),
                    new DungeonCriteria(3752, 65674, "8.0 Dungeon - Outlaw Town - Challenge(More Trash)", 5)),

                new MythicKeystone(247,7,1594,21, 2340,1872,1404,1553, "The MOTHERLODE!!",
                    new DungeonCriteria(3793, 66283, "8.0 Dungeon - Kezan - Challenge"),
                    new DungeonCriteria(3794, 66317, "8.0 Dungeon - Kezan - Challenge(More Trash)", 5)),

                new MythicKeystone(248, 7, 1862, 24, 2340, 1872, 1404, 1554, "Waycrest Manor",
                    new DungeonCriteria(3795, 66371, "8.0 Dungeon - Drustvar Dungeon - Challenge"),
                    new DungeonCriteria(3796, 66408, "8.0 Dungeon - Drustvar Dungeon - Challenge(More Trash)", 5)),

                new MythicKeystone(249, 7, 1762, 17, 2520, 2016, 1512,1560, "Kings' Rest",
                    new DungeonCriteria(3802,66450,"8.0 Dungeon - Kings' Rest - Challenge"),
                    new DungeonCriteria(3803,66485,"8.0 Dungeon - Kings' Rest - Challenge(More Trash)", 5)),

                new MythicKeystone(250,7, 1877, 20, 2160, 1728, 1296,1562, "Temple of Sethraliss",
                    new DungeonCriteria(3806, 66549, "8.0 Dungeon - Temple of Sethraliss -Challenge"),
                    new DungeonCriteria(3807, 66567, "8.0 Dungeon - Temple of Sethraliss -Challenge(More Trash)", 5)),

                new MythicKeystone(251, 7, 1841,22, 1980,1584,1188,1563, "The Underrot",
                    new DungeonCriteria(3808, 66585, "8.0 Dungeon - The Underrot - Challenge"),
                    new DungeonCriteria(3809, 66605, "8.0 Dungeon - The Underrot - Challenge(More Trash)", 5)),

                new MythicKeystone(252,7, 1864,18, 2520,2016,1512,1564, "Shrine of the Storm",
                    new DungeonCriteria(3810, 66625, "8.0 Dungeon - Shrine of the Storm - Challenge"),
                    new DungeonCriteria(3811, 66648, "8.0 Dungeon - Shrine of the Storm - Challenge(More Trash)", 5)),

                new MythicKeystone(353,7,1822,19, 2160,1728,1296,1685, "Siege of Boralus", 
                    new DungeonCriteria(3966, 67227, "Boralus Dungeon -Dungeon Scenario - Challenge", 0, 1),
                    new DungeonCriteria(3967, 67255, "Boralus Dungeon -Dungeon Scenario - Challenge(More Trash)", 5, 1),
                    new DungeonCriteria(3968, 67283, "Boralus Dungeon -Dungeon Scenario - Challenge(Horde)", 0, 2),
                    new DungeonCriteria(3969, 67311, "Boralus Dungeon -Dungeon Scenario - Challenge(Horde)(More Trash)", 5, 2)),

                new MythicKeystone(369, 7, 2097, 25, 2160,1728,1296,1768, "Mechagon Junkyard",
                    new DungeonCriteria(4394, 82551, "8.2 Dungeon - Operation: Mechagon, Junkyard - Challenge"),
                    new DungeonCriteria(4395, 82556, "8.2 Dungeon - Operation: Mechagon, Junkyard - Challenge (More Trash)", 5)),

                new MythicKeystone(370, 7, 2097, 26, 1920, 1536, 1152,1769, "Mechagon Workshop",
                    new DungeonCriteria(4396, 82541, "8.2 Dungeon - Operation: Mechagon, City - Challenge"),
                    new DungeonCriteria(4397, 82546, "8.2 Dungeon - Operation: Mechagon, City - Challenge (More Trash)", 5))
            };
        }

    }

    public class KeystoneAffix
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Level { get;set; }

        public int Expansion { get; set; }

        public int Season { get; set; }

        public int SpellId { get; set; }
    }
}
