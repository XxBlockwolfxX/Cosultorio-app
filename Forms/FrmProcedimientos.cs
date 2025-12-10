using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using ConsultorioDentalApp.Forms;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmProcedimientos : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvPacientes;

        public FrmProcedimientos()
        {
            InicializarComponentesPersonalizados();
            CargarPacientes();
        }

        private void InicializarComponentesPersonalizados()
        {
            this.Text = "Gestión de Procedimientos";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 950;
            this.Height = 600;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            // ======== TÍTULO ========
            Label lblTitulo = new Label()
            {
                Text = "Lista de Pacientes",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 150),
                AutoSize = true,
                Top = 20,
                Left = 330
            };
            Controls.Add(lblTitulo);

            // ======== BÚSQUEDA ========
            Label lblBuscar = new Label()
            {
                Text = "Buscar:",
                Left = 20,
                Top = 80,
                AutoSize = true
            };
            Controls.Add(lblBuscar);

            txtBuscar = new TextBox()
            {
                Left = 90,
                Top = 75,
                Width = 300
            };
            txtBuscar.TextChanged += (s, e) => FiltrarPacientes(txtBuscar.Text);
            Controls.Add(txtBuscar);

            // ======== TABLA ========
            dgvPacientes = new DataGridView()
            {
                Left = 20,
                Top = 120,
                Width = 900,
                Height = 400,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvPacientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 255);
            dgvPacientes.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPacientes.CellContentClick += DgvPacientes_CellContentClick;
            Controls.Add(dgvPacientes);
        }

        // ===========================================================
        //             CARGAR PACIENTES (MYSQL)
        // ===========================================================
        private void CargarPacientes()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                var da = new MySqlDataAdapter(
                    "SELECT Id, Nombre, Edad, Sexo FROM Paciente ORDER BY Nombre ASC",
                    conn
                );

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvPacientes.Columns.Clear();
                dgvPacientes.DataSource = dt;

                // Botón
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
                {
                    Text = "🦷 Ver Tarjeta Terapéutica",
                    UseColumnTextForButtonValue = true,
                    HeaderText = "Acción",
                    Name = "btnVer",
                    FlatStyle = FlatStyle.Flat
                };

                dgvPacientes.Columns.Add(btnCol);
            }
        }

        // ===========================================================
        //                 FILTRAR PACIENTES (MYSQL)
        // ===========================================================
        private void FiltrarPacientes(string texto)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                var da = new MySqlDataAdapter(
                    "SELECT Id, Nombre, Edad, Sexo FROM Paciente WHERE Nombre LIKE @texto",
                    conn
                );

                da.SelectCommand.Parameters.AddWithValue("@texto", "%" + texto + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvPacientes.Columns.Clear();
                dgvPacientes.DataSource = dt;

                if (!dgvPacientes.Columns.Contains("btnVer"))
                {
                    DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
                    {
                        Text = "🦷 Ver Tarjeta Terapéutica",
                        UseColumnTextForButtonValue = true,
                        HeaderText = "Acción",
                        Name = "btnVer",
                        FlatStyle = FlatStyle.Flat
                    };

                    dgvPacientes.Columns.Add(btnCol);
                }
            }
        }

        // ===========================================================
        //                EVENTO CLICK BOTÓN
        // ===========================================================
        private void DgvPacientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvPacientes.Columns[e.ColumnIndex].Name == "btnVer")
            {
                int pacienteId = Convert.ToInt32(dgvPacientes.Rows[e.RowIndex].Cells["Id"].Value);
                FrmTarjetaTerapéutica tarjeta = new FrmTarjetaTerapéutica(pacienteId);
                tarjeta.ShowDialog();
            }
        }
    }
}
