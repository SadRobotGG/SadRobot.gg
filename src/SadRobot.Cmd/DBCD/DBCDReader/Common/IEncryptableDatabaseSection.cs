namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    public interface IEncryptableDatabaseSection
    {
        ulong TactKeyLookup { get; }
        int NumRecords { get; }
    }
}