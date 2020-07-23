using System.Data.SqlClient;

namespace SadRobot.Cmd
{
    public static class Configuration
    {
        public static readonly string ConnectionString = new SqlConnectionStringBuilder
        {
            InitialCatalog = "wow",
            DataSource = "(local)",
            IntegratedSecurity = true
        }.ToString();
    }
}