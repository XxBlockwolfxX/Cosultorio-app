using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Forms
{
    public class FrmConsultasPaciente : Form
    {
        private readonly int _pacienteId;
        private readonly string _nombrePaciente;

        private Label lblTitulo;
        private DataGridView dgvConsultas;
        private Button btnNueva;

        public FrmConsultasPaciente(int pacienteId, string nombrePaciente)
        {
            _pacienteId = pacienteId;
            _nombrePaciente = nombrePaciente;

            InitializeComponent();
            CargarConsultas();
        }

        private void InitializeComponent()
        {
            Text = "Consultas del paciente";
            BackColor = Color.FromArgb(40, 40, 45);
            Font = new Font("Segoe UI", 10f);
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 600);
            MaximizeBox = false;

            // ===== PANEL SUPERIOR =====
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(30, 30, 35)
            };

            lblTitulo = new Label
            {
                Text = "CONSULTAS - " + _nombrePaciente,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0),
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(lblTitulo);

            // ===== PANEL INFERIOR CON BOTÓN =====
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(35, 35, 40),
                Padding = new Padding(10, 5, 10, 5)
            };

            btnNueva = new Button
            {
                Text = "Nueva consulta",
                Dock = DockStyle.Left,
                Width = 160,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.Click += BtnNueva_Click;
            pnlBottom.Controls.Add(btnNueva);

            // ===== PANEL CENTRAL (GRID) =====
            var pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 55, 60),
                Padding = new Padding(10, 0, 10, 0)
            };

            dgvConsultas = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.FromArgb(60, 60, 65),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                GridColor = Color.DimGray
            };

            dgvConsultas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgvConsultas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvConsultas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvConsultas.DefaultCellStyle.BackColor = Color.FromArgb(70, 70, 75);
            dgvConsultas.DefaultCellStyle.ForeColor = Color.White;
            dgvConsultas.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgvConsultas.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvConsultas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 85);

            dgvConsultas.CellDoubleClick += DgvConsultas_CellDoubleClick;
            pnlCenter.Controls.Add(dgvConsultas);

            // ===== ORDEN DE DOCK EN EL FORM =====
            Controls.Add(pnlCenter);   // Fill
            Controls.Add(pnlBottom);   // Bottom
            Controls.Add(pnlTop);      // Top
        }

        private void CargarConsultas()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            Id,
                            Motivo,
                            Diagnostico,
                            CodigoCie,
                            DATE_FORMAT(FechaConsulta, '%d/%m/%Y') AS Fecha,
                            DATE_FORMAT(FechaConsulta, '%H:%i') AS Hora
                        FROM Consulta
                        WHERE PacienteId = @Pac
                        ORDER BY FechaConsulta DESC;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@Pac", _pacienteId);

                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvConsultas.DataSource = dt;

                        if (dgvConsultas.Columns["Id"] != null)
                            dgvConsultas.Columns["Id"].Visible = false;

                        if (dgvConsultas.Columns["Motivo"] != null)
                            dgvConsultas.Columns["Motivo"].HeaderText = "Motivo de consulta";
                        if (dgvConsultas.Columns["Diagnostico"] != null)
                            dgvConsultas.Columns["Diagnostico"].HeaderText = "Diagnóstico";
                        if (dgvConsultas.Columns["CodigoCie"] != null)
                            dgvConsultas.Columns["CodigoCie"].HeaderText = "Código CIE";
                        if (dgvConsultas.Columns["Fecha"] != null)
                            dgvConsultas.Columns["Fecha"].HeaderText = "Fecha";
                        if (dgvConsultas.Columns["Hora"] != null)
                            dgvConsultas.Columns["Hora"].HeaderText = "Hora";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar consultas:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmConsultaEdicion(_pacienteId, _nombrePaciente))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CargarConsultas();
            }
        }

        private void DgvConsultas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvConsultas.Rows[e.RowIndex];
            object valId = row.Cells["Id"].Value;
            if (valId == null) return;

            if (!int.TryParse(valId.ToString(), out int consultaId)) return;

            using (var frm = new FrmConsultaEdicion(_pacienteId, _nombrePaciente, consultaId))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CargarConsultas();
            }
        }
    }
}
