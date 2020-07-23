#pragma warning disable CS0169

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct HotfixEntryV1 : IHotfixEntry
    {
        public int PushId { get; }
        public int DataSize { get; }
        public uint TableHash { get; }
        public int RecordId { get; }
        public bool IsValid { get; }

        private readonly byte pad1, pad2, pad3;
    }
}
