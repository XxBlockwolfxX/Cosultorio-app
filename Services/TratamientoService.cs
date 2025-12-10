using System.Data;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Services
{
    public class TratamientoService
    {
        /// <summary>
        /// Devuelve historial del diente seleccionado para un paciente.
        /// </summary>
        public static DataTable ObtenerPorDiente(int pacienteId, string diente)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT 
                        Fecha AS Fecha,
                        Actividad,
                        Estado,
                        IFNULL(Observacion, '') AS Observaciones,
                        IFNULL(Doctor, '') AS Doctor,
                        IFNULL(FechaFin, '') AS FechaFin
                    FROM Procedimiento
                    WHERE PacienteId = @PacienteId
                      AND Actividad LIKE CONCAT('%', @Diente, '%')
                    ORDER BY Fecha DESC;
                ";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                    cmd.Parameters.AddWithValue("@Diente", diente);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    return dt;
                }
            }
        }
    }
}
