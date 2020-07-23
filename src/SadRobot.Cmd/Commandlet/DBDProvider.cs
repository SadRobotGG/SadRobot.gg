using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SadRobot.Cmd.DBCD.DBDefsLib;
using SadRobot.Cmd.DBCD.Providers;

namespace SadRobot.Cmd.Commandlet
{
    public class DBDProvider : IDBDProvider
    {
        readonly string directory;
        readonly DBDReader dbdReader;
        Dictionary<string, (string FilePath, Structs.DBDefinition Definition)> definitionLookup;

        public DBDProvider(string directory)
        {
            this.directory = directory;
            dbdReader = new DBDReader();
            LoadDefinitions();
        }

        public int LoadDefinitions()
        {
            Console.WriteLine("Reloading definitions from directory " + directory);

            // lookup needs both filepath and def for DBCD to work
            // also no longer case sensitive now
            var definitionFiles = Directory.EnumerateFiles(directory);
            definitionLookup = definitionFiles.ToDictionary(Path.GetFileNameWithoutExtension, x => (x, dbdReader.Read(x)), StringComparer.OrdinalIgnoreCase);

            Console.WriteLine("Loaded " + definitionLookup.Count + " definitions!");

            return definitionLookup.Count;
        }

        public Stream StreamForTableName(string tableName, string build = null)
        {
            tableName = Path.GetFileNameWithoutExtension(tableName);

            if (definitionLookup.TryGetValue(tableName, out var lookup))
            {
                return new FileStream(lookup.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            throw new FileNotFoundException("Definition for " + tableName);
        }
    }
}