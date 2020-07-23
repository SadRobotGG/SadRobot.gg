using System.Collections.Generic;

namespace SadRobot.Cmd.Casc
{
    public class ReferenceData
    {
        public int NumRecords { get; set; }
        public int MinId { get; set; }
        public int MaxId { get; set; }
        public Dictionary<int, int> Entries { get; set; }
    }
}