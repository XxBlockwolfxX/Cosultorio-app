using System.Data;
using System.Data.SqlClient;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Models;

namespace ConsultorioDentalApp.Services
{
    public class PacienteService
    {
        public Paciente ObtenerPorId(int pacienteId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
SELECT 
    Id, 
    Nombre, 
    Edad, 
    Sexo, 
    EstadoCivil,
    Telefono, 
    Correo, 
    Direccion
FROM Paciente
WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", pacienteId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new Paciente
                            {
                                Id = rd.GetInt32(0),
                                Nombre = rd.IsDBNull(1) ? null : rd.GetString(1),
                                Edad = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2),
                                Sexo = rd.IsDBNull(3) ? null : rd.GetString(3),
                                EstadoCivil = rd.IsDBNull(4) ? null : rd.GetString(4),
                                Telefono = rd.IsDBNull(5) ? null : rd.GetString(5),
                                Correo = rd.IsDBNull(6) ? null : rd.GetString(6),
                                Direccion = rd.IsDBNull(7) ? null : rd.GetString(7)
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
