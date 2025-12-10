using System;
using System.Data;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Models;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Services
{
    public class PacienteService
    {
        public Paciente ObtenerPorId(int pacienteId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        Id, 
                        Nombre, 
                        Edad, 
                        Sexo, 
                        FechaNacimiento,
                        EstadoCivil,
                        Direccion,
                        TelefonoMovil AS Telefono,
                        Whatsapp,
                        Correo
                    FROM Paciente
                    WHERE Id = @Id;
                ";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", pacienteId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new Paciente
                            {
                                Id = rd.GetInt32("Id"),
                                Nombre = rd.IsDBNull(rd.GetOrdinal("Nombre")) ? null : rd.GetString("Nombre"),
                                Edad = rd.IsDBNull(rd.GetOrdinal("Edad")) ? (int?)null : rd.GetInt32("Edad"),
                                Sexo = rd.IsDBNull(rd.GetOrdinal("Sexo")) ? null : rd.GetString("Sexo"),
                                FechaNacimiento = rd.IsDBNull(rd.GetOrdinal("FechaNacimiento"))
                                    ? (DateTime?)null
                                    : rd.GetDateTime("FechaNacimiento"),
                                EstadoCivil = rd.IsDBNull(rd.GetOrdinal("EstadoCivil")) ? null : rd.GetString("EstadoCivil"),
                                Direccion = rd.IsDBNull(rd.GetOrdinal("Direccion")) ? null : rd.GetString("Direccion"),
                                Telefono = rd.IsDBNull(rd.GetOrdinal("Telefono")) ? null : rd.GetString("Telefono"),
                                Whatsapp = rd.IsDBNull(rd.GetOrdinal("Whatsapp")) ? null : rd.GetString("Whatsapp"),
                                Correo = rd.IsDBNull(rd.GetOrdinal("Correo")) ? null : rd.GetString("Correo")
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
