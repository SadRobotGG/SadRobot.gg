namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    interface IHotfixEntry
    {
        int PushId { get; }
        int DataSize { get; }
        uint TableHash { get; }
        int RecordId { get; }
        bool IsValid { get; }
    }
}