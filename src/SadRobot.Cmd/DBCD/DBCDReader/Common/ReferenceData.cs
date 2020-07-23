using System.Collections.Generic;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    class ReferenceData
    {
        public int NumRecords { get; set; }
        public int MinId { get; set; }
        public int MaxId { get; set; }
        public Dictionary<int, int> Entries { get; set; } = new Dictionary<int, int>();
    }
}