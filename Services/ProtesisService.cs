using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Models;

namespace ConsultorioDentalApp.Services
{
    public class ProtesisService
    {
        public List<Protesis> ObtenerPorPaciente(int pacienteId)
        {
            var lista = new List<Protesis>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, PacienteId, Tipo, Inicio, Fin, Estado
                    FROM Protesis
                    WHERE PacienteId = @PacienteId";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lista.Add(new Protesis
                            {
                                Id = rd.GetInt32("Id"),
                                PacienteId = rd.GetInt32("PacienteId"),
                                Tipo = rd.GetString("Tipo"),
                                Inicio = rd.GetInt32("Inicio"),
                                Fin = rd.GetInt32("Fin"),
                                Estado = rd.GetString("Estado")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public void Guardar(int pacienteId, List<Protesis> lista)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    // 1) Borramos las prótesis previas del paciente
                    using (var del = new MySqlCommand(
                        "DELETE FROM Protesis WHERE PacienteId = @PacienteId", conn, tx))
                    {
                        del.Parameters.AddWithValue("@PacienteId", pacienteId);
                        del.ExecuteNonQuery();
                    }

                    // 2) Insertamos las nuevas
                    string insertSql = @"
                        INSERT INTO Protesis (PacienteId, Tipo, Inicio, Fin, Estado)
                        VALUES (@PacienteId, @Tipo, @Inicio, @Fin, @Estado);";

                    foreach (var p in lista)
                    {
                        using (var ins = new MySqlCommand(insertSql, conn, tx))
                        {
                            ins.Parameters.AddWithValue("@PacienteId", pacienteId);
                            ins.Parameters.AddWithValue("@Tipo", p.Tipo);
                            ins.Parameters.AddWithValue("@Inicio", p.Inicio);
                            ins.Parameters.AddWithValue("@Fin", p.Fin);
                            ins.Parameters.AddWithValue("@Estado", p.Estado);
                            ins.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
        }
    }
}
