namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct HotfixEntryV7 : IHotfixEntry
    {
        public int PushId { get; }
        public uint TableHash { get; }
        public int RecordId { get; }
        public int DataSize { get; }
        public bool IsValid { get; }

        private readonly byte pad1, pad2, pad3;
    }
}