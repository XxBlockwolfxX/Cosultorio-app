using ConsultorioDentalApp.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ConsultorioDentalApp.Services
{
    public class OdontogramaService
    {
        public List<Odontograma> ObtenerPorPaciente(int pacienteId)
        {
            var lista = new List<Odontograma>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT Id, PacienteId, Diente, Cara, Estado, Color, FechaActualizacion 
                                 FROM Odontograma
                                 WHERE PacienteId = @PacienteId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Odontograma
                            {
                                Id = rd.GetInt32(0),
                                PacienteId = rd.GetInt32(1),
                                Diente = rd.GetInt32(2),
                                Cara = rd.GetString(3),
                                Estado = rd.GetString(4),
                                Color = rd.GetString(5),
                                FechaActualizacion = rd.GetDateTime(6)
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public void GuardarEstado(int pacienteId, List<Odontograma> estado)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // borrar odontograma anterior
                    using (var cmdDel = new SqlCommand(
                        "DELETE FROM Odontograma WHERE PacienteId = @PacienteId", conn, tx))
                    {
                        cmdDel.Parameters.AddWithValue("@PacienteId", pacienteId);
                        cmdDel.ExecuteNonQuery();
                    }

                    // insertar nuevo estado
                    foreach (var item in estado)
                    {
                        using (var cmdIns = new SqlCommand(@"
                            INSERT INTO Odontograma 
                            (PacienteId, Diente, Cara, Estado, Color, FechaActualizacion)
                            VALUES (@PacienteId, @Diente, @Cara, @Estado, @Color, GETDATE());",
                            conn, tx))
                        {
                            cmdIns.Parameters.AddWithValue("@PacienteId", pacienteId);
                            cmdIns.Parameters.AddWithValue("@Diente", item.Diente);
                            cmdIns.Parameters.AddWithValue("@Cara", item.Cara);
                            cmdIns.Parameters.AddWithValue("@Estado", item.Estado);
                            cmdIns.Parameters.AddWithValue("@Color", item.Color);

                            cmdIns.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }
    }
}
