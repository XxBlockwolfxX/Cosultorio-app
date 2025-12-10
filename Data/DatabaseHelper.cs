using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Data
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            "Server=localhost;Database=ConsultorioDentalDB;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
