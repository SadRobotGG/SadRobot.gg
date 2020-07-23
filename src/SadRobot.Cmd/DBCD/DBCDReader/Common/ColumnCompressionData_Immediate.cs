namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct ColumnCompressionData_Immediate
    {
        public int BitOffset;
        public int BitWidth;
        public int Flags; // 0x1 signed
    }
}