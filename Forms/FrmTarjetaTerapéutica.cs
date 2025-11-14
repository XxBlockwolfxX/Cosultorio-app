using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Services;

namespace ConsultorioDentalApp.Forms
{
    public class FrmTarjetaTerapéutica : Form
    {
        private readonly int _pacienteId;
        private readonly Panel pnlOdontograma = new Panel();
        private readonly Panel pnlPaciente = new Panel();
        private readonly DataGridView dgvHistorial = new DataGridView();
        private readonly Label lblNombre = new Label();
        private readonly Label lblEdad = new Label();
        private readonly Label lblSexo = new Label();
        private readonly Label lblDiagnostico = new Label();
        private string dienteSeleccionado = "11";

        public FrmTarjetaTerapéutica(int pacienteId)
        {
            _pacienteId = pacienteId;
            Text = "Tarjeta Terapéutica";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;

            // Layout principal
            pnlOdontograma.Dock = DockStyle.Left;
            pnlOdontograma.Width = 600;
            pnlOdontograma.BackColor = Color.WhiteSmoke;

            dgvHistorial.Dock = DockStyle.Fill;
            dgvHistorial.ReadOnly = true;
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlPaciente.Dock = DockStyle.Right;
            pnlPaciente.Width = 300;
            pnlPaciente.BackColor = Color.White;
            pnlPaciente.Padding = new Padding(16);

            Controls.Add(dgvHistorial);
            Controls.Add(pnlPaciente);
            Controls.Add(pnlOdontograma);

            CargarPaciente();
            ConstruirOdontograma();
            CargarHistorial(dienteSeleccionado);
        }

        private void CargarPaciente()
        {
            var service = new PacienteService();
            var p = service.ObtenerPorId(_pacienteId) ?? new ConsultorioDentalApp.Models.Paciente
            {
                Id = _pacienteId,
                Nombre = "Paciente #" + _pacienteId,
                Edad = null,
                Sexo = null,
                Diagnostico = null
            };

            lblNombre.Text = "Nombre: " + (p.Nombre ?? "-");
            lblEdad.Text = "Edad: " + (p.Edad?.ToString() ?? "-");
            lblSexo.Text = "Sexo: " + (p.Sexo ?? "-");
            lblDiagnostico.Text = "Diagnóstico: " + (p.Diagnostico ?? "-");

            var titulo = new Label
            {
                Text = "Paciente",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 32
            };

            var pila = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };

            foreach (var lbl in new[] { lblNombre, lblEdad, lblSexo, lblDiagnostico })
            {
                lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                lbl.AutoSize = true;
                pila.Controls.Add(lbl);
            }

            pnlPaciente.Controls.Add(pila);
            pnlPaciente.Controls.Add(titulo);
            pila.BringToFront();
        }

        private void ConstruirOdontograma()
        {
            pnlOdontograma.Controls.Clear();

            var lblTitulo = new Label
            {
                Text = "Odontograma",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Left = 16,
                Top = 12,
                AutoSize = true
            };
            pnlOdontograma.Controls.Add(lblTitulo);

            // Fila superior e inferior (adulto) estilo simple
            string[] superiores = { "18", "17", "16", "15", "14", "13", "12", "11", "21", "22", "23", "24", "25", "26", "27", "28" };
            string[] inferiores = { "48", "47", "46", "45", "44", "43", "42", "41", "31", "32", "33", "34", "35", "36", "37", "38" };

            int x = 16, y = 48;
            int w = 62, h = 50, gap = 8;
            var font = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (var d in superiores)
            {
                var btn = CrearBotonDiente(d, font, w, h);
                btn.Left = x; btn.Top = y;
                pnlOdontograma.Controls.Add(btn);
                x += w + gap;
            }

            x = 16; y += h + 20;
            foreach (var d in inferiores)
            {
                var btn = CrearBotonDiente(d, font, w, h);
                btn.Left = x; btn.Top = y;
                pnlOdontograma.Controls.Add(btn);
                x += w + gap;
            }
        }

        private Button CrearBotonDiente(string numero, Font f, int w, int h)
        {
            var btn = new Button
            {
                Text = numero,
                Font = f,
                Width = w,
                Height = h,
                BackColor = numero == dienteSeleccionado ? Color.LightSkyBlue : Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 1;

            btn.Click += (s, e) =>
            {
                dienteSeleccionado = numero;
                // Resaltar seleccionado
                foreach (Control c in pnlOdontograma.Controls)
                {
                    if (c is Button b && b.Text.Length >= 2)
                        b.BackColor = b.Text == dienteSeleccionado ? Color.LightSkyBlue : Color.White;
                }
                CargarHistorial(dienteSeleccionado);
            };

            return btn;
        }

        private void CargarHistorial(string diente)
        {
            DataTable dt = TratamientoService.ObtenerPorDiente(_pacienteId, diente);
            dgvHistorial.DataSource = dt;
        }
    }
}
