using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SadRobot.Cmd.Commandlet
{
    public static class SqlExtensions
    {
        public static IDictionary<string, object> ReadRow(this DbDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
        }

        public static DbCommand CreateCommand(this DbConnection connection, string query, params KeyValuePair<string, object>[] parameters)
        {
            return connection.CreateCommand().SetParameters(query, parameters);
        }

        public static DbCommand SetParameters(this DbCommand command, string query, params KeyValuePair<string, object>[] parameters)
        {
            command.CommandText = query;
            foreach (var pair in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.Value = pair.Value;
                parameter.ParameterName = pair.Key;
                command.Parameters.Add(parameter);
            }
            return command;
        }

        public static int ExecuteNonQuery(this DbConnection connection, string query)
        {
            using var command = connection.CreateCommand(query);
            return command.ExecuteNonQuery();
        }

        public static IEnumerable<IDictionary<string, object>> ExecuteQuery(this DbConnection connection, string query, params KeyValuePair<string, object>[] parameters)
        {
            using var command = connection.CreateCommand(query, parameters);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return reader.ReadRow();
            }
        }
    }
}