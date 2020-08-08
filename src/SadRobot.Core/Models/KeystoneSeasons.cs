using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SadRobot.Core.Models
{
    public class KeystoneSeasons
    {
        static KeystoneSeasons()
        {
            Season720 = new KeystoneSeason(720, WowExpansion.Legion, "Legion 7.2.0", "season-7.2.0", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season725 = new KeystoneSeason(725, WowExpansion.Legion, "Legion 7.2.5", "season-7.2.5", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season730 = new KeystoneSeason(730, WowExpansion.Legion, "Legion 7.3.0", "season-7.3.0", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season732 = new KeystoneSeason(732, WowExpansion.Legion, "Legion 7.3.2", "season-7.3.2", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season735 = new KeystoneSeason(735, WowExpansion.Legion, "Legion 7.3.5", "season-post-legion", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season800 = new KeystoneSeason(800, WowExpansion.Bfa, "BfA Pre-Season", "season-pre-bfa", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season801 = new KeystoneSeason(801, WowExpansion.Bfa, "BfA Season 1", "season-bfa-1", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season810 = new KeystoneSeason(810, WowExpansion.Bfa, "BfA Season 2", "season-bfa-2", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season815 = new KeystoneSeason(815, WowExpansion.Bfa, "BfA Season 2 Post-Season", "season-bfa-2-post", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season820 = new KeystoneSeason(820, WowExpansion.Bfa, "BfA Season 3", "season-bfa-3", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season825 = new KeystoneSeason(825, WowExpansion.Bfa, "BfA Season 3 Post-Season", "season-bfa-3-post", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");
            Season830 = new KeystoneSeason(830, WowExpansion.Bfa, "BfA Season 4", "season-bfa-4", "2017-03-28 15:00:00Z", "2017-06-13 14:59:59Z");

            Values = new List<KeystoneSeason>{ Season720, Season725,Season730,Season732,Season735, Season800, Season801, Season810, Season815, Season820,  Season825, Season830};

            All = Values.ToDictionary(season => season.Id);
        }

        public static IList<KeystoneSeason> Values { get; set; }

        public static KeystoneSeason Season720 { get; set; }
        public static KeystoneSeason Season725 { get; set; }
        public static KeystoneSeason Season730 { get; set; }
        public static KeystoneSeason Season732 { get; set; }
        public static KeystoneSeason Season735 { get; set; }
        public static KeystoneSeason Season800 { get; set; }
        public static KeystoneSeason Season801 { get; set; }
        public static KeystoneSeason Season810 { get; set; }
        public static KeystoneSeason Season815 { get; set; }
        public static KeystoneSeason Season820 { get; set; }
        public static KeystoneSeason Season825 { get; set; }
        public static KeystoneSeason Season830 { get; set; }
        public static IDictionary<int, KeystoneSeason> All { get; private set; }
    }

    public class KeystoneSeason
    {
        public KeystoneSeason(int id, WowExpansion expansion, string name, string rio, string startTimestamp, string endTimestamp = null)
        {
            Id = id;
            Expansion = expansion;
            Name = name;
            RioSlug = rio;
            StartTime = DateTime.ParseExact(startTimestamp, "u", CultureInfo.InvariantCulture.DateTimeFormat);
            
            if (endTimestamp != null)
            {
                EndTime = DateTime.ParseExact(endTimestamp, "u", CultureInfo.InvariantCulture.DateTimeFormat);
            }
        }

        public int Id { get; set; }

        public WowExpansion Expansion { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string RioSlug { get; set; }

        /// <summary>
        /// The season start time is in UTC and refers to the start time for the North American realms,
        /// with the start time in other realms calculated using the time zone offsets.
        /// </summary>
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }

    public enum KeystoneSeasonFlag
    {
        None = 0,

        Legion_7_2_0,

        Legion_7_2_5,

        Legion_7_3_0,

        Legion_7_3_2,

        Legion_Post_Season,

        Bfa_Pre_Season,

        Bfa_Season_1,

        Bfa_Season_2,

        Bfa_Season_3,

        Bfa_Season_4
    }
}
