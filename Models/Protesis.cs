using System;

namespace ConsultorioDentalApp.Models
{

    public class Protesis
    {
        public int Id { get; set; }

        // Id del paciente al que pertenece la prótesis
        public int PacienteId { get; set; }

        // Tipo de prótesis: "Superior Total", "Inferior Total", "Parcial Removible", etc.
        public string Tipo { get; set; } = string.Empty;

        // Rango de dientes que cubre (por ejemplo 14–24)
        public int Inicio { get; set; }
        public int Fin { get; set; }

        // Estado: "Realizada" o "Por Realizar"
        public string Estado { get; set; } = string.Empty;
    }
}
