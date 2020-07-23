namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct HotfixEntryV2 : IHotfixEntry
    {
        public uint Version { get; }
        public int PushId { get; }
        public int DataSize { get; }
        public uint TableHash { get; }
        public int RecordId { get; }
        public bool IsValid { get; }

        private readonly byte pad1, pad2, pad3;
    }
}