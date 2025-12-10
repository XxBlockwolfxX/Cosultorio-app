using ConsultorioDentalApp.Data;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

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

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Odontograma
                            {
                                Id = rd.GetInt32("Id"),
                                PacienteId = rd.GetInt32("PacienteId"),
                                Diente = rd.GetInt32("Diente"),
                                Cara = rd.GetString("Cara"),
                                Estado = rd.GetString("Estado"),
                                Color = rd.GetString("Color"),
                                FechaActualizacion = rd.GetDateTime("FechaActualizacion")
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
                    using (var cmdDel = new MySqlCommand(
                        "DELETE FROM Odontograma WHERE PacienteId = @PacienteId", conn, tx))
                    {
                        cmdDel.Parameters.AddWithValue("@PacienteId", pacienteId);
                        cmdDel.ExecuteNonQuery();
                    }

                    // insertar nuevo estado
                    foreach (var item in estado)
                    {
                        using (var cmdIns = new MySqlCommand(@"
                            INSERT INTO Odontograma 
                            (PacienteId, Diente, Cara, Estado, Color, FechaActualizacion)
                            VALUES (@PacienteId, @Diente, @Cara, @Estado, @Color, NOW());",
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
