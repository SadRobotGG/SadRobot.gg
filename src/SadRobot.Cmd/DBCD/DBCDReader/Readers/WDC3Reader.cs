using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDC3Reader : BaseEncryptionSupportingReader
    {
        private const int HeaderSize = 72;
        private const uint WDC3FmtSig = 0x33434457; // WDC3

        public WDC3Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDC3Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDC3 file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDC3FmtSig)
                    throw new InvalidDataException("WDC3 file is corrupted!");

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                TableHash = reader.ReadUInt32();
                LayoutHash = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                Flags = (DB2Flags)reader.ReadUInt16();
                IdFieldIndex = reader.ReadUInt16();
                int totalFieldsCount = reader.ReadInt32();
                int packedDataOffset = reader.ReadInt32(); // Offset within the field where packed data starts
                int lookupColumnCount = reader.ReadInt32(); // count of lookup columns
                int columnMetaDataSize = reader.ReadInt32(); // 24 * NumFields bytes, describes column bit packing, {ushort recordOffset, ushort size, uint additionalDataSize, uint compressionType, uint packedDataOffset or commonvalue, uint cellSize, uint cardinality}[NumFields], sizeof(DBC2CommonValue) == 8
                int commonDataSize = reader.ReadInt32();
                int palletDataSize = reader.ReadInt32(); // in bytes, sizeof(DBC2PalletValue) == 4
                int sectionsCount = reader.ReadInt32();

                if (sectionsCount == 0 || RecordsCount == 0)
                    return;

                var sections = reader.ReadArray<SectionHeaderWDC3>(sectionsCount).ToList();
                this.m_sections = sections.OfType<IEncryptableDatabaseSection>().ToList();

                // field meta data
                m_meta = reader.ReadArray<FieldMetaData>(FieldsCount);

                // column meta data
                m_columnMeta = reader.ReadArray<ColumnMetaData>(FieldsCount);

                // pallet data
                m_palletData = new Value32[m_columnMeta.Length][];
                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Pallet || m_columnMeta[i].CompressionType == CompressionType.PalletArray)
                    {
                        m_palletData[i] = reader.ReadArray<Value32>((int)m_columnMeta[i].AdditionalDataSize / 4);
                    }
                }

                // common data
                m_commonData = new Dictionary<int, Value32>[m_columnMeta.Length];
                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Common)
                    {
                        var commonValues = new Dictionary<int, Value32>((int)m_columnMeta[i].AdditionalDataSize / 8);
                        m_commonData[i] = commonValues;

                        for (int j = 0; j < m_columnMeta[i].AdditionalDataSize / 8; j++)
                            commonValues[reader.ReadInt32()] = reader.Read<Value32>();
                    }
                }

                int previousStringTableSize = 0, previousRecordCount = 0;
                foreach (var section in sections)
                {
                    reader.BaseStream.Position = section.FileOffset;

                    if (!Flags.HasFlagExt(DB2Flags.Sparse))
                    {
                        // records data
                        recordsData = reader.ReadBytes(section.NumRecords * RecordSize);

                        Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                        // string data
                        if (m_stringsTable == null)
                            m_stringsTable = new Dictionary<long, string>(section.StringTableSize / 0x20);

                        for (int i = 0; i < section.StringTableSize;)
                        {
                            long oldPos = reader.BaseStream.Position;
                            m_stringsTable[i + previousStringTableSize] = reader.ReadCString();
                            i += (int)(reader.BaseStream.Position - oldPos);
                        }

                        previousStringTableSize += section.StringTableSize;
                    }
                    else
                    {
                        // sparse data with inlined strings
                        recordsData = reader.ReadBytes(section.OffsetRecordsEndOffset - section.FileOffset);

                        if (reader.BaseStream.Position != section.OffsetRecordsEndOffset)
                            throw new Exception("reader.BaseStream.Position != section.OffsetRecordsEndOffset");
                    }

                    // skip encrypted sections => has tact key + record data is zero filled
                    if (section.TactKeyLookup != 0 && Array.TrueForAll(recordsData, x => x == 0))
                    {
                        previousRecordCount += section.NumRecords;
                        continue;
                    }

                    // index data
                    m_indexData = reader.ReadArray<int>(section.IndexDataSize / 4);

                    // fix zero-filled index data
                    if (m_indexData.Length > 0 && m_indexData.All(x => x == 0))
                        m_indexData = Enumerable.Range(MinIndex + previousRecordCount, section.NumRecords).ToArray();

                    // duplicate rows data
                    if (section.CopyTableCount > 0)
                    {
                        if (m_copyData == null)
                            m_copyData = new Dictionary<int, int>();

                        for (int i = 0; i < section.CopyTableCount; i++)
                            m_copyData[reader.ReadInt32()] = reader.ReadInt32();
                    }

                    if (section.OffsetMapIDCount > 0)
                    {
                        // HACK unittestsparse is malformed and has sparseIndexData first
                        if (TableHash == 145293629)
                            reader.BaseStream.Position += 4 * section.OffsetMapIDCount;

                        m_sparseEntries = reader.ReadArray<SparseEntry>(section.OffsetMapIDCount).ToList();
                    }

                    // reference data
                    ReferenceData refData = new ReferenceData();
                    if (section.ParentLookupDataSize > 0)
                    {
                        refData.NumRecords = reader.ReadInt32();
                        refData.MinId = reader.ReadInt32();
                        refData.MaxId = reader.ReadInt32();

                        var entries = reader.ReadArray<ReferenceEntry>(refData.NumRecords);
                        for (int i = 0; i < entries.Length; i++)
                            refData.Entries[entries[i].Index] = entries[i].Id;
                    }

                    if (section.OffsetMapIDCount > 0)
                    {
                        int[] sparseIndexData = reader.ReadArray<int>(section.OffsetMapIDCount);

                        if (section.IndexDataSize > 0 && m_indexData.Length != sparseIndexData.Length)
                            throw new Exception("m_indexData.Length != sparseIndexData.Length");

                        m_indexData = sparseIndexData;
                    }

                    int position = 0;
                    for (int i = 0; i < section.NumRecords; i++)
                    {
                        BitReader bitReader = new BitReader(recordsData) { Position = 0 };

                        if (Flags.HasFlagExt(DB2Flags.Sparse))
                        {
                            bitReader.Position = position;
                            position += m_sparseEntries[i].Size * 8;
                        }
                        else
                        {
                            bitReader.Offset = i * RecordSize;
                        }

                        refData.Entries.TryGetValue(i, out int refId);

                        IDBRow rec = new WDC3Row(this, bitReader, section.IndexDataSize != 0 ? m_indexData[i] : -1, refId, i + previousRecordCount);
                        _Records.Add(_Records.Count, rec);
                    }

                    previousRecordCount += section.NumRecords;
                }
            }
        }
    }
}
