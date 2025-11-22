using System;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Services;  // ← IMPORTANTE
using ConsultorioDentalApp;           // ← Modelo Odontograma
using System.Collections.Generic;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmFichaClinica : Form
    {
        private readonly int _pacienteId;
        private readonly OdontogramaService _odontogramaService = new OdontogramaService();
        private OdontogramaControl _odontogramaControl;

        public FrmFichaClinica(int pacienteId)
        {
            _pacienteId = pacienteId;
            InitializeComponent();
            BuildUI();
            CargarDatosPaciente();
            CargarOdontograma();
        }

        private void CargarDatosPaciente()
        {
            // Aquí puedes conectar con tu tabla Paciente si deseas
            // por ahora mostramos solo el ID seleccionado.
            Text = $"Ficha Clínica del Paciente #{_pacienteId}";
        }

        private void CargarOdontograma()
        {
            List<Odontograma> datos = _odontogramaService.ObtenerPorPaciente(_pacienteId);
            _odontogramaControl.AplicarEstado(datos);
        }

        private void GuardarOdontograma()
        {
            List<Odontograma> estado = _odontogramaControl.CapturarEstado(_pacienteId);
            _odontogramaService.GuardarEstado(_pacienteId, estado);

            MessageBox.Show("Odontograma guardado correctamente.",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BuildUI()
        {
            // ================== CONFIG FORM ===================
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            // ================= PANEL PRINCIPAL =================
            var main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            Controls.Add(main);

            // ================= PANEL IZQUIERDO =================
            var pnlPaciente = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(247, 249, 252)
            };
            main.Controls.Add(pnlPaciente);

            var lblPaciente = new Label
            {
                Text = $"Paciente ID: {_pacienteId}",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 70, 120),
                Location = new Point(20, 25),
                AutoSize = true
            };
            pnlPaciente.Controls.Add(lblPaciente);

            // ================= PANEL DERECHO =================
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            main.Controls.Add(pnlRight);

            // =============== TABCONTROL ======================
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };
            pnlRight.Controls.Add(tabs);

            var tpFicha = new TabPage("Ficha terapéutica") { BackColor = Color.White };
            tabs.TabPages.Add(tpFicha);

            // ================= PANEL ODONTOGRAMA =================
            var pnlOdonto = new Panel
            {
                Dock = DockStyle.Top,
                Height = 360,
                BackColor = Color.White
            };
            tpFicha.Controls.Add(pnlOdonto);

            // AGREGAR TU ODONTOGRAMA
            _odontogramaControl = new OdontogramaControl
            {
                Dock = DockStyle.Fill
            };
            pnlOdonto.Controls.Add(_odontogramaControl);

            // ================= BOTÓN GUARDAR =================
            var btnGuardar = new Button
            {
                Text = "Guardar Odontograma",
                Width = 200,
                Height = 40,
                BackColor = Color.FromArgb(45, 90, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 370)
            };
            btnGuardar.Click += (s, e) => GuardarOdontograma();
            tpFicha.Controls.Add(btnGuardar);
        }
    }
}
