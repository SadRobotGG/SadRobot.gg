using System;
using System.Collections.Generic;

namespace SadRobot.Cmd.Casc
{
    public abstract class ClientDBRow : IDB2Row
    {
        public abstract int GetId();

        public void Read<T>(FieldCache[] fields, T entry, BitReader r, int recordOffset, Dictionary<long, string> stringsTable, FieldMetaData[] fieldMeta, ColumnMetaData[] columnMeta, Value32[][] palletData, Dictionary<int, Value32>[] commonData, int id, int refId, bool isSparse = false) where T : ClientDBRow
        {
            int fieldIndex = 0;

            foreach (var f in fields)
            {
                if (f.IsIndex && id != -1)
                {
                    ((FieldCache<T, int>)f).Setter(entry, id);
                    continue;
                }

                if (fieldIndex >= fieldMeta.Length)
                {
                    if (refId != -1)
                        ((FieldCache<T, int>)f).Setter(entry, refId);
                    continue;
                }

                if (f.IsArray)
                {
                    switch (f)
                    {
                        case FieldCache<T, int[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<int>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, uint[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<uint>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, byte[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<byte>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, sbyte[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<sbyte>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, short[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<short>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, ushort[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<ushort>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, float[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<float>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, long[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<long>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, ulong[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueArray<ulong>(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], c1.ArraySize));
                            break;
                        case FieldCache<T, string[]> c1:
                            c1.Setter(entry, FieldReader.GetFieldValueStringsArray(r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], stringsTable, isSparse, recordOffset, c1.ArraySize));
                            break;
                        default:
                            throw new Exception($"Unhandled DbcTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                    }
                }
                else
                {
                    switch (f)
                    {
                        case FieldCache<T, int> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<int>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, uint> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<uint>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, byte> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<byte>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, sbyte> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<sbyte>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, short> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<short>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, ushort> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<ushort>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, float> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<float>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, long> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<long>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, ulong> c1:
                            c1.Setter(entry, FieldReader.GetFieldValue<ulong>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex]));
                            break;
                        case FieldCache<T, string> c1:
                            c1.Setter(entry, isSparse ? r.ReadCString() : stringsTable[(recordOffset + (r.Position >> 3)) + FieldReader.GetFieldValue<int>(GetId(), r, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex])]);
                            break;
                        default:
                            throw new Exception($"Unhandled DbcTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                    }
                }

                fieldIndex++;
            }
        }

        public void SetId(int id) => throw new InvalidOperationException(nameof(SetId));
        public T GetField<T>(int fieldIndex, int arrayIndex = -1) => throw new InvalidOperationException(nameof(GetField));
        public IDB2Row Clone() => (IDB2Row)MemberwiseClone();
    }
}