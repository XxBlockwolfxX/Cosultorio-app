using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmCieSelector : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvCie;

        public string CodigoSeleccionado { get; private set; }
        public string DescripcionSeleccionada { get; private set; }

        public FrmCieSelector()
        {
            InitializeComponent();
            BuildUI();

            // Cargar datos al abrir
            this.Load += (s, e) => CargarCie("");
        }

        private void BuildUI()
        {
            this.Text = "Códigos CIE";
            this.BackColor = Color.FromArgb(45, 45, 50);
            this.Font = new Font("Segoe UI", 9f);
            this.WindowState = FormWindowState.Maximized;

            // Buscador
            txtBuscar = new TextBox
            {
                Left = 10,
                Top = 10,
                Width = 400,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.TextChanged += (s, e) => CargarCie(txtBuscar.Text);
            this.Controls.Add(txtBuscar);

            // Grid
            dgvCie = new DataGridView
            {
                Left = 10,
                Top = 40,
                Width = this.ClientSize.Width - 20,
                Height = this.ClientSize.Height - 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.FromArgb(60, 60, 65),
                BorderStyle = BorderStyle.None
            };
            dgvCie.CellDoubleClick += DgvCie_CellDoubleClick;
            this.Controls.Add(dgvCie);

            // Estilos de encabezado
            dgvCie.EnableHeadersVisualStyles = false;
            dgvCie.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvCie.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCie.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvCie.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 55);
            dgvCie.DefaultCellStyle.ForeColor = Color.White;
            dgvCie.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgvCie.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCie.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
        }

        private void CargarCie(string filtro)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT 
                code        AS Codigo,
                description AS Descripcion
            FROM cie10";

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    // Aquí usamos los nombres REALES de la tabla: code, description
                    sql += " WHERE code LIKE @filtro OR description LIKE @filtro";
                }

                sql += " ORDER BY code;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(filtro))
                    {
                        cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                    }

                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvCie.DataSource = dt;
                    }
                }
            }

            // A partir de aquí ya trabajas con los alias "Codigo" y "Descripcion"
            if (dgvCie.Columns["Codigo"] != null)
            {
                dgvCie.Columns["Codigo"].HeaderText = "Código";
                dgvCie.Columns["Codigo"].Width = 80;
            }
            if (dgvCie.Columns["Descripcion"] != null)
            {
                dgvCie.Columns["Descripcion"].HeaderText = "Descripción";
                dgvCie.Columns["Descripcion"].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill;
            }
        }


        private void DgvCie_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvCie.Rows[e.RowIndex];
            CodigoSeleccionado = row.Cells["Codigo"].Value?.ToString();
            DescripcionSeleccionada = row.Cells["Descripcion"].Value?.ToString();

            if (!string.IsNullOrEmpty(CodigoSeleccionado))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
