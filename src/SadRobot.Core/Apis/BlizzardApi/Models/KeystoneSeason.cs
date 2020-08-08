using System;
using System.Collections.Generic;

namespace SadRobot.Core.Apis.BlizzardApi.Models
{
    public class KeystoneSeason
    {
        public int Id { get; set; }

        public DateTime? StartTimestamp { get; set; }

        public DateTime? EndTimestamp { get; set; }

        public IList<KeystonePeriod> Periods { get; set; }
    }
}