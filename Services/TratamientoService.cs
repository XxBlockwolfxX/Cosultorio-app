using System.Data;
using System.Data.SqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Services
{
    public class TratamientoService
    {
        /// <summary>
        /// Devuelve el historial (Fecha, Actividad, Estado, Observaciones, Doctor, FechaFin)
        /// del diente seleccionado para el paciente indicado.
        /// </summary>
        public static DataTable ObtenerPorDiente(int pacienteId, string diente)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
SELECT
    Fecha AS Fecha,
    Actividad,
    Estado,
    ISNULL(Observacion, '') AS Observaciones,
    ISNULL(Doctor, '') AS Doctor,
    ISNULL(FechaFin, '') AS FechaFin
FROM Procedimiento
WHERE PacienteId = @PacienteId AND Actividad LIKE '%' + @Diente + '%'
ORDER BY Fecha DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                    cmd.Parameters.AddWithValue("@Diente", diente);

                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
