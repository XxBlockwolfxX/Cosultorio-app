using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Services;
using ConsultorioDentalApp.Models;  
using ConsultorioDentalApp;
using System.Data;



namespace ConsultorioDentalApp.Forms
{
    public partial class FrmFichaClinica : Form
    {
        private readonly int _pacienteId;

        private readonly OdontogramaService _odontogramaService = new OdontogramaService();
        private readonly PacienteService _pacienteService = new PacienteService();
        private readonly ProtesisService _protesisService = new ProtesisService();

        private OdontogramaControl _odontogramaControl;

        // === NUEVOS CAMPOS PARA LAS PESTAÑAS ===
        private TabControl _tabDetalles;
        private TabPage _tabClinica;
        private TabPage _tabProtesis;

        private DataGridView _dgvHistorial;
        private ComboBox _cmbTipoProtesis;
        private NumericUpDown _numDienteInicio;
        private NumericUpDown _numDienteFin;
        private RadioButton _rdbRealizada;
        private RadioButton _rdbPorRealizar;
        private Button _btnAplicarProtesis;
        private Button _btnLimpiarProtesis;

        private string _dienteSeleccionado = "11";

        // Labels de la ficha del paciente
        private Label lblTituloPaciente;
        private Label lblNombreValor;
        private Label lblEdadValor;
        private Label lblSexoValor;
        private Label lblEstadoCivilValor;
        private Label lblTelefonoValor;
        private Label lblCorreoValor;
        private Label lblDireccionValor;

        public FrmFichaClinica(int pacienteId)
        {
            _pacienteId = pacienteId;
            InitializeComponent();
            BuildUI();
            CargarDatosPaciente();
            CargarOdontograma();
        }

        private void BuildUI()
        {
            SuspendLayout();

            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(236, 240, 245);
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            // ===== LAYOUT PRINCIPAL: 2 COLUMNAS =====
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            // Columna 0 fija (datos del paciente), columna 1 ocupa el resto
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Controls.Add(mainLayout);

            // ===== PANEL IZQUIERDO (DATOS PACIENTE) =====
            var pnlPaciente = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(20, 25, 20, 20)
            };
            mainLayout.Controls.Add(pnlPaciente, 0, 0);

            // Sombra a la derecha
            var pnlShadow = new Panel
            {
                Dock = DockStyle.Right,
                Width = 2,
                BackColor = Color.FromArgb(210, 215, 225)
            };
            pnlPaciente.Controls.Add(pnlShadow);

            // Título grande (nombre del paciente)
            lblTituloPaciente = new Label
            {
                Text = $"Paciente ID: {_pacienteId}",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 60, 110),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            pnlPaciente.Controls.Add(lblTituloPaciente);

            int y = 60;
            int sep = 30;

            Label CrearCampo(string titulo, ref int yPos, out Label lblValor)
            {
                var lblTitulo = new Label
                {
                    Text = titulo,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(90, 110, 140),
                    Location = new Point(0, yPos),
                    AutoSize = true
                };
                pnlPaciente.Controls.Add(lblTitulo);

                lblValor = new Label
                {
                    Text = "-",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(30, 60, 100),
                    Location = new Point(0, yPos + 17),
                    AutoSize = true,
                    MaximumSize = new Size(pnlPaciente.Width - 40, 0)
                };
                pnlPaciente.Controls.Add(lblValor);

                yPos += sep + 18;
                return lblValor;
            }

            CrearCampo("Nombre:", ref y, out lblNombreValor);
            CrearCampo("Edad:", ref y, out lblEdadValor);
            CrearCampo("Sexo:", ref y, out lblSexoValor);
            CrearCampo("Estado civil:", ref y, out lblEstadoCivilValor);
            CrearCampo("Teléfono:", ref y, out lblTelefonoValor);
            CrearCampo("Correo:", ref y, out lblCorreoValor);
            CrearCampo("Dirección:", ref y, out lblDireccionValor);

            // ===== PANEL DERECHO (ODONTOGRAMA) =====
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 245),
                Padding = new Padding(15)
            };
            mainLayout.Controls.Add(pnlRight, 1, 0);

            // Layout vertical del lado derecho
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Fila 0: título
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35f));
            // Fila 1: tarjeta odontograma 
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 390f));
            // Fila 2: pestañas (ocupa el resto)
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));


            pnlRight.Controls.Add(rightLayout);

            // --- Fila 0: título odonto ---
            var lblTituloOdonto = new Label
            {
                Text = "Odontograma",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 70, 120),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightLayout.Controls.Add(lblTituloOdonto, 0, 0);

            // --- Fila 1: tarjeta blanca del odontograma (más pequeña y centrada) ---
            var card = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5),   
                Width = 900,                
                Height = 480,               
                Anchor = AnchorStyles.Top   
            };

            // agregamos la card en la fila 1
            rightLayout.Controls.Add(card, 0, 1);

            // centrar horizontalmente el recuadro dentro del rightLayout
            rightLayout.Resize += (s, e) =>
            {
                var anchoCelda = rightLayout.GetColumnWidths()[0];
                card.Left = (anchoCelda - card.Width) / 2;
                card.Top = (rightLayout.GetRowHeights()[1] - card.Height) / 2;
            };


            // Panel interno para el control de odontograma
            var pnlOdonto = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            card.Controls.Add(pnlOdonto);

            _odontogramaControl = new OdontogramaControl
            {
                Dock = DockStyle.Fill
            };
            pnlOdonto.Controls.Add(_odontogramaControl);

            // Cada vez que el usuario haga clic en una pieza/cara, actualizamos el diente seleccionado
            _odontogramaControl.CaraSeleccionada += (num, cara) =>
            {
                _dienteSeleccionado = num.ToString();
                CargarHistorial(_dienteSeleccionado);
            };

            // === Fila 2: TabControl con Clínica y Prótesis ===
            _tabDetalles = new TabControl
            {
                Dock = DockStyle.Fill
            };

            _tabClinica = new TabPage("Clínica");
            _tabProtesis = new TabPage("Prótesis / Historial");

            _tabDetalles.TabPages.Add(_tabClinica);
            _tabDetalles.TabPages.Add(_tabProtesis);

            rightLayout.Controls.Add(_tabDetalles, 0, 2);

            // ----- Contenido de la pestaña CLÍNICA -----
            var pnlClinica = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 251)
            };
            _tabClinica.Controls.Add(pnlClinica);

            int xBase = 10;
            int yBtn = 8;

            // Botón GUARDAR
            var btnGuardar = new Button
            {
                Text = "Guardar Odontograma",
                Width = 200,
                Height = 30,
                BackColor = Color.FromArgb(45, 90, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(xBase, yBtn)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += (s, e) => GuardarOdontograma();
            pnlClinica.Controls.Add(btnGuardar);

            // Texto de ayuda
            var lblHint = new Label
            {
                Text = "Tip: haga clic derecho en las caras de cada diente para marcar tratamientos.",
                AutoSize = true,
                ForeColor = Color.FromArgb(110, 125, 150),
                Location = new Point(xBase + btnGuardar.Width + 15, 12)
            };
            pnlClinica.Controls.Add(lblHint);

            // ----- Contenido de la pestaña PRÓTESIS / HISTORIAL -----
            var pnlProtesis = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            _tabProtesis.Controls.Add(pnlProtesis);

            // Panel superior: controles de prótesis
            var pnlProtesisTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };
            pnlProtesis.Controls.Add(pnlProtesisTop);

            // Tipo de prótesis
            var lblTipo = new Label
            {
                Text = "Tipo de prótesis:",
                Left = 10,
                Top = 14,
                AutoSize = true
            };
            pnlProtesisTop.Controls.Add(lblTipo);

            _cmbTipoProtesis = new ComboBox
            {
                Left = 120,
                Top = 10,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbTipoProtesis.Items.AddRange(new object[]
            {
    "Superior Total",
    "Inferior Total",
    "Removible Parcial"
            });
            pnlProtesisTop.Controls.Add(_cmbTipoProtesis);

            // Rango de dientes
            var lblDesde = new Label
            {
                Text = "Desde:",
                Left = 310,
                Top = 14,
                AutoSize = true
            };
            pnlProtesisTop.Controls.Add(lblDesde);

            _numDienteInicio = new NumericUpDown
            {
                Left = 360,
                Top = 10,
                Width = 50,
                Minimum = 11,
                Maximum = 88,
                Value = 11
            };
            pnlProtesisTop.Controls.Add(_numDienteInicio);

            var lblHasta = new Label
            {
                Text = "Hasta:",
                Left = 420,
                Top = 14,
                AutoSize = true
            };
            pnlProtesisTop.Controls.Add(lblHasta);

            _numDienteFin = new NumericUpDown
            {
                Left = 470,
                Top = 10,
                Width = 50,
                Minimum = 11,
                Maximum = 88,
                Value = 21
            };
            pnlProtesisTop.Controls.Add(_numDienteFin);

            // Estado: realizada / por realizar
            _rdbRealizada = new RadioButton
            {
                Text = "Realizada",
                Left = 540,
                Top = 12,
                AutoSize = true,
                Checked = true
            };
            pnlProtesisTop.Controls.Add(_rdbRealizada);

            _rdbPorRealizar = new RadioButton
            {
                Text = "Por realizar",
                Left = 630,
                Top = 12,
                AutoSize = true
            };
            pnlProtesisTop.Controls.Add(_rdbPorRealizar);

            // Botón aplicar
            _btnAplicarProtesis = new Button
            {
                Text = "Aplicar prótesis",
                Left = 540,
                Top = 38,
                Width = 140,
                Height = 26
            };
            _btnAplicarProtesis.Click += BtnAplicarProtesis_Click;
            pnlProtesisTop.Controls.Add(_btnAplicarProtesis);

            // Botón aplicar
            _btnAplicarProtesis = new Button
            {
                Text = "Aplicar prótesis",
                Left = 540,
                Top = 38,
                Width = 140,
                Height = 26
            };
            _btnAplicarProtesis.Click += BtnAplicarProtesis_Click;
            pnlProtesisTop.Controls.Add(_btnAplicarProtesis);

            // ===== NUEVO BOTÓN: LIMPIAR PRÓTESIS =====
            _btnLimpiarProtesis = new Button
            {
                Text = "Limpiar prótesis",
                Left = _btnAplicarProtesis.Right + 10,
                Top = _btnAplicarProtesis.Top,
                Width = 140,
                Height = 26
            };
            _btnLimpiarProtesis.Click += BtnLimpiarProtesis_Click;
            pnlProtesisTop.Controls.Add(_btnLimpiarProtesis);


            // DataGridView de historial (ocupa el resto)
            _dgvHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            pnlProtesis.Controls.Add(_dgvHistorial);


            ResumeLayout(false);
        }




        private void CargarDatosPaciente()
        {
            var paciente = _pacienteService.ObtenerPorId(_pacienteId);

            if (paciente == null)
            {
                Text = $"Ficha Clínica del Paciente #{_pacienteId}";
                lblTituloPaciente.Text = $"Paciente ID: {_pacienteId}";
                return;
            }

            // Título de la ventana y del panel
            Text = $"Ficha Clínica - {paciente.Nombre} (ID: {paciente.Id})";
            lblTituloPaciente.Text = paciente.Nombre ?? $"Paciente ID: {paciente.Id}";

            lblNombreValor.Text = paciente.Nombre ?? "-";
            lblEdadValor.Text = paciente.Edad.HasValue ? paciente.Edad.Value + " años" : "-";
            lblSexoValor.Text = string.IsNullOrWhiteSpace(paciente.Sexo) ? "-" : paciente.Sexo;
            lblEstadoCivilValor.Text = string.IsNullOrWhiteSpace(paciente.EstadoCivil) ? "-" : paciente.EstadoCivil;
            lblTelefonoValor.Text = string.IsNullOrWhiteSpace(paciente.Telefono) ? "-" : paciente.Telefono;
            lblCorreoValor.Text = string.IsNullOrWhiteSpace(paciente.Correo) ? "-" : paciente.Correo;
            lblDireccionValor.Text = string.IsNullOrWhiteSpace(paciente.Direccion) ? "-" : paciente.Direccion;
        }

        private void CargarOdontograma()
        {
            List<Odontograma> datos = _odontogramaService.ObtenerPorPaciente(_pacienteId);
            _odontogramaControl.AplicarEstado(datos);

            List<Protesis> datosProtesis = _protesisService.ObtenerPorPaciente(_pacienteId);
            _odontogramaControl.AplicarProtesisDesdeDb(datosProtesis);
        }

        private void BtnAplicarProtesis_Click(object sender, EventArgs e)
        {
            if (_cmbTipoProtesis.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione el tipo de prótesis.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int inicio = (int)_numDienteInicio.Value;
            int fin = (int)_numDienteFin.Value;

            // Por si el usuario pone el rango al revés
            if (inicio > fin)
            {
                int tmp = inicio;
                inicio = fin;
                fin = tmp;
            }

            string estado = _rdbRealizada.Checked ? "Realizada" : "Por realizar";

            // Aplica la prótesis en el control del odontograma
            _odontogramaControl.AplicarProtesis(
                _cmbTipoProtesis.SelectedItem.ToString(),
                inicio,
                fin,
                estado
            );

            // Guardar en BD
            var lista = _odontogramaControl.CapturarProtesis(_pacienteId);
            _protesisService.Guardar(_pacienteId, lista);

            MessageBox.Show("Prótesis aplicada y guardada correctamente.",
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLimpiarProtesis_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "¿Desea eliminar todas las prótesis registradas para este paciente?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            // 1) Limpiar visualmente en el odontograma
            _odontogramaControl.LimpiarProtesis();

            // 2) Limpiar en base de datos
            // Opción A: si tu servicio tiene un método específico:
            // _protesisService.EliminarPorPaciente(_pacienteId);

            // Opción B: sobrescribir con lista vacía (si Guardar ya borra las anteriores)
            _protesisService.Guardar(_pacienteId, new List<Protesis>());

            MessageBox.Show(
                "Se han eliminado las prótesis de este paciente.",
                "Información",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        // ================= HISTORIAL =================
        private void CargarHistorial(string diente)
        {
            // Aquí reutilizamos el mismo servicio de la Tarjeta Terapéutica
            DataTable dt = TratamientoService.ObtenerPorDiente(_pacienteId, diente);
            _dgvHistorial.DataSource = dt;
        }


        private void GuardarOdontograma()
        {
            List<Odontograma> estado = _odontogramaControl.CapturarEstado(_pacienteId);
            _odontogramaService.GuardarEstado(_pacienteId, estado);

            List<Protesis> estadoProtesis = _odontogramaControl.CapturarProtesis(_pacienteId);
            _protesisService.Guardar(_pacienteId, estadoProtesis);

            MessageBox.Show("Odontograma guardado correctamente.",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
