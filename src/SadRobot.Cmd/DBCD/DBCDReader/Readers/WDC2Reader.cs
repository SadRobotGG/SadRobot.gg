using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDC2Reader : BaseEncryptionSupportingReader
    {
        private const int HeaderSize = 72;
        private const uint WDC2FmtSig = 0x32434457; // WDC2
        private const uint CLS1FmtSig = 0x434C5331; // CLS1

        public WDC2Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDC2Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDC2 file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDC2FmtSig && magic != CLS1FmtSig)
                    throw new InvalidDataException("WDC2 file is corrupted!");

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

                if (sectionsCount > 1)
                    throw new Exception("WDC2 only supports 1 section");

                if (sectionsCount == 0 || RecordsCount == 0)
                    return;

                var sections = reader.ReadArray<SectionHeader>(sectionsCount).ToList();
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

                for (int sectionIndex = 0; sectionIndex < sectionsCount; sectionIndex++)
                {
                    reader.BaseStream.Position = sections[sectionIndex].FileOffset;

                    if (!Flags.HasFlagExt(DB2Flags.Sparse))
                    {
                        // records data
                        recordsData = reader.ReadBytes(sections[sectionIndex].NumRecords * RecordSize);

                        Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                        // string data
                        m_stringsTable = new Dictionary<long, string>(sections[sectionIndex].StringTableSize / 0x20);
                        for (int i = 0; i < sections[sectionIndex].StringTableSize;)
                        {
                            long oldPos = reader.BaseStream.Position;
                            m_stringsTable[oldPos] = reader.ReadCString();
                            i += (int)(reader.BaseStream.Position - oldPos);
                        }
                    }
                    else
                    {
                        // sparse data with inlined strings
                        recordsData = reader.ReadBytes(sections[sectionIndex].SparseTableOffset - sections[sectionIndex].FileOffset);

                        if (reader.BaseStream.Position != sections[sectionIndex].SparseTableOffset)
                            throw new Exception("reader.BaseStream.Position != sections[sectionIndex].SparseTableOffset");

                        int sparseCount = MaxIndex - MinIndex + 1;

                        m_sparseEntries = new List<SparseEntry>(sparseCount);
                        m_copyData = new Dictionary<int, int>(sparseCount);
                        var sparseIdLookup = new Dictionary<uint, int>(sparseCount);

                        for (int i = 0; i < sparseCount; i++)
                        {
                            SparseEntry sparse = reader.Read<SparseEntry>();
                            if (sparse.Offset == 0 || sparse.Size == 0)
                                continue;

                            if (sparseIdLookup.TryGetValue(sparse.Offset, out int copyId))
                            {
                                m_copyData[MinIndex + i] = copyId;
                            }
                            else
                            {
                                m_sparseEntries.Add(sparse);
                                sparseIdLookup.Add(sparse.Offset, MinIndex + i);
                            }
                        }
                    }

                    // index data
                    m_indexData = reader.ReadArray<int>(sections[sectionIndex].IndexDataSize / 4);

                    // fix zero-filled index data
                    if (m_indexData.Length > 0 && m_indexData.All(x => x == 0))
                        m_indexData = Enumerable.Range(MinIndex, MaxIndex - MinIndex + 1).ToArray();

                    // duplicate rows data
                    if (m_copyData == null)
                        m_copyData = new Dictionary<int, int>(sections[sectionIndex].CopyTableSize / 8);

                    for (int i = 0; i < sections[sectionIndex].CopyTableSize / 8; i++)
                        m_copyData[reader.ReadInt32()] = reader.ReadInt32();

                    // reference data
                    ReferenceData refData = new ReferenceData();
                    if (sections[sectionIndex].ParentLookupDataSize > 0)
                    {
                        refData.NumRecords = reader.ReadInt32();
                        refData.MinId = reader.ReadInt32();
                        refData.MaxId = reader.ReadInt32();

                        var entries = reader.ReadArray<ReferenceEntry>(refData.NumRecords);
                        for (int i = 0; i < entries.Length; i++)
                            refData.Entries[entries[i].Index] = entries[i].Id;
                    }

                    int position = 0;
                    for (int i = 0; i < RecordsCount; i++)
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

                        IDBRow rec = new WDC2Row(this, bitReader, sections[sectionIndex].FileOffset, sections[sectionIndex].IndexDataSize != 0 ? m_indexData[i] : -1, refId, i);
                        _Records.Add(i, rec);
                    }
                }
            }
        }
    }
}
