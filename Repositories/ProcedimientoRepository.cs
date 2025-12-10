using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Models;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Repositories
{
    public class ProcedimientoRepository
    {
        public List<Procedimiento> ObtenerPorActividad(int pacienteId, string actividad)
        {
            var lista = new List<Procedimiento>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT Id, Fecha, Dia, Actividad, Valor, Pago, Saldo
                    FROM Procedimiento
                    WHERE PacienteId = @PacienteId AND Actividad = @Actividad
                    ORDER BY Id ASC;", conn);

                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                cmd.Parameters.AddWithValue("@Actividad", actividad);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Procedimiento
                        {
                            Id = reader.GetInt32("Id"),
                            Fecha = reader["Fecha"]?.ToString(),
                            Dia = reader["Dia"]?.ToString(),
                            Actividad = reader["Actividad"]?.ToString(),
                            Valor = reader["Valor"] != DBNull.Value ? Convert.ToDecimal(reader["Valor"]) : 0,
                            Pago = reader["Pago"] != DBNull.Value ? Convert.ToDecimal(reader["Pago"]) : 0,
                            Saldo = reader["Saldo"] != DBNull.Value ? Convert.ToDecimal(reader["Saldo"]) : 0
                        });
                    }
                }
            }

            return lista;
        }

        public decimal? ObtenerSaldoAnterior(int pacienteId, string actividad)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                var cmd = new MySqlCommand(@"
                    SELECT Saldo 
                    FROM Procedimiento
                    WHERE PacienteId = @PacienteId AND Actividad = @Actividad
                    ORDER BY Id DESC
                    LIMIT 1;", conn);

                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                cmd.Parameters.AddWithValue("@Actividad", actividad);

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : (decimal?)null;
            }
        }
    }
}
