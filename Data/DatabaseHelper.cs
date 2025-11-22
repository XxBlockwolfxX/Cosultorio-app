using System.Data.SqlClient;

namespace ConsultorioDentalApp.Data
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            "Server=BLOCKWOLF;Database=ConsultorioDentalDB;User Id=da;Password=123;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
