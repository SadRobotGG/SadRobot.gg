using System;

namespace SadRobot.Cmd.DBCD.DBCDReader.Attributes
{
    public class IndexAttribute : Attribute
    {
        public readonly bool NonInline;

        public IndexAttribute(bool noninline) => NonInline = noninline;
    }
}
