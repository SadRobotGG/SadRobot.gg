using System.Runtime.InteropServices;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct SectionHeaderWDC3 : IEncryptableDatabaseSection
    {
        public ulong TactKeyLookup;
        public int FileOffset;
        public int NumRecords;
        public int StringTableSize;
        public int OffsetRecordsEndOffset; // CatalogDataOffset, absolute value, {uint offset, ushort size}[MaxId - MinId + 1]
        public int IndexDataSize; // int indexData[IndexDataSize / 4]
        public int ParentLookupDataSize; // uint NumRecords, uint minId, uint maxId, {uint id, uint index}[NumRecords], questionable usefulness...
        public int OffsetMapIDCount;
        public int CopyTableCount;

        ulong IEncryptableDatabaseSection.TactKeyLookup => this.TactKeyLookup;
        int IEncryptableDatabaseSection.NumRecords => this.NumRecords;
    }
}