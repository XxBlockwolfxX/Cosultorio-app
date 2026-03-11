using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Services;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmAgendaCalendar : Form
    {
        private DateTime semanaInicio;

        // Top bar
        private Panel pnlTop;
        private Label lblSemana;
        private Button btnPrev, btnHoy, btnNext;

        // Body
        private TableLayoutPanel body;
        private Panel pnlInfo;
        private Panel pnlCalendarHost;
        private BorderedPanel pnlCalendarCard;
        private DataGridView dgv;

        // Info labels (vacíos al inicio)
        private Label lblInfoTitulo;
        private Label lblInfoDia;
        private Label lblInfoHora;
        private Label lblInfoPaciente;
        private Label lblInfoTratamiento;

        // Config
        private AgendaConfig cfg;
        private int[] diasVisibles; // 1=Lun..7=Dom

        public FrmAgendaCalendar()
        {
            cfg = AgendaConfigService.Cargar();
            diasVisibles = (cfg?.DiasLaborales != null && cfg.DiasLaborales.Count > 0)
                ? cfg.DiasLaborales.OrderBy(x => x).ToArray()
                : new[] { 1, 2, 3, 4, 5, 6 };

            Text = "Agenda (Calendario)";
            BackColor = Color.FromArgb(20, 20, 24);
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            semanaInicio = InicioDeSemana(DateTime.Today);

            BuildUI();

            // ✅ eventos
            dgv.CellClick += Dgv_CellClick;

            ConstruirGrid();
            CargarSemana();
            LimpiarInfo(); // ✅ panel info vacío al inicio
        }

        private void BuildUI()
        {
            // ===== ROOT LAYOUT (Top + Body) =====
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = BackColor
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // top bar
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // body
            Controls.Add(root);

            // ===== TOP BAR =====
            pnlTop = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 14, 18, 10),
                BackColor = Color.FromArgb(20, 20, 24)
            };
            root.Controls.Add(pnlTop, 0, 0);

            btnPrev = CrearBotonCircular("‹");
            btnHoy = CrearBotonCircular("●");
            btnNext = CrearBotonCircular("›");

            btnPrev.Click += (s, e) => { semanaInicio = semanaInicio.AddDays(-7); CargarSemana(); LimpiarInfo(); };
            btnNext.Click += (s, e) => { semanaInicio = semanaInicio.AddDays(7); CargarSemana(); LimpiarInfo(); };
            btnHoy.Click += (s, e) => { semanaInicio = InicioDeSemana(DateTime.Today); CargarSemana(); LimpiarInfo(); };

            var flowTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 6, 0, 0),
                BackColor = pnlTop.BackColor
            };
            flowTop.Controls.Add(btnPrev);
            flowTop.Controls.Add(btnHoy);
            flowTop.Controls.Add(btnNext);
            pnlTop.Controls.Add(flowTop);

            lblSemana = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Left = flowTop.Right + 20,
                Top = 24
            };
            pnlTop.Controls.Add(lblSemana);

            // ===== BODY: left info + right calendar =====
            body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(18, 10, 18, 18),
                BackColor = BackColor
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240)); // Info
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // Calendar
            root.Controls.Add(body, 0, 1);

            // ===== INFO PANEL (izquierda) =====
            pnlInfo = new BorderedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 12,
                BorderColor = Color.FromArgb(90, 90, 95),
                BorderThickness = 2,
                BackColor = Color.FromArgb(30, 30, 36),
                Padding = new Padding(14)
            };
            body.Controls.Add(pnlInfo, 0, 0);

            lblInfoTitulo = new Label
            {
                Text = "Información",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 28
            };
            pnlInfo.Controls.Add(lblInfoTitulo);

            // ✅ Estos van VACÍOS y se llenan al dar click en una cita
            lblInfoDia = new Label
            {
                Text = "",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlInfo.Controls.Add(lblInfoDia);

            lblInfoHora = new Label
            {
                Text = "",
                ForeColor = Color.Gainsboro,
                Dock = DockStyle.Top,
                Height = 24
            };
            pnlInfo.Controls.Add(lblInfoHora);

            lblInfoPaciente = new Label
            {
                Text = "",
                ForeColor = Color.Gainsboro,
                Dock = DockStyle.Top,
                Height = 40
            };
            pnlInfo.Controls.Add(lblInfoPaciente);

            lblInfoTratamiento = new Label
            {
                Text = "",
                ForeColor = Color.Gainsboro,
                Dock = DockStyle.Top,
                Height = 60
            };
            pnlInfo.Controls.Add(lblInfoTratamiento);

            // ===== CALENDAR HOST (derecha) =====
            pnlCalendarHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(14, 0, 0, 0)
            };
            body.Controls.Add(pnlCalendarHost, 1, 0);

            pnlCalendarCard = new BorderedPanel
            {
                Dock = DockStyle.Fill,
                Radius = 14,
                BorderColor = Color.FromArgb(90, 90, 95),
                BorderThickness = 2,
                BackColor = Color.FromArgb(30, 30, 36),
                Padding = new Padding(12)
            };
            pnlCalendarHost.Controls.Add(pnlCalendarCard);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                BackgroundColor = Color.FromArgb(45, 48, 58),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = true,
                RowHeadersWidth = 70,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 40);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 42;

            dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 40);
            dgv.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.RowHeadersDefaultCellStyle.Padding = new Padding(2, 0, 0, 0);

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            pnlCalendarCard.Controls.Add(dgv);
        }

        private void LimpiarInfo()
        {
            lblInfoDia.Text = "";
            lblInfoHora.Text = "";
            lblInfoPaciente.Text = "";
            lblInfoTratamiento.Text = "";
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var info = cell.Tag as CitaInfo;

            if (info == null)
            {
                LimpiarInfo();
                return;
            }

            string diaTxt = FormatoDia(info.FechaHora);
            lblInfoDia.Text = $"Día: {diaTxt}";
            lblInfoHora.Text = $"Hora: {info.FechaHora:HH:mm}";
            lblInfoPaciente.Text = $"Paciente: {info.Paciente}";
            lblInfoTratamiento.Text = $"Tratamiento: {info.Motivo}";
        }

        private string FormatoDia(DateTime dt)
        {
            // "Lunes 13 Ene"
            // (si tu Windows ya está en español, esto funciona igual)
            var cultura = new CultureInfo("es-EC");

            string dia = cultura.DateTimeFormat.GetDayName(dt.DayOfWeek);
            dia = char.ToUpper(dia[0]) + dia.Substring(1);

            string mes = cultura.DateTimeFormat.GetAbbreviatedMonthName(dt.Month);
            mes = mes.Replace(".", "");
            mes = char.ToUpper(mes[0]) + mes.Substring(1);

            return $"{dia} {dt.Day} {mes}";
        }

        private Button CrearBotonCircular(string texto)
        {
            var b = new Button
            {
                Text = texto,
                Width = 44,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 48, 58),
                Margin = new Padding(0, 0, 10, 0),
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 2;
            b.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 95);

            b.Resize += (s, e) => HacerCircular(b);
            HacerCircular(b);
            return b;
        }

        private void HacerCircular(Control c)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, c.Width - 1, c.Height - 1);
                c.Region = new Region(path);
            }
        }

        private void ConstruirGrid()
        {
            if (cfg == null) cfg = AgendaConfigService.Cargar();

            diasVisibles = (cfg.DiasLaborales != null && cfg.DiasLaborales.Count > 0)
                ? cfg.DiasLaborales.OrderBy(x => x).ToArray()
                : new[] { 1, 2, 3, 4, 5, 6 };

            dgv.Columns.Clear();
            dgv.Rows.Clear();

            for (int i = 0; i < diasVisibles.Length; i++)
                dgv.Columns.Add($"D{i}", "");

            int step = Math.Max(5, cfg.IntervaloMin);

            for (var t = cfg.HoraInicio; t <= cfg.HoraFin; t = t.Add(TimeSpan.FromMinutes(step)))
            {
                int idx = dgv.Rows.Add();
                dgv.Rows[idx].HeaderCell.Value = DateTime.Today.Add(t).ToString("HH:mm");
                dgv.Rows[idx].Height = 58;
            }
        }

        private void CargarSemana()
        {
            if (cfg == null) cfg = AgendaConfigService.Cargar();

            string[] nombres = { "", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };

            for (int i = 0; i < diasVisibles.Length; i++)
            {
                int dia = diasVisibles[i];
                var fecha = semanaInicio.AddDays(dia - 1);
                dgv.Columns[i].HeaderText = $"{nombres[dia]} {fecha:dd/MM}";
            }

            var desde = semanaInicio.AddDays(diasVisibles.First() - 1);
            var hasta = semanaInicio.AddDays(diasVisibles.Last() - 1);
            lblSemana.Text = $"{desde:dd/MM/yyyy}  -  {hasta:dd/MM/yyyy}";

            foreach (DataGridViewRow r in dgv.Rows)
                foreach (DataGridViewCell c in r.Cells)
                {
                    c.Value = "";
                    c.Tag = null; // ✅ importante
                    c.Style.BackColor = dgv.DefaultCellStyle.BackColor;
                    c.Style.ForeColor = dgv.DefaultCellStyle.ForeColor;
                }

            var dt = ObtenerCitasDeSemana(semanaInicio, semanaInicio.AddDays(7));
            int step = Math.Max(5, cfg.IntervaloMin);

            foreach (DataRow row in dt.Rows)
            {
                DateTime fechaHora = Convert.ToDateTime(row["FechaHora"]);
                string paciente = row["Paciente"].ToString();
                string motivo = row["Motivo"].ToString();

                int dow = (int)fechaHora.DayOfWeek;
                int dia = (dow == 0) ? 7 : dow;

                int col = Array.IndexOf(diasVisibles, dia);
                if (col < 0) continue;

                TimeSpan ts = fechaHora.TimeOfDay;
                if (ts < cfg.HoraInicio || ts > cfg.HoraFin) continue;

                int mins = (int)(ts - cfg.HoraInicio).TotalMinutes;
                int fila = mins / step;

                if (fila < 0 || fila >= dgv.Rows.Count) continue;

                var cell = dgv.Rows[fila].Cells[col];

                // ✅ guardar info para el panel
                cell.Tag = new CitaInfo
                {
                    FechaHora = fechaHora,
                    Paciente = paciente,
                    Motivo = motivo
                };

                // ✅ lo que se ve en el calendario
                cell.Value = $"{paciente}\n{fechaHora:HH:mm}  {motivo}";

                if (fechaHora < DateTime.Now)
                    cell.Style.BackColor = Color.FromArgb(231, 76, 60);
                else if (fechaHora.Date == DateTime.Today)
                    cell.Style.BackColor = Color.FromArgb(46, 204, 113);
                else if (fechaHora.Date == DateTime.Today.AddDays(1))
                    cell.Style.BackColor = Color.FromArgb(241, 196, 15);
                else
                    cell.Style.BackColor = Color.FromArgb(80, 80, 85);

                bool claro = cell.Style.BackColor == Color.FromArgb(241, 196, 15) ||
                             cell.Style.BackColor == Color.FromArgb(46, 204, 113);

                cell.Style.ForeColor = claro ? Color.Black : Color.White;
            }
        }

        private DataTable ObtenerCitasDeSemana(DateTime desde, DateTime hasta)
        {
            var dt = new DataTable();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        a.Id,
                        a.FechaHora,
                        p.Nombre AS Paciente,
                        a.Motivo,
                        a.Estado
                    FROM CitaAgenda a
                    INNER JOIN Paciente p ON p.Id = a.PacienteId
                    WHERE a.FechaHora >= @desde AND a.FechaHora < @hasta
                    ORDER BY a.FechaHora ASC;";

                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@desde", desde);
                    da.SelectCommand.Parameters.AddWithValue("@hasta", hasta);
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DateTime InicioDeSemana(DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }

        public void RecargarConfig()
        {
            cfg = AgendaConfigService.Cargar();
            diasVisibles = (cfg.DiasLaborales != null && cfg.DiasLaborales.Count > 0)
                ? cfg.DiasLaborales.OrderBy(x => x).ToArray()
                : new[] { 1, 2, 3, 4, 5, 6 };

            ConstruirGrid();
            CargarSemana();
            LimpiarInfo();
        }

        // ✅ clase para guardar en cell.Tag
        private class CitaInfo
        {
            public DateTime FechaHora { get; set; }
            public string Paciente { get; set; }
            public string Motivo { get; set; }
        }
    }

    // Panel con borde y esquinas redondeadas
    public class BorderedPanel : Panel
    {
        public int Radius { get; set; } = 12;
        public int BorderThickness { get; set; } = 2;
        public Color BorderColor { get; set; } = Color.FromArgb(90, 90, 95);

        public BorderedPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (var path = RoundedRect(rect, Radius))
            using (var pen = new Pen(BorderColor, BorderThickness))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
