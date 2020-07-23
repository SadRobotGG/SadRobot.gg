using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SadRobot.Cmd.Commandlet;

namespace SadRobot.Cmd
{
    class ScanCache
    {
        const string mergeSql = @"MERGE creature AS target
USING 
(SELECT [ID],[Name_lang],[NameAlt_lang],[Title_lang],[TitleAlt_lang],[Classification],[CreatureType],[CreatureFamily],[StartAnimState]
,[DisplayID_0],[DisplayID_1],[DisplayID_2],[DisplayID_3],[DisplayProbability_0],[DisplayProbability_1],[DisplayProbability_2],[DisplayProbability_3]
,[AlwaysItem_0],[AlwaysItem_1],[AlwaysItem_2]  FROM [creature_insert]) AS source
ON (target.ID = source.ID)
WHEN MATCHED THEN UPDATE SET [Name_lang] = source.Name_lang
	,[NameAlt_lang] = source.NameAlt_lang
	,[Title_lang] = source.Title_lang
	,[TitleAlt_lang] = source.TitleAlt_lang
	,[Classification] = source.Classification
	,[CreatureType] = source.CreatureType
	,[CreatureFamily] = source.CreatureFamily
	,[StartAnimState] = source.StartAnimState
	,[DisplayID_0] = source.DisplayID_0
	,[DisplayID_1] = source.DisplayID_1
	,[DisplayID_2] = source.DisplayID_2
	,[DisplayID_3] = source.DisplayID_3
	,[DisplayProbability_0] = source.DisplayProbability_0
	,[DisplayProbability_1] = source.DisplayProbability_1
	,[DisplayProbability_2] = source.DisplayProbability_2
	,[DisplayProbability_3] = source.DisplayProbability_3
	,[AlwaysItem_0] = source.AlwaysItem_0
	,[AlwaysItem_1] = source.AlwaysItem_1
	,[AlwaysItem_2] = source.AlwaysItem_2
WHEN NOT MATCHED THEN INSERT 
([ID],[Name_lang],[NameAlt_lang],[Title_lang],[TitleAlt_lang],[Classification],[CreatureType],[CreatureFamily],[StartAnimState]
,[DisplayID_0],[DisplayID_1],[DisplayID_2],[DisplayID_3],[DisplayProbability_0],[DisplayProbability_1],[DisplayProbability_2],[DisplayProbability_3]
,[AlwaysItem_0],[AlwaysItem_1],[AlwaysItem_2])
VALUES
(source.ID,source.Name_lang,source.NameAlt_lang,source.Title_lang,source.TitleAlt_lang,source.Classification,source.CreatureType,source.CreatureFamily,source.StartAnimState
,source.DisplayID_0,source.DisplayID_1,source.DisplayID_2,source.DisplayID_3,source.DisplayProbability_0,source.DisplayProbability_1,source.DisplayProbability_2,source.DisplayProbability_3
,source.AlwaysItem_0,source.AlwaysItem_1,source.AlwaysItem_2);";


        private static byte[] BitReverseTable = {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
            0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8,
            0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4,
            0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec,
            0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2,
            0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea,
            0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6,
            0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee,
            0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
            0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9,
            0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5,
            0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed,
            0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3,
            0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb,
            0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7,
            0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef,
            0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };

        public static async Task Execute()
        {
            var cachePath = @"C:\Games\World of Warcraft\_retail_\Cache\WDB\enUS\creaturecache.wdb";
            
            using var reader = new BinaryReader(File.OpenRead(cachePath));

            var signature = new string(reader.ReadChars(4));

            uint build = reader.ReadUInt32();

            var locale = new string(reader.ReadChars(4));

            reader.ReadInt32(); // unk1
            reader.ReadInt32(); // unk2
            reader.ReadInt32(); // unk3

            // Creature table for bulk loading
            var dt = new DataTable("creature");
            dt.PrimaryKey = new [] {dt.Columns.Add("ID", typeof(int))};
            dt.Columns.Add("Name_lang", typeof(string));
            dt.Columns.Add("NameAlt_lang", typeof(string));
            dt.Columns.Add("Title_lang", typeof(string));
            dt.Columns.Add("TitleAlt_lang", typeof(string));
            dt.Columns.Add("Classification", typeof(int));
            dt.Columns.Add("CreatureType", typeof(int));
            dt.Columns.Add("CreatureFamily", typeof(int));
            dt.Columns.Add("StartAnimState", typeof(int));
            dt.Columns.Add("DisplayID_0", typeof(int));
            dt.Columns.Add("DisplayID_1", typeof(int));
            dt.Columns.Add("DisplayID_2", typeof(int));
            dt.Columns.Add("DisplayID_3", typeof(int));
            dt.Columns.Add("DisplayProbability_0", typeof(float));
            dt.Columns.Add("DisplayProbability_1", typeof(float));
            dt.Columns.Add("DisplayProbability_2", typeof(float));
            dt.Columns.Add("DisplayProbability_3", typeof(float));
            dt.Columns.Add("AlwaysItem_0", typeof(int));
            dt.Columns.Add("AlwaysItem_1", typeof(int));
            dt.Columns.Add("AlwaysItem_2", typeof(int));
            
            var eof = reader.BaseStream.Length - 8;
            while (reader.BaseStream.Position < eof)
            {
                var id = reader.ReadInt32();
                var length = reader.ReadInt32();

                if (length <= 0) continue;

                long startPosition = reader.BaseStream.Position;
                long expectedEndPosition = startPosition + length;
                
                var record = new Dictionary<string, object>();

                record["ID"] = id;

                // String lengths are stored as a bit-packed array instead of just using null-terminated strings, fun times
                byte[] lengthBytes = reader.ReadBytes(15);
                BitArray ba = new BitArray(lengthBytes.Select(x => BitReverseTable[x]).ToArray());
                uint titleLength = ba.GetBits(0, 11, true);
                uint titleAltLength = ba.GetBits(11, 11, true);
                uint cursorNameLength = ba.GetBits(22, 6, true);
                record["IsFactionLeader"] = ba.GetBits(28, 1);
                uint name0Length = ba.GetBits(29, 11, true);
                uint name0AltLength = ba.GetBits(40, 11, true);
                uint name1Length = ba.GetBits(51, 11, true);
                uint name1AltLength = ba.GetBits(62, 11, true);
                uint name2Length = ba.GetBits(73, 11, true);
                uint name2AltLength = ba.GetBits(84, 11, true);
                uint name3Length = ba.GetBits(95, 11, true);
                uint name3AltLength = ba.GetBits(106, 11, true);

                record["Name0"] = reader.ReadFixedLengthString(name0Length);
                record["Name0Alt"] = reader.ReadFixedLengthString(name0AltLength);
                record["Name1"] = reader.ReadFixedLengthString(name1Length);
                record["Name1Alt"] = reader.ReadFixedLengthString(name1AltLength);
                record["Name2"] = reader.ReadFixedLengthString(name2Length);
                record["Name2Alt"] = reader.ReadFixedLengthString(name2AltLength);
                record["Name3"] = reader.ReadFixedLengthString(name3Length);
                record["Name3Alt"] = reader.ReadFixedLengthString(name3AltLength);

                record["Flags"] = reader.ReadInt32();
                record["Flags2"] = reader.ReadInt32();
                record["CreatureType"] = reader.ReadInt32();
                record["CreatureFamily"] = reader.ReadInt32();
                record["Classification"] = reader.ReadInt32();
                record["ProxyCreature0_ID"] = reader.ReadInt32();
                record["ProxyCreature1_ID"] = reader.ReadInt32();

                int numCreatureDisplays = reader.ReadInt32();

                record["UNK_BFA_Multiplier"] = reader.ReadSingle();

                for (int i = 0; i < numCreatureDisplays; i++)
                {
                    record[$"CreatureDisplay{i}_ID"] = reader.ReadInt32();
                    record[$"CreatureDisplay{i}_Scale"] = reader.ReadSingle();
                    record[$"CreatureDisplay{i}_Probability"] = reader.ReadSingle();
                }

                record["HPMultiplier"] = reader.ReadSingle();
                record["EnergyMultiplier"] = reader.ReadSingle();

                uint numQuestItems = reader.ReadUInt32();

                record["CreatureMovementInfoID"] = reader.ReadInt32();
                record["RequiredExpansion"] = reader.ReadInt32();
                record["TrackingQuestID"] = reader.ReadInt32();
                record["LEGION_INT_1"] = reader.ReadInt32();
                record["LEGION_INT_2"] = reader.ReadInt32();

                //if (PatchVersion >= 80100)
                {
                    record["28202_INT_1"] = reader.ReadInt32();
                }

                record["Title"] = reader.ReadFixedLengthString(titleLength);
                record["TitleAlt"] = reader.ReadFixedLengthString(titleAltLength);
                    
                // cursorNameLength is set to 1 when it actually means 0, for some reason
                if (cursorNameLength != 1)
                {
                    record["Cursor"] = reader.ReadFixedLengthString(cursorNameLength);
                }
                else
                {
                    record["Cursor"] = "";
                }

                for (uint i = 0; i < numQuestItems; i++)
                {
                    record[$"QuestItem{i}_ID"] = reader.ReadInt32();
                }

                while (reader.BaseStream.Position < expectedEndPosition) reader.ReadByte();

                long endPosition = reader.BaseStream.Position;
                if (endPosition != expectedEndPosition)
                {
                    throw new Exception($"Format change? Read { endPosition - startPosition }, expected { expectedEndPosition - startPosition } for record ID={ id } @ 0x{ (startPosition - 8).ToString("X") }");
                }

                // Upsert the record to the creature table
                var row = dt.NewRow();
                
                row["ID"] = record["ID"];
                row["Name_lang"] = record["Name0"];
                row["NameAlt_lang"] = record["Name0Alt"];
                row["Title_lang"] = record["Title"];
                row["TitleAlt_lang"] = record["TitleAlt"];
                row["Classification"] = record["Classification"];
                row["CreatureType"] = record["CreatureType"];
                row["CreatureFamily"] = record["CreatureFamily"];
                row["StartAnimState"] = 0;

                if (record.ContainsKey("CreatureDisplay0_ID")) row["DisplayID_0"] = record["CreatureDisplay0_ID"]; else row["DisplayID_0"] = 0;
                if(record.ContainsKey("CreatureDisplay1_ID")) row["DisplayID_1"] = record["CreatureDisplay1_ID"]; else row["DisplayID_1"] = 0;
                if (record.ContainsKey("CreatureDisplay2_ID")) row["DisplayID_2"] = record["CreatureDisplay2_ID"]; else row["DisplayID_2"] = 0;
                if (record.ContainsKey("CreatureDisplay3_ID")) row["DisplayID_3"] = record["CreatureDisplay3_ID"]; else row["DisplayID_3"] = 0;

                if (record.ContainsKey("CreatureDisplay0_Probability")) row["DisplayProbability_0"] = record["CreatureDisplay0_Probability"]; else row["DisplayProbability_0"] = 0;
                if (record.ContainsKey("CreatureDisplay1_Probability")) row["DisplayProbability_1"] = record["CreatureDisplay1_Probability"]; else row["DisplayProbability_1"] = 0;
                if (record.ContainsKey("CreatureDisplay2_Probability")) row["DisplayProbability_2"] = record["CreatureDisplay2_Probability"]; else row["DisplayProbability_2"] = 0;
                if (record.ContainsKey("CreatureDisplay3_Probability")) row["DisplayProbability_3"] = record["CreatureDisplay3_Probability"]; else row["DisplayProbability_3"] = 0;

                if (record.ContainsKey("QuestItem0_ID")) row["AlwaysItem_0"] = record["QuestItem0_ID"]; else row["AlwaysItem_0"] = 0;
                if (record.ContainsKey("QuestItem1_ID")) row["AlwaysItem_1"] = record["QuestItem1_ID"]; else row["AlwaysItem_1"] = 0;
                if (record.ContainsKey("QuestItem2_ID")) row["AlwaysItem_2"] = record["QuestItem2_ID"]; else row["AlwaysItem_2"] = 0;

                dt.Rows.Add(row);

                //Console.WriteLine(record["ID"] + ": " + record["Name0"]);
            }

            Console.WriteLine("Inserting to SQL...");

            var connectionString = new SqlConnectionStringBuilder
            {
                InitialCatalog = "wow",
                DataSource = "(local)",
                IntegratedSecurity = true
            }.ToString();

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Bulk import the data
            var bulk = new SqlBulkCopy(connection);
            bulk.DestinationTableName = dt.TableName + "_insert";

            // Clear out the copy table first
            connection.ExecuteNonQuery("TRUNCATE TABLE " + bulk.DestinationTableName);

            // Now upload
            await bulk.WriteToServerAsync(dt);

            // Now merge the tables
            Console.WriteLine("Merging...");
            connection.ExecuteNonQuery(mergeSql);
        }

        static string ReadCString(byte[] bytes)
        {
            var index = bytes.AsSpan().IndexOf((byte)0);
            if (index <= 0) return string.Empty;
            return Encoding.UTF8.GetString(bytes, 0, index);
        }
    }
}