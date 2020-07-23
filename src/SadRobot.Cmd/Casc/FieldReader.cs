using System;
using System.Collections.Generic;

namespace SadRobot.Cmd.Casc
{
    public class FieldReader
    {
        public static T GetFieldValue<T>(int id, BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData) where T : unmanaged
        {
            switch (columnMeta.CompressionType)
            {
                case CompressionType.None:
                    int bitSize = 32 - fieldMeta.Bits;
                    if (bitSize > 0)
                        return r.Read<T>(bitSize);
                    else
                        return r.Read<T>(columnMeta.Immediate.BitWidth);
                case CompressionType.Immediate:
                    return r.Read<T>(columnMeta.Immediate.BitWidth);
                case CompressionType.SignedImmediate:
                    return r.ReadSigned<T>(columnMeta.Immediate.BitWidth);
                case CompressionType.Common:
                    if (commonData.TryGetValue(id, out Value32 val))
                        return val.As<T>();
                    else
                        return columnMeta.Common.DefaultValue.As<T>();
                case CompressionType.Pallet:
                    uint palletIndex = r.Read<uint>(columnMeta.Pallet.BitWidth);
                    return palletData[palletIndex].As<T>();
            }
            throw new Exception(string.Format("Unexpected compression type {0}", columnMeta.CompressionType));
        }

        public static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, int arraySize) where T : unmanaged
        {
            switch (columnMeta.CompressionType)
            {
                case CompressionType.None:
                    int bitSize = 32 - fieldMeta.Bits;

                    T[] arr1 = new T[arraySize];

                    for (int i = 0; i < arr1.Length; i++)
                    {
                        if (bitSize > 0)
                            arr1[i] = r.Read<T>(bitSize);
                        else
                            arr1[i] = r.Read<T>(columnMeta.Immediate.BitWidth);
                    }

                    return arr1;
                case CompressionType.Immediate:
                    T[] arr2 = new T[arraySize];

                    for (int i = 0; i < arr2.Length; i++)
                        arr2[i] = r.Read<T>(columnMeta.Immediate.BitWidth);

                    return arr2;
                case CompressionType.SignedImmediate:
                    T[] arr3 = new T[arraySize];

                    for (int i = 0; i < arr3.Length; i++)
                        arr3[i] = r.ReadSigned<T>(columnMeta.Immediate.BitWidth);

                    return arr3;
                case CompressionType.PalletArray:
                    int cardinality = columnMeta.Pallet.Cardinality;

                    // if (arraySize != cardinality)
                    //     throw new Exception("Struct missmatch for pallet array field?");

                    uint palletArrayIndex = r.Read<uint>(columnMeta.Pallet.BitWidth);

                    T[] arr4 = new T[cardinality];

                    for (int i = 0; i < arr4.Length; i++)
                        arr4[i] = palletData[i + cardinality * (int)palletArrayIndex].As<T>();

                    return arr4;
            }
            throw new Exception(string.Format("Unexpected compression type {0}", columnMeta.CompressionType));
        }

        public static string[] GetFieldValueStringsArray(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<int, Value32> commonData, Dictionary<long, string> stringsTable, bool isSparse, int recordOffset, int arraySize)
        {
            string[] array = new string[arraySize];

            if (isSparse)
            {
                for (int i = 0; i < array.Length; i++)
                    array[i] = r.ReadCString();
            }
            else
            {
                var pos = recordOffset + (r.Position >> 3);

                int[] strIdx = GetFieldValueArray<int>(r, fieldMeta, columnMeta, palletData, commonData, arraySize);

                for (int i = 0; i < array.Length; i++)
                    array[i] = stringsTable[pos + i * 4 + strIdx[i]];
            }

            return array;
        }
    }
}