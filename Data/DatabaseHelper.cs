using System.Data.SqlClient;

namespace ConsultorioDentalApp.Data
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            "Data Source=BLOCKWOLF;Initial Catalog=ConsultorioDentalDB;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
