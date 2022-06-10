﻿using System;
using System.IO;
using DBCD.Providers;

namespace SadRobot.Cmd.Commandlet
{
    public class DBCProvider : IDBCProvider
    {
        readonly string directory;

        public DBCProvider(string directory)
        {
            this.directory = directory;
        }

        public Stream StreamForTableName(string tableName, string build)
        {
            string filename = GetDBCFile(tableName, build);
            return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        private string GetDBCFile(string tableName, string build)
        {
            if (tableName.Contains(".")) throw new Exception("Invalid DBC name!");
            //if (string.IsNullOrEmpty(build)) throw new Exception("No build given!");

            // Find file
            string fileName = Path.Combine(directory, tableName + ".db2");

            // if the db2 variant doesn't exist try dbc
            if (!File.Exists(fileName))
            {
                fileName = Path.ChangeExtension(fileName, ".dbc");

                // if the dbc variant doesn't exist throw
                if (!File.Exists(fileName)) throw new FileNotFoundException($"Unable to find {tableName}");
            }

            return fileName;
        }
    }
}