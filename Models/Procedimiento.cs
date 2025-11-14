using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsultorioDentalApp.Models
{
    public class Procedimiento
    {
        // Definición de propiedades según uso en el repositorio
        public int Id { get; set; }
        public string Fecha { get; set; }
        public string Dia { get; set; }
        public string Actividad { get; set; }
        public decimal Valor { get; set; }
        public decimal Pago { get; set; }
        public decimal Saldo { get; set; }
    }
}
