using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Services;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmAgenda : Form
    {
        private Label lblTitulo;

        // Form cita (izquierda arriba)
        private TextBox txtMotivo;
        private ComboBox cboHora;
        private DateTimePicker dtpFecha;
        private Button btnGuardarCita;

        // Pacientes (izquierda abajo)
        private Label lblBuscarPaciente;
        private TextBox txtBuscarPaciente;
        private DataGridView dgvPacientes;
        private Label lblPacienteSel;

        // Citas (derecha)
        private DataGridView dgvCitas;

        // Estado
        private int? pacienteIdSeleccionado = null;
        private string pacienteNombreSeleccionado = "";
        private string pacienteWhatsappSeleccionado = "";

        private string plantillaMensaje =
            "Hola {NOMBRE}, le recordamos que tiene una cita odontológica el {FECHA} a las {HORA} " +
            "para {TRATAMIENTO}. Si no puede asistir, por favor avísenos para reprogramar. Gracias.";

        private AgendaConfig cfg;

        private Timer tmrRecordatorios;

        public FrmAgenda()
        {
            cfg = AgendaConfigService.Cargar();
            BuildUI();
            CargarHoras();
            CargarPacientes("");
            CargarCitas();
            IniciarRecordatorios();
        }

        private void BuildUI()
        {
            BackColor = Color.FromArgb(20, 20, 24);
            Font = new Font("Segoe UI", 10f);

            lblTitulo = new Label
            {
                Text = "Agenda de citas",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(10, 6, 0, 0)
            };
            Controls.Add(lblTitulo);

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10, 6, 10, 10),
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430)); // izquierda fija (como tu imagen)
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(main);

            // ============ IZQUIERDA ============
            var cardLeft = CrearCard();
            cardLeft.Dock = DockStyle.Fill;
            cardLeft.Padding = new Padding(12);
            main.Controls.Add(cardLeft, 0, 0);

            // layout interno (form + pacientes)
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // form
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // seleccionado
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // lista
            cardLeft.Controls.Add(left);

            // ------- FORM CITA -------
            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                AutoSize = true
            };
            left.Controls.Add(form, 0, 0);

            form.Controls.Add(CrearLabel("Motivo"));
            txtMotivo = CrearTextbox();
            form.Controls.Add(txtMotivo);

            form.Controls.Add(CrearLabel("Hora", topPad: 8));
            cboHora = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 34,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            form.Controls.Add(cboHora);

            form.Controls.Add(CrearLabel("Fecha", topPad: 8));
            dtpFecha = new DateTimePicker
            {
                Dock = DockStyle.Top,
                Height = 34,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = DateTime.Today
            };
            form.Controls.Add(dtpFecha);

            btnGuardarCita = new Button
            {
                Text = "Guardar cita",
                Dock = DockStyle.Top,           // ✅ ancho completo
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 174, 96),
                Margin = new Padding(0, 12, 0, 10)
            };
            btnGuardarCita.FlatAppearance.BorderSize = 0;
            btnGuardarCita.Click += (s, e) => GuardarCita();
            form.Controls.Add(btnGuardarCita);

            // ------- PACIENTE SELECCIONADO -------
            lblPacienteSel = new Label
            {
                Text = "Paciente: (no seleccionado)",
                ForeColor = Color.Gainsboro,
                Dock = DockStyle.Top,
                Height = 22,
                Margin = new Padding(0, 0, 0, 8)
            };
            left.Controls.Add(lblPacienteSel, 0, 1);

            // ------- BUSCAR + GRID PACIENTES -------
            var pacientesPanel = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(pacientesPanel, 0, 2);

            lblBuscarPaciente = new Label
            {
                Text = "Buscar paciente",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 22
            };
            pacientesPanel.Controls.Add(lblBuscarPaciente);

            txtBuscarPaciente = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 0, 0, 8)
            };
            txtBuscarPaciente.TextChanged += (s, e) => CargarPacientes(txtBuscarPaciente.Text);
            pacientesPanel.Controls.Add(txtBuscarPaciente);

            dgvPacientes = CrearGrid();
            dgvPacientes.Dock = DockStyle.Fill;
            dgvPacientes.CellClick += DgvPacientes_CellClick; // ✅ 1 clic
            pacientesPanel.Controls.Add(dgvPacientes);

            // ============ DERECHA ============
            var cardRight = CrearCard();
            cardRight.Dock = DockStyle.Fill;
            cardRight.Padding = new Padding(12);
            main.Controls.Add(cardRight, 1, 0);

            var lblCitas = new Label
            {
                Text = "Próximas citas",
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 24
            };
            cardRight.Controls.Add(lblCitas);

            dgvCitas = CrearGrid();
            dgvCitas.Dock = DockStyle.Fill;
            dgvCitas.RowPrePaint += DgvCitas_RowPrePaint;
            dgvCitas.CellDoubleClick += DgvCitas_CellDoubleClick;
            cardRight.Controls.Add(dgvCitas);

            // menú clic derecho (incluye WhatsApp)
            var menu = new ContextMenuStrip();

            var subEstado = new ToolStripMenuItem("Cambiar estado");
            subEstado.DropDownItems.Add("Pendiente", null, (s, e) => CambiarEstadoCitaSeleccionada("Pendiente"));
            subEstado.DropDownItems.Add("Atendida", null, (s, e) => CambiarEstadoCitaSeleccionada("Atendida"));
            subEstado.DropDownItems.Add("Cancelada", null, (s, e) => CambiarEstadoCitaSeleccionada("Cancelada"));
            subEstado.DropDownItems.Add("No asistió", null, (s, e) => CambiarEstadoCitaSeleccionada("No asistió"));
            menu.Items.Add(subEstado);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Enviar WhatsApp", null, (s, e) => EnviarWhatsappDeCitaSeleccionada());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Eliminar cita", null, (s, e) => EliminarCitaSeleccionada());

            dgvCitas.ContextMenuStrip = menu;

            dgvCitas.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = dgvCitas.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        dgvCitas.ClearSelection();
                        dgvCitas.Rows[hit.RowIndex].Selected = true;
                        dgvCitas.CurrentCell = dgvCitas.Rows[hit.RowIndex].Cells["Paciente"];
                    }
                }
            };
        }

        // ============ HELPERS UI ============
        private Panel CrearCard()
        {
            return new Panel
            {
                BackColor = Color.FromArgb(45, 48, 58),
                Padding = new Padding(14),
                Margin = new Padding(0)
            };
        }

        private Label CrearLabel(string text, int topPad = 2)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 18,
                Padding = new Padding(0, topPad, 0, 0)
            };
        }

        private TextBox CrearTextbox()
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Height = 32
            };
        }

        private DataGridView CrearGrid()
        {
            var dgv = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.FromArgb(60, 60, 65),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(70, 70, 75);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 85);

            return dgv;
        }

        // ============ PACIENTES ============
        private void DgvPacientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvPacientes.Rows[e.RowIndex];
            pacienteIdSeleccionado = Convert.ToInt32(row.Cells["Id"].Value);
            pacienteNombreSeleccionado = row.Cells["Nombre"].Value?.ToString() ?? "";

            string w = row.Cells["Whatsapp"].Value?.ToString() ?? "";
            string t = row.Cells["Telefono"].Value?.ToString() ?? "";
            pacienteWhatsappSeleccionado = !string.IsNullOrWhiteSpace(w) ? w : t;

            lblPacienteSel.Text = $"Paciente: {pacienteNombreSeleccionado}";
        }

        private void CargarPacientes(string filtro)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT Id, Nombre, TelefonoMovil AS Telefono, Whatsapp
                        FROM Paciente
                        WHERE Nombre LIKE @filtro
                        ORDER BY Nombre ASC
                        LIMIT 50;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvPacientes.DataSource = dt;

                        if (dgvPacientes.Columns["Id"] != null) dgvPacientes.Columns["Id"].Visible = false;
                        if (dgvPacientes.Columns["Nombre"] != null) dgvPacientes.Columns["Nombre"].HeaderText = "Paciente";
                        if (dgvPacientes.Columns["Telefono"] != null) dgvPacientes.Columns["Telefono"].HeaderText = "Teléfono";
                        if (dgvPacientes.Columns["Whatsapp"] != null) dgvPacientes.Columns["Whatsapp"].HeaderText = "WhatsApp";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar pacientes:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ CITAS ============
        private void CargarCitas()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            a.Id,
                            a.PacienteId,
                            p.Nombre AS Paciente,
                            a.FechaHora,
                            DATE_FORMAT(a.FechaHora, '%d/%m/%Y') AS Fecha,
                            DATE_FORMAT(a.FechaHora, '%H:%i') AS Hora,
                            a.Motivo,
                            a.Estado,
                            CASE WHEN DATE(a.FechaHora) = CURDATE() THEN 1 ELSE 0 END AS EsHoy,
                            CASE WHEN DATE(a.FechaHora) = DATE(DATE_ADD(NOW(), INTERVAL 1 DAY)) THEN 1 ELSE 0 END AS EsManana,
                            CASE WHEN a.FechaHora < NOW() THEN 1 ELSE 0 END AS EsPasada
                        FROM CitaAgenda a
                        INNER JOIN Paciente p ON p.Id = a.PacienteId
                        ORDER BY a.FechaHora ASC;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvCitas.DataSource = dt;
                    }
                }

                if (dgvCitas.Columns["Id"] != null) dgvCitas.Columns["Id"].Visible = false;
                if (dgvCitas.Columns["PacienteId"] != null) dgvCitas.Columns["PacienteId"].Visible = false;
                if (dgvCitas.Columns["FechaHora"] != null) dgvCitas.Columns["FechaHora"].Visible = false;
                if (dgvCitas.Columns["EsHoy"] != null) dgvCitas.Columns["EsHoy"].Visible = false;
                if (dgvCitas.Columns["EsManana"] != null) dgvCitas.Columns["EsManana"].Visible = false;
                if (dgvCitas.Columns["EsPasada"] != null) dgvCitas.Columns["EsPasada"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar citas:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarCita()
        {
            if (cfg == null) cfg = AgendaConfigService.Cargar();

            if (pacienteIdSeleccionado == null)
            {
                MessageBox.Show("Selecciona un paciente en la lista.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cboHora.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una hora.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int dow = (int)dtpFecha.Value.DayOfWeek; // Dom=0..Sab=6
            int dia = (dow == 0) ? 7 : dow;

            if (!cfg.DiasLaborales.Contains(dia))
            {
                MessageBox.Show("Esa fecha no es un día laboral según la configuración.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TimeSpan.TryParse(cboHora.SelectedItem.ToString(), out TimeSpan hora))
            {
                MessageBox.Show("La hora seleccionada no es válida.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (hora < cfg.HoraInicio || hora > cfg.HoraFin)
            {
                MessageBox.Show("La hora está fuera del horario laboral configurado.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime fechaHora = dtpFecha.Value.Date.Add(hora);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    using (var chk = new MySqlCommand(@"SELECT COUNT(*) FROM CitaAgenda WHERE FechaHora=@FechaHora;", conn))
                    {
                        chk.Parameters.AddWithValue("@FechaHora", fechaHora);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Ya existe una cita para esa fecha y hora.", "Agenda",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var cmd = new MySqlCommand(@"
                        INSERT INTO CitaAgenda (PacienteId, FechaHora, Motivo, Estado)
                        VALUES (@PacienteId, @FechaHora, @Motivo, 'Pendiente');", conn))
                    {
                        cmd.Parameters.AddWithValue("@PacienteId", pacienteIdSeleccionado.Value);
                        cmd.Parameters.AddWithValue("@FechaHora", fechaHora);
                        cmd.Parameters.AddWithValue("@Motivo", txtMotivo.Text?.Trim() ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Cita guardada correctamente.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtMotivo.Clear();
                CargarCitas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cita:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvCitas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvCitas.Rows[e.RowIndex];
            txtMotivo.Text = row.Cells["Motivo"]?.Value?.ToString() ?? "";
        }

        private void EliminarCitaSeleccionada()
        {
            if (dgvCitas.CurrentRow == null) return;

            var idObj = dgvCitas.CurrentRow.Cells["Id"].Value;
            if (idObj == null) return;

            int citaId = Convert.ToInt32(idObj);

            if (MessageBox.Show("¿Eliminar esta cita?", "Agenda",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM CitaAgenda WHERE Id=@Id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", citaId);
                        cmd.ExecuteNonQuery();
                    }
                }

                CargarCitas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar cita:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CambiarEstadoCitaSeleccionada(string nuevoEstado)
        {
            if (dgvCitas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una cita.", "Agenda",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvCitas.SelectedRows[0];
            var idObj = row.Cells["Id"].Value;
            if (idObj == null) return;

            int citaId = Convert.ToInt32(idObj);

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("UPDATE CitaAgenda SET Estado=@Estado WHERE Id=@Id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
                        cmd.Parameters.AddWithValue("@Id", citaId);
                        cmd.ExecuteNonQuery();
                    }
                }

                CargarCitas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar estado:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ WHATSAPP (clic derecho) ============
        private void EnviarWhatsappDeCitaSeleccionada()
        {
            if (dgvCitas.CurrentRow == null)
            {
                MessageBox.Show("Selecciona una cita.", "WhatsApp",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvCitas.CurrentRow;

            int pacienteId = Convert.ToInt32(row.Cells["PacienteId"].Value);
            string nombre = row.Cells["Paciente"]?.Value?.ToString() ?? "";
            string fecha = row.Cells["Fecha"]?.Value?.ToString() ?? "";
            string hora = row.Cells["Hora"]?.Value?.ToString() ?? "";
            string tratamiento = row.Cells["Motivo"]?.Value?.ToString() ?? "";

            string numeroRaw = ObtenerWhatsappOPTelefono(pacienteId);
            if (string.IsNullOrWhiteSpace(numeroRaw))
            {
                MessageBox.Show("El paciente no tiene número registrado.", "WhatsApp",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string numero = NormalizarNumero(numeroRaw);

            string mensaje = plantillaMensaje
                .Replace("{NOMBRE}", nombre)
                .Replace("{FECHA}", fecha)
                .Replace("{HORA}", hora)
                .Replace("{TRATAMIENTO}", string.IsNullOrWhiteSpace(tratamiento) ? "su tratamiento" : tratamiento);

            string url = "https://wa.me/" + numero + "?text=" + Uri.EscapeDataString(mensaje);

            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir WhatsApp:\n" + ex.Message, "WhatsApp",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerWhatsappOPTelefono(int pacienteId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT Whatsapp, TelefonoMovil FROM Paciente WHERE Id=@Id LIMIT 1;", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pacienteId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                string w = rd["Whatsapp"]?.ToString() ?? "";
                                string t = rd["TelefonoMovil"]?.ToString() ?? "";
                                return !string.IsNullOrWhiteSpace(w) ? w : t;
                            }
                        }
                    }
                }
            }
            catch { }
            return "";
        }

        private string NormalizarNumero(string raw)
        {
            string n = (raw ?? "").Trim();
            n = n.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (n.StartsWith("+")) n = n.Substring(1);
            if (n.StartsWith("0") && n.Length >= 10) n = "593" + n.Substring(1);
            if (n.Length == 9 && n.StartsWith("9")) n = "593" + n;

            return n;
        }

        // ============ HORAS ============
        private void CargarHoras()
        {
            if (cfg == null) cfg = AgendaConfigService.Cargar();

            cboHora.Items.Clear();

            var start = cfg.HoraInicio;
            var end = cfg.HoraFin;
            int step = Math.Max(5, cfg.IntervaloMin);

            for (var t = start; t <= end; t = t.Add(TimeSpan.FromMinutes(step)))
                cboHora.Items.Add(t.ToString(@"hh\:mm"));

            if (cboHora.Items.Count > 0) cboHora.SelectedIndex = 0;
        }

        public void RecargarConfig()
        {
            cfg = AgendaConfigService.Cargar();
            CargarHoras();
        }

        // ============ COLOR FILAS ============
        private void DgvCitas_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvCitas.Rows[e.RowIndex];

            int esPasada = dgvCitas.Columns.Contains("EsPasada") && row.Cells["EsPasada"].Value != null ? Convert.ToInt32(row.Cells["EsPasada"].Value) : 0;
            int esHoy = dgvCitas.Columns.Contains("EsHoy") && row.Cells["EsHoy"].Value != null ? Convert.ToInt32(row.Cells["EsHoy"].Value) : 0;
            int esManana = dgvCitas.Columns.Contains("EsManana") && row.Cells["EsManana"].Value != null ? Convert.ToInt32(row.Cells["EsManana"].Value) : 0;

            if (esPasada == 1)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(231, 76, 60);
                row.DefaultCellStyle.ForeColor = Color.White;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(192, 57, 43);
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else if (esHoy == 1)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(46, 204, 113);
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 174, 96);
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
            }
            else if (esManana == 1)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(241, 196, 15);
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 180, 10);
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = dgvCitas.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.ForeColor = dgvCitas.DefaultCellStyle.ForeColor;
                row.DefaultCellStyle.SelectionBackColor = dgvCitas.DefaultCellStyle.SelectionBackColor;
                row.DefaultCellStyle.SelectionForeColor = dgvCitas.DefaultCellStyle.SelectionForeColor;
            }
        }

        private void IniciarRecordatorios()
        {
            tmrRecordatorios = new Timer();
            tmrRecordatorios.Interval = 60 * 1000; // 1 minuto
            tmrRecordatorios.Tick += (s, e) => RevisarYEnviarRecordatorios();
            tmrRecordatorios.Start();

            // opcional: revisar al abrir también
            RevisarYEnviarRecordatorios();
        }

        private void RevisarYEnviarRecordatorios()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    var pendientes = new System.Collections.Generic.List<(int Id, string Nombre, string Fecha, string Hora, string Motivo, string Numero)>();

                    using (var cmd = new MySqlCommand(@"
                SELECT a.Id,
                       DATE_FORMAT(a.FechaHora,'%d/%m/%Y') AS Fecha,
                       DATE_FORMAT(a.FechaHora,'%H:%i') AS Hora,
                       a.Motivo,
                       p.Nombre AS Paciente,
                       COALESCE(NULLIF(p.Whatsapp,''), p.TelefonoMovil) AS Numero
                FROM CitaAgenda a
                INNER JOIN Paciente p ON p.Id=a.PacienteId
                WHERE a.RecordatorioEnviado = 0
                  AND a.Estado IN ('Pendiente')
                  AND a.FechaHora BETWEEN DATE_ADD(NOW(), INTERVAL 59 MINUTE)
                                     AND DATE_ADD(NOW(), INTERVAL 61 MINUTE)
                ORDER BY a.FechaHora ASC;", conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            pendientes.Add((
                                Convert.ToInt32(rd["Id"]),
                                rd["Paciente"]?.ToString() ?? "",
                                rd["Fecha"]?.ToString() ?? "",
                                rd["Hora"]?.ToString() ?? "",
                                rd["Motivo"]?.ToString() ?? "",
                                rd["Numero"]?.ToString() ?? ""
                            ));
                        }
                    } // ✅ aquí ya se cerró el reader

                    foreach (var c in pendientes)
                    {
                        if (string.IsNullOrWhiteSpace(c.Numero))
                        {
                            MarcarRecordatorioEnviado(conn, c.Id);
                            continue;
                        }

                        string numero = NormalizarNumero(c.Numero);

                        string mensaje = plantillaMensaje
                            .Replace("{NOMBRE}", c.Nombre)
                            .Replace("{FECHA}", c.Fecha)
                            .Replace("{HORA}", c.Hora)
                            .Replace("{TRATAMIENTO}", string.IsNullOrWhiteSpace(c.Motivo) ? "su tratamiento" : c.Motivo);

                        AbrirWhatsapp(numero, mensaje);
                        MarcarRecordatorioEnviado(conn, c.Id);
                    }
                }
            }
            catch
            {
                // opcional: log
            }
        }


        private void MarcarRecordatorioEnviado(MySqlConnection conn, int citaId)
        {
            using (var up = new MySqlCommand("UPDATE CitaAgenda SET RecordatorioEnviado=1 WHERE Id=@Id;", conn))
            {
                up.Parameters.AddWithValue("@Id", citaId);
                up.ExecuteNonQuery();
            }
        }

        private void AbrirWhatsapp(string numero, string mensaje)
        {
            string url = "https://wa.me/" + numero + "?text=" + Uri.EscapeDataString(mensaje);
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }



    }
}
