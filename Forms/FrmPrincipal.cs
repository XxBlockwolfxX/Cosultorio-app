using System;
using System.Data;                      // <-- agregado
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmPrincipal : Form
    {
        private Panel pnlTop;
        private Panel pnlContenido;
        private FlowLayoutPanel flpMenu;

        private Button btnNuevoPaciente;
        private Button btnBackPacientes;

        //datos de usuario
        private Panel avatarPanel;
        private PictureBox picAvatar;
        private Label lblUser;
        private string usuarioNombre = "Nombre del usuario";
        private Image usuarioImagen;

        public FrmPrincipal()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "DentaSoft";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(18, 18, 24);
            Font = new Font("Segoe UI", 10f);

            // ====== TOP BAR ======
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(45, 48, 58)
            };
            Controls.Add(pnlTop);

            pnlTop.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(70, 70, 82), 1))
                {
                    e.Graphics.DrawLine(pen, 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);
                }
            };

            // Logo / nombre
            var lblLogo = new Label
            {
                Text = "★ DentaSoft",
                Dock = DockStyle.Left,
                Width = 220,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0),
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(lblLogo);

            // ====== BOTONES DEL MÓDULO PACIENTES ======
            btnNuevoPaciente = new Button
            {
                Text = "Nuevo paciente",
                Dock = DockStyle.Right,
                Width = 170,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 174, 96),
                Visible = false
            };
            btnNuevoPaciente.FlatAppearance.BorderSize = 0;
            btnNuevoPaciente.Click += BtnNuevoPaciente_Click;
            pnlTop.Controls.Add(btnNuevoPaciente);

            btnBackPacientes = new Button
            {
                Text = "← Menú principal",
                Dock = DockStyle.Right,
                Width = 160,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(64, 64, 80),
                Visible = false
            };
            btnBackPacientes.FlatAppearance.BorderSize = 0;
            btnBackPacientes.Click += BtnBackPacientes_Click;
            pnlTop.Controls.Add(btnBackPacientes);

            // ====== MENÚ PRINCIPAL (iconos) ======
            flpMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(220, 6, 10, 6),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            pnlTop.Controls.Add(flpMenu);

            // PANEL CENTRAL
            pnlContenido = new Panel
            {
                Dock = DockStyle.Fill
            };
            pnlContenido.Paint += PnlContenido_Paint;
            Controls.Add(pnlContenido);

            // Cargar datos guardados de usuario
            CargarConfiguracionUsuario();

            // Pantalla de bienvenida
            MostrarHome();

            // ====== ITEMS DE MENÚ ======
            flpMenu.Controls.Add(CrearItemMenu("Pacientes",
                Properties.Resources.paciente, (s, e) => SeleccionarPacientes()));

            // AHORA CONSULTAS MUESTRA TABLA GENERAL
            flpMenu.Controls.Add(CrearItemMenu("Consultas",
                Properties.Resources.consultoria, (s, e) => MostrarConsultasPacientes()));

            flpMenu.Controls.Add(CrearItemMenu("Plantillas",
                Properties.Resources.plantilla, (s, e) => MostrarEnConstruccion("Plantillas")));

            flpMenu.Controls.Add(CrearItemMenu("Agenda",
                Properties.Resources.agenda, (s, e) => MostrarEnConstruccion("Agenda")));

            flpMenu.Controls.Add(CrearItemMenu("Items",
                Properties.Resources.estante, (s, e) => MostrarEnConstruccion("Items")));

            flpMenu.Controls.Add(CrearItemMenu("Proformas",
                Properties.Resources.facturas, (s, e) => MostrarEnConstruccion("Proformas")));

            flpMenu.Controls.Add(CrearItemMenu("Fichas",
                Properties.Resources.Fichas, (s, e) => MostrarEnConstruccion("Fichas")));

            flpMenu.Controls.Add(CrearItemMenu("Apps",
                Properties.Resources.Apps, (s, e) => MostrarEnConstruccion("Apps")));

            flpMenu.Controls.Add(CrearItemMenu("Configur.",
                Properties.Resources.ajustes, BtnConfiguracion_Click));

            flpMenu.Controls.Add(CrearItemMenu("Info",
                Properties.Resources.informacion, (s, e) => MostrarEnConstruccion("Información")));

            flpMenu.Controls.Add(CrearItemMenu("Finalizar",
                Properties.Resources.borrar, (s, e) => Close()));
        }

        // Fondo
        private void PnlContenido_Paint(object sender, PaintEventArgs e)
        {
            var rect = pnlContenido.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return;

            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(30, 30, 36),
                Color.FromArgb(18, 18, 24),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        // ======================= MENÚ VISUAL =======================

        private Panel CrearItemMenu(string texto, Image icono, EventHandler onClick)
        {
            var panel = new Panel
            {
                Width = 84,
                Height = 64,
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            var card = new Panel
            {
                Width = 46,
                Height = 46,
                Top = 2,
                Left = (panel.Width - 46) / 2,
                BackColor = Color.Transparent,
                Tag = false
            };

            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool hovered = (bool)(card.Tag ?? false);

                Color baseColor = Color.FromArgb(52, 152, 219);
                Color hoverColor = Color.FromArgb(64, 179, 255);
                Color fill = hovered ? hoverColor : baseColor;

                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);

                using (var brush = new SolidBrush(fill))
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }
            };

            var pic = new PictureBox
            {
                Image = icono,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 24,
                Height = 24,
                Left = (card.Width - 24) / 2,
                Top = (card.Height - 24) / 2,
                BackColor = Color.Transparent
            };
            card.Controls.Add(pic);

            var lbl = new Label
            {
                Text = texto,
                Dock = DockStyle.Bottom,
                Height = 18,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8f),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(card);
            panel.Controls.Add(lbl);

            void ClickHandler(object s, EventArgs e) => onClick(s, e);
            panel.Click += ClickHandler;
            card.Click += ClickHandler;
            pic.Click += ClickHandler;
            lbl.Click += ClickHandler;

            void HoverOn(object s, EventArgs e)
            {
                card.Tag = true;
                card.Invalidate();
            }
            void HoverOff(object s, EventArgs e)
            {
                card.Tag = false;
                card.Invalidate();
            }

            panel.MouseEnter += HoverOn;
            panel.MouseLeave += HoverOff;
            card.MouseEnter += HoverOn;
            card.MouseLeave += HoverOff;
            pic.MouseEnter += HoverOn;
            pic.MouseLeave += HoverOff;
            lbl.MouseEnter += HoverOn;
            lbl.MouseLeave += HoverOff;

            return panel;
        }

        private void MostrarHome()
        {
            pnlContenido.Controls.Clear();
            pnlContenido.Invalidate();

            avatarPanel = new Panel
            {
                Width = 180,
                Height = 180,
                Left = 40,
                Top = 110,
                BackColor = Color.Transparent
            };

            picAvatar = new PictureBox
            {
                Left = 8,
                Top = 8,
                Width = avatarPanel.Width - 16,
                Height = avatarPanel.Height - 16,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            if (usuarioImagen != null)
            {
                picAvatar.Image = usuarioImagen;
            }

            avatarPanel.Controls.Add(picAvatar);
            pnlContenido.Controls.Add(avatarPanel);

            avatarPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(
                    picAvatar.Left - 2,
                    picAvatar.Top - 2,
                    picAvatar.Width + 4,
                    picAvatar.Height + 4);

                using (var pen = new Pen(Color.White, 4))
                {
                    e.Graphics.DrawEllipse(pen, rect);
                }
            };

            lblUser = new Label
            {
                Text = usuarioNombre,
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Segoe UI", 10f),
                Left = avatarPanel.Left + 20,
                Top = avatarPanel.Bottom + 8,
                BackColor = Color.Transparent
            };
            pnlContenido.Controls.Add(lblUser);

            var lblTitulo = new Label
            {
                Text = "Bienvenido al Sistema del Consultorio",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                AutoSize = true,
                Left = 320,
                Top = 140,
                BackColor = Color.Transparent
            };
            pnlContenido.Controls.Add(lblTitulo);
        }

        // ======================= ACCIONES DE MENÚ =======================

        private void SeleccionarPacientes()
        {
            flpMenu.Visible = false;
            btnNuevoPaciente.Visible = true;
            btnBackPacientes.Visible = true;

            pnlContenido.Controls.Clear();

            var frm = new FrmPacientes(soloListado: true)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            frm.PacienteSeleccionado += Frm_PacienteSeleccionado;
            pnlContenido.Controls.Add(frm);
            frm.Show();
        }

        // ======================= CONSULTAS (MENÚ PRINCIPAL) =======================

        private void MostrarConsultasPacientes()
        {
            flpMenu.Visible = true;
            btnNuevoPaciente.Visible = false;
            btnBackPacientes.Visible = false;

            pnlContenido.Controls.Clear();

            // Márgenes alrededor del contenido
            pnlContenido.Padding = new Padding(40, 80, 40, 40);
            pnlContenido.Invalidate();

            // ===== GRID =====
            var dgv = new DataGridView
            {
                Name = "dgvConsultasPacientes",
                Dock = DockStyle.Fill,                  
                ReadOnly = true,
                AllowUserToAddRows = false,
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
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(70, 70, 75);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 85);

            // ===== TÍTULO =====
            var lblTitulo = new Label
            {
                Text = "Consultas de pacientes",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)  
            };


            // IMPORTANTE: primero se agrega el grid (Fill) y luego el título (Top)
            pnlContenido.Controls.Add(dgv);
            pnlContenido.Controls.Add(lblTitulo);

            // ===== CARGAR DATOS =====
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT 
                    c.Id,
                    c.PacienteId,
                    p.Nombre AS Paciente,
                    DATE_FORMAT(c.FechaConsulta, '%d/%m/%Y') AS Fecha,
                    DATE_FORMAT(c.FechaConsulta, '%H:%i') AS Hora,
                    c.Motivo,
                    c.Diagnostico
                FROM Consulta c
                INNER JOIN Paciente p ON c.PacienteId = p.Id
                ORDER BY c.FechaConsulta DESC;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }
                }

                AjustarColumnasConsultas(dgv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar consultas:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Doble clic => historial del paciente
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                var row = dgv.Rows[e.RowIndex];
                var valIdPac = row.Cells["PacienteId"].Value;
                var valNombre = row.Cells["Paciente"].Value;

                if (valIdPac == null || valNombre == null) return;
                if (!int.TryParse(valIdPac.ToString(), out var pacienteId)) return;

                string nombrePaciente = valNombre.ToString();

                using (var frm = new FrmConsultasPaciente(pacienteId, nombrePaciente))
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(this);
                }
            };
        }




        private void AjustarColumnasConsultas(DataGridView dgv)
        {
            if (dgv.Columns.Count == 0) return;

            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgv.Columns["Id"] != null)
                dgv.Columns["Id"].Visible = false;
            if (dgv.Columns["PacienteId"] != null)
                dgv.Columns["PacienteId"].Visible = false;

            if (dgv.Columns["Paciente"] != null)
                dgv.Columns["Paciente"].HeaderText = "Paciente";
            if (dgv.Columns["Fecha"] != null)
                dgv.Columns["Fecha"].HeaderText = "Fecha";
            if (dgv.Columns["Hora"] != null)
                dgv.Columns["Hora"].HeaderText = "Hora";
            if (dgv.Columns["Motivo"] != null)
                dgv.Columns["Motivo"].HeaderText = "Motivo de consulta";
            if (dgv.Columns["Diagnostico"] != null)
                dgv.Columns["Diagnostico"].HeaderText = "Diagnóstico";

            // Proporciones de ancho
            if (dgv.Columns["Paciente"] != null)
                dgv.Columns["Paciente"].FillWeight = 25;
            if (dgv.Columns["Fecha"] != null)
                dgv.Columns["Fecha"].FillWeight = 10;
            if (dgv.Columns["Hora"] != null)
                dgv.Columns["Hora"].FillWeight = 8;
            if (dgv.Columns["Motivo"] != null)
                dgv.Columns["Motivo"].FillWeight = 28;
            if (dgv.Columns["Diagnostico"] != null)
                dgv.Columns["Diagnostico"].FillWeight = 29;

            if (dgv.Columns["Fecha"] != null)
                dgv.Columns["Fecha"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (dgv.Columns["Hora"] != null)
                dgv.Columns["Hora"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }



        private void MostrarEnConstruccion(string modulo)
        {
            flpMenu.Visible = true;
            btnNuevoPaciente.Visible = false;
            btnBackPacientes.Visible = false;

            pnlContenido.Controls.Clear();
            pnlContenido.Invalidate();

            var lbl = new Label
            {
                Text = $"{modulo} - módulo en construcción",
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 18f, FontStyle.Italic),
                AutoSize = true,
                Left = 60,
                Top = 80,
                BackColor = Color.Transparent
            };
            pnlContenido.Controls.Add(lbl);
        }

        // ======================= NUEVO PACIENTE / VOLVER =======================

        private void BtnNuevoPaciente_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmPacientes())
            {
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog();
            }

            SeleccionarPacientes();
        }

        private void BtnBackPacientes_Click(object sender, EventArgs e)
        {
            flpMenu.Visible = true;
            btnNuevoPaciente.Visible = false;
            btnBackPacientes.Visible = false;

            MostrarHome();
        }

        private void Frm_PacienteSeleccionado(int pacienteId)
        {
            using (var frm = new FrmPacienteMenu(pacienteId))
            {
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog(this);
            }
        }

        private void BtnConfiguracion_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmConfiguracionUsuario(usuarioNombre, usuarioImagen))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    usuarioNombre = frm.NombreUsuario;
                    usuarioImagen = frm.FotoSeleccionada;

                    if (lblUser != null)
                        lblUser.Text = usuarioNombre;
                    if (picAvatar != null)
                        picAvatar.Image = usuarioImagen;

                    GuardarConfiguracionUsuario();
                }
            }
        }

        // ======================= CONFIG USUARIO =======================

        private string PedirNombreUsuario(string mensaje, string titulo, string valorPorDefecto)
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                form.Text = titulo;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ClientSize = new Size(420, 140);
                form.BackColor = Color.White;

                lbl.Text = mensaje;
                lbl.Left = 10;
                lbl.Top = 10;
                lbl.AutoSize = true;

                txt.Left = 10;
                txt.Top = 40;
                txt.Width = 390;
                txt.Text = valorPorDefecto ?? "";

                btnOk.Text = "Aceptar";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Left = 220;
                btnOk.Width = 80;
                btnOk.Top = 80;

                btnCancel.Text = "Cancelar";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Left = 320;
                btnCancel.Width = 80;
                btnCancel.Top = 80;

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                var result = form.ShowDialog(this);
                return result == DialogResult.OK ? txt.Text : null;
            }
        }

        private void CargarConfiguracionUsuario()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT NombreUsuario, Foto FROM UsuarioConfig WHERE Id = 1;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            if (!rd.IsDBNull(0))
                                usuarioNombre = rd.GetString(0);

                            if (!rd.IsDBNull(1))
                            {
                                var bytes = (byte[])rd[1];
                                if (bytes.Length > 0)
                                {
                                    using (var ms = new MemoryStream(bytes))
                                    {
                                        usuarioImagen = Image.FromStream(ms);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignorar errores de config
            }
        }

        private void GuardarConfiguracionUsuario()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                INSERT INTO UsuarioConfig (Id, NombreUsuario, Foto)
                VALUES (1, @NombreUsuario, @Foto)
                ON DUPLICATE KEY UPDATE
                    NombreUsuario = VALUES(NombreUsuario),
                    Foto = VALUES(Foto);";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@NombreUsuario", usuarioNombre ?? "");

                        byte[] bytesFoto = null;
                        if (usuarioImagen != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                usuarioImagen.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                bytesFoto = ms.ToArray();
                            }
                        }

                        if (bytesFoto != null && bytesFoto.Length > 0)
                            cmd.Parameters.Add("@Foto", MySqlDbType.LongBlob).Value = bytesFoto;
                        else
                            cmd.Parameters.Add("@Foto", MySqlDbType.LongBlob).Value = DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
            }
        }
    }
}
