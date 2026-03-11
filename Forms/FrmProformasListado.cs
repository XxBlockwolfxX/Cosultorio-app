using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public class FrmProformasListado : Form
    {
        private Label lblTitulo;
        private DataGridView dgvProformas;
        private Panel panelLista;


        public FrmProformasListado()
        {
            InitializeComponent();
            CargarProformas();

        }
            

            private void InitializeComponent()
            {
                Text = "Proformas";
                BackColor = Color.FromArgb(20, 20, 24);
                Font = new Font("Segoe UI", 10f);
                WindowState = FormWindowState.Maximized;

                // Título
                lblTitulo = new Label
                {
                    Text = "Listado de proformas",
                    Dock = DockStyle.Top,
                    Height = 40,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(16, 0, 0, 0),
                    BackColor = Color.Transparent
                };

            // GRID
            dgvProformas = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(45, 45, 50),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvProformas.CellDoubleClick += DgvProformas_CellDoubleClick;


            dgvProformas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
                dgvProformas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvProformas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

                dgvProformas.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
                dgvProformas.DefaultCellStyle.ForeColor = Color.White;
                dgvProformas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(70, 70, 75);

                // PANEL que contiene la lista (para bajarla un poco)
                panelLista = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(16, 50, 16, 16), 
                    BackColor = Color.Transparent
                };
                panelLista.Controls.Add(dgvProformas);

                // Agregar al formulario
                Controls.Add(panelLista);
                Controls.Add(lblTitulo);

                // Orden: primero título, luego lista
                Controls.SetChildIndex(lblTitulo, 0);
            }
        


        private void CargarProformas()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            p.Id,
                            p.PacienteId,
                            p.Fecha,
                            pa.Nombre      AS Paciente,
                            p.Estado,
                            p.Total,
                            p.SaldoPendiente
                        FROM Proforma p
                        INNER JOIN paciente pa ON pa.Id = p.PacienteId
                        -- si quieres ver SOLO pendientes, descomenta la siguiente línea:
                        -- WHERE p.SaldoPendiente > 0
                        ORDER BY p.Fecha DESC;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvProformas.DataSource = dt;
                    }
                }

                // Ocultar IDs
                if (dgvProformas.Columns["Id"] != null)
                    dgvProformas.Columns["Id"].Visible = false;

                if (dgvProformas.Columns["PacienteId"] != null)
                    dgvProformas.Columns["PacienteId"].Visible = false;

                // Encabezados bonitos
                if (dgvProformas.Columns["Fecha"] != null)
                    dgvProformas.Columns["Fecha"].HeaderText = "Fecha";

                if (dgvProformas.Columns["Paciente"] != null)
                    dgvProformas.Columns["Paciente"].HeaderText = "Paciente";

                if (dgvProformas.Columns["Estado"] != null)
                    dgvProformas.Columns["Estado"].HeaderText = "Estado";

                if (dgvProformas.Columns["Total"] != null)
                {
                    dgvProformas.Columns["Total"].HeaderText = "Total factura";
                    dgvProformas.Columns["Total"].DefaultCellStyle.Format = "N2";
                }

                if (dgvProformas.Columns["SaldoPendiente"] != null)
                {
                    dgvProformas.Columns["SaldoPendiente"].HeaderText = "Saldo por pagar";
                    dgvProformas.Columns["SaldoPendiente"].DefaultCellStyle.Format = "N2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las proformas:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvProformas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvProformas.Rows[e.RowIndex];

            if (row.Cells["Id"].Value == null || row.Cells["PacienteId"].Value == null)
                return;

            int proformaId = Convert.ToInt32(row.Cells["Id"].Value);
            int pacienteId = Convert.ToInt32(row.Cells["PacienteId"].Value);
            string paciente = Convert.ToString(row.Cells["Paciente"].Value);

            using (var frm = new FrmProformaEditar(proformaId, pacienteId, paciente))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    // refrescar lista después de guardar cambios
                    CargarProformas();
                }
            }
        }
    }
}
