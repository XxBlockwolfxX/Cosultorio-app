namespace ConsultorioDentalApp
{
    public class Odontograma
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int Diente { get; set; }
        public string Cara { get; set; }
        public string Color { get; set; }
        public string Overlay { get; set; }
    }
}
