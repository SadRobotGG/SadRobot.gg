using System.Collections.Generic;

namespace SadRobot.Core.Apis.BlizzardApi.Models
{
    public class KeystoneSeasons
    {
        public IList<KeystoneSeason> Seasons { get; set; }

        public KeystoneSeason CurrentSeason { get; set; }
    }
}