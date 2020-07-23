using System.Collections.Generic;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    public interface IEncryptionSupportingReader
    {
        List<IEncryptableDatabaseSection> GetEncryptedSections();
    }
}