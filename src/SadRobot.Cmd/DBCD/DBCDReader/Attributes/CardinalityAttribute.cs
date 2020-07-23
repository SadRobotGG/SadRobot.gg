using System;

namespace SadRobot.Cmd.DBCD.DBCDReader.Attributes
{
    public class CardinalityAttribute : Attribute
    {
        public readonly int Count;

        public CardinalityAttribute(int count) => Count = count;
    }
}
