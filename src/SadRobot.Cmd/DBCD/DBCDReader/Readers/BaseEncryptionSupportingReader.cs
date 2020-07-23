using System.Collections.Generic;
using System.Linq;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    abstract class BaseEncryptionSupportingReader : BaseReader, IEncryptionSupportingReader
    {
        protected List<IEncryptableDatabaseSection> m_sections;

        List<IEncryptableDatabaseSection> IEncryptionSupportingReader.GetEncryptedSections()
        {
            return this.m_sections.Where(s => s.TactKeyLookup != 0).ToList();
        }
    }
}