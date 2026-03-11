using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Services
{
    public class AgendaConfig
    {
        public HashSet<int> DiasLaborales { get; set; } = new HashSet<int> { 1, 2, 3, 4, 5, 6 }; // Lun-Sab
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan HoraFin { get; set; } = new TimeSpan(19, 0, 0);
        public int IntervaloMin { get; set; } = 30;
    }

    public static class AgendaConfigService
    {
        public static AgendaConfig Cargar()
        {
            var cfg = new AgendaConfig();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(@"
                        SELECT DiasLaborales, HoraInicio, HoraFin, IntervaloMin
                        FROM UsuarioConfig WHERE Id = 1 LIMIT 1;", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return cfg;

                        string dias = rd["DiasLaborales"]?.ToString() ?? "1,2,3,4,5,6";
                        cfg.DiasLaborales = new HashSet<int>(
                            dias.Split(',')
                                .Select(x => x.Trim())
                                .Where(x => int.TryParse(x, out _))
                                .Select(int.Parse)
                        );

                        cfg.HoraInicio = TimeSpan.Parse(rd["HoraInicio"].ToString());
                        cfg.HoraFin = TimeSpan.Parse(rd["HoraFin"].ToString());

                        int intervalo;
                        if (int.TryParse(rd["IntervaloMin"].ToString(), out intervalo))
                            cfg.IntervaloMin = Math.Max(5, intervalo);
                    }
                }
            }
            catch { /* si falla, usa defaults */ }

            if (cfg.HoraFin <= cfg.HoraInicio) cfg.HoraFin = cfg.HoraInicio.Add(TimeSpan.FromHours(10));
            if (cfg.DiasLaborales.Count == 0) cfg.DiasLaborales = new HashSet<int> { 1, 2, 3, 4, 5, 6 };

            return cfg;
        }

        public static void Guardar(AgendaConfig cfg)
        {
            string dias = string.Join(",", cfg.DiasLaborales.OrderBy(x => x));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(@"
                    INSERT INTO UsuarioConfig (Id, DiasLaborales, HoraInicio, HoraFin, IntervaloMin)
                    VALUES (1, @Dias, @Ini, @Fin, @Int)
                    ON DUPLICATE KEY UPDATE
                        DiasLaborales = VALUES(DiasLaborales),
                        HoraInicio = VALUES(HoraInicio),
                        HoraFin = VALUES(HoraFin),
                        IntervaloMin = VALUES(IntervaloMin);", conn))
                {
                    cmd.Parameters.AddWithValue("@Dias", dias);
                    cmd.Parameters.AddWithValue("@Ini", cfg.HoraInicio.ToString(@"hh\:mm\:ss"));
                    cmd.Parameters.AddWithValue("@Fin", cfg.HoraFin.ToString(@"hh\:mm\:ss"));
                    cmd.Parameters.AddWithValue("@Int", cfg.IntervaloMin);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
