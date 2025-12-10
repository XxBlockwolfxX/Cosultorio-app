using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Services;
using ConsultorioDentalApp.Models;

namespace ConsultorioDentalApp.Forms
{
    public class FrmTarjetaTerapéutica : Form
    {
        private readonly int _pacienteId;

        // Paneles principales
        private readonly Panel pnlOdontograma = new Panel();
        private readonly Panel pnlPaciente = new Panel();
        private readonly DataGridView dgvHistorial = new DataGridView();

        // Datos paciente
        private readonly Label lblNombre = new Label();
        private readonly Label lblEdad = new Label();
        private readonly Label lblSexo = new Label();
        private readonly Label lblDiagnostico = new Label();

        // Odontograma / servicios
        private string dienteSeleccionado = "11";
        private readonly OdontogramaService _odontogramaService = new OdontogramaService();
        private readonly ProtesisService _protesisService = new ProtesisService();
        private OdontogramaControl odontogramaControl1;

        // Controles de prótesis
        private ComboBox cmbTipoProtesis;
        private NumericUpDown numDienteInicio;
        private NumericUpDown numDienteFin;
        private RadioButton rdbRealizada;
        private RadioButton rdbPorRealizar;
        private Button btnAplicarProtesis;

        public FrmTarjetaTerapéutica(int pacienteId)
        {
            _pacienteId = pacienteId;
            Text = "Tarjeta Terapéutica";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;

            // ====== LAYOUT PRINCIPAL ======
            pnlOdontograma.Dock = DockStyle.Left;
            pnlOdontograma.Width = 650;
            pnlOdontograma.BackColor = Color.White;

            dgvHistorial.Dock = DockStyle.Fill;
            dgvHistorial.ReadOnly = true;
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            pnlPaciente.Dock = DockStyle.Right;
            pnlPaciente.Width = 280;
            pnlPaciente.BackColor = Color.White;
            pnlPaciente.Padding = new Padding(16);

            Controls.Add(dgvHistorial);
            Controls.Add(pnlPaciente);
            Controls.Add(pnlOdontograma);

            CargarPaciente();
            ConstruirOdontograma();
            CargarHistorial(dienteSeleccionado);
        }

        // ================= PACIENTE =================
        private void CargarPaciente()
        {
            var service = new PacienteService();
            var p = service.ObtenerPorId(_pacienteId) ?? new Paciente
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
                Text = "Datos del paciente",
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

        // ============== ODONTOGRAMA + PRÓTESIS ==============
        private void ConstruirOdontograma()
        {
            pnlOdontograma.Controls.Clear();

            // Título
            var lblTitulo = new Label
            {
                Text = "Odontograma",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(10, 8, 0, 0)
            };
            pnlOdontograma.Controls.Add(lblTitulo);

            // Panel central donde irá el control
            var pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 80) // espacio para la franja de prótesis
            };
            pnlOdontograma.Controls.Add(pnlCentro);

            // Panel inferior (controles de prótesis)
            var pnlProtesis = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(245, 247, 251)
            };
            pnlOdontograma.Controls.Add(pnlProtesis);

            // ---- OdontogramaControl ----
            odontogramaControl1 = new OdontogramaControl
            {
                Dock = DockStyle.Fill
            };

            // Cuando el usuario selecciona una cara/diente en el control
            odontogramaControl1.CaraSeleccionada += (num, cara) =>
            {
                dienteSeleccionado = num.ToString();
                CargarHistorial(dienteSeleccionado);
            };

            pnlCentro.Controls.Add(odontogramaControl1);

            // Cargar estado de caras desde BD
            var datos = _odontogramaService.ObtenerPorPaciente(_pacienteId);
            odontogramaControl1.AplicarEstado(datos);

            // Cargar prótesis desde BD y pintarlas en el odontograma
            var listaProtesis = _protesisService.ObtenerPorPaciente(_pacienteId);
            odontogramaControl1.AplicarProtesisDesdeDb(listaProtesis);

            // ---- Controles de prótesis ----
            cmbTipoProtesis = new ComboBox
            {
                Left = 10,
                Top = 10,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTipoProtesis.Items.AddRange(new object[]
            {
                "Superior Total",
                "Inferior Total",
                "Removible Parcial"
            });
            pnlProtesis.Controls.Add(cmbTipoProtesis);

            var lblDesde = new Label
            {
                Text = "Desde:",
                Left = 170,
                Top = 14,
                AutoSize = true
            };
            pnlProtesis.Controls.Add(lblDesde);

            numDienteInicio = new NumericUpDown
            {
                Left = 220,
                Top = 10,
                Width = 50,
                Minimum = 11,
                Maximum = 88,
                Value = 11
            };
            pnlProtesis.Controls.Add(numDienteInicio);

            var lblHasta = new Label
            {
                Text = "Hasta:",
                Left = 280,
                Top = 14,
                AutoSize = true
            };
            pnlProtesis.Controls.Add(lblHasta);

            numDienteFin = new NumericUpDown
            {
                Left = 330,
                Top = 10,
                Width = 50,
                Minimum = 11,
                Maximum = 88,
                Value = 21
            };
            pnlProtesis.Controls.Add(numDienteFin);

            rdbRealizada = new RadioButton
            {
                Text = "Realizada",
                Left = 400,
                Top = 12,
                AutoSize = true,
                Checked = true
            };
            pnlProtesis.Controls.Add(rdbRealizada);

            rdbPorRealizar = new RadioButton
            {
                Text = "Por realizar",
                Left = 490,
                Top = 12,
                AutoSize = true
            };
            pnlProtesis.Controls.Add(rdbPorRealizar);

            btnAplicarProtesis = new Button
            {
                Text = "Aplicar prótesis",
                Left = 400,
                Top = 38,
                Width = 140,
                Height = 26
            };
            btnAplicarProtesis.Click += btnAplicarProtesis_Click;
            pnlProtesis.Controls.Add(btnAplicarProtesis);
        }

        // === Botón "Aplicar prótesis" ===
        private void btnAplicarProtesis_Click(object sender, EventArgs e)
        {
            if (cmbTipoProtesis.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione el tipo de prótesis.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int inicio = (int)numDienteInicio.Value;
            int fin = (int)numDienteFin.Value;

            // Por si el usuario pone el rango al revés
            if (inicio > fin)
            {
                int tmp = inicio;
                inicio = fin;
                fin = tmp;
            }

            string tipo = cmbTipoProtesis.Text;               // "Superior Total", etc.
            string estado = rdbRealizada.Checked ? "Realizada" : "Por Realizar";

            // 1) Pintar en el odontograma
            odontogramaControl1.AplicarProtesis(tipo, inicio, fin, estado);

            // 2) Capturar TODO el estado de prótesis actual y guardarlo en BD
            var lista = odontogramaControl1.CapturarProtesis(_pacienteId);
            _protesisService.Guardar(_pacienteId, lista);

            MessageBox.Show("Prótesis aplicada y guardada correctamente.",
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ================= HISTORIAL =================
        private void CargarHistorial(string diente)
        {
            DataTable dt = TratamientoService.ObtenerPorDiente(_pacienteId, diente);
            dgvHistorial.DataSource = dt;
        }
    }
}
