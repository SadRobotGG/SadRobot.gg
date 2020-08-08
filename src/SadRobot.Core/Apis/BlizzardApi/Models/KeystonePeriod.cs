using System;

namespace SadRobot.Core.Apis.BlizzardApi.Models
{
    public class KeystonePeriod
    {
        public int Id { get; set; }

        public DateTime? StartTimestamp { get; set; }

        public DateTime? EndTimestamp { get; set; }
    }
}
