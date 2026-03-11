using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public class FrmProformasPaciente : Form
    {
        private readonly int _pacienteId;
        private readonly string _nombrePaciente;

        private Label lblTitulo;
        private DataGridView dgvProformas;
        private Button btnNueva;
        private Button btnEliminar;


        public FrmProformasPaciente(int pacienteId, string nombrePaciente)
        {
            _pacienteId = pacienteId;
            _nombrePaciente = nombrePaciente;

            InitializeComponent();
            CargarProformas();
        }

        private void InitializeComponent()
        {
            Text = "Proformas del paciente";
            BackColor = Color.FromArgb(20, 20, 24);
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            // ===== TÍTULO =====
            lblTitulo = new Label
            {
                Text = $"Proformas - {_nombrePaciente}",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitulo);

            // ===== PANEL INFERIOR (BOTONES) =====
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                Padding = new Padding(16, 4, 16, 4),
                BackColor = Color.FromArgb(20, 20, 24)
            };
            Controls.Add(panelBottom);

            btnNueva = new Button
            {
                Text = "Nueva proforma",
                Width = 160,
                Height = 36,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNueva.FlatAppearance.BorderSize = 0;
            btnNueva.Click += BtnNueva_Click;

            btnEliminar = new Button
            {
                Text = "Eliminar seleccionada",
                Width = 190,
                Height = 36,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += BtnEliminar_Click;

            // botones dentro del panel inferior
            btnNueva.Location = new Point(16, 4);
            btnEliminar.Location = new Point(panelBottom.Width - btnEliminar.Width - 16, 4);
            btnEliminar.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            panelBottom.Controls.Add(btnNueva);
            panelBottom.Controls.Add(btnEliminar);

            // ===== PANEL CENTRAL (LISTA) =====
            var panelLista = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 60, 16, 10), 
                BackColor = Color.Transparent
            };
            Controls.Add(panelLista);

            // ===== GRID =====
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

            dgvProformas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgvProformas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProformas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgvProformas.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);
            dgvProformas.DefaultCellStyle.ForeColor = Color.White;
            dgvProformas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(70, 70, 75);

            dgvProformas.CellDoubleClick += DgvProformas_CellDoubleClick;

            
            panelLista.Controls.Add(dgvProformas);
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
                            Id,
                            Fecha,
                            Estado,
                            Total,
                            SaldoPendiente
                        FROM Proforma
                        WHERE PacienteId = @PacienteId
                        ORDER BY Fecha DESC;";

                    using (var da = new MySqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@PacienteId", _pacienteId);

                        var dt = new DataTable();
                        da.Fill(dt);
                        dgvProformas.DataSource = dt;
                    }
                }

                if (dgvProformas.Columns["Id"] != null)
                    dgvProformas.Columns["Id"].Visible = false;

                if (dgvProformas.Columns["Fecha"] != null)
                    dgvProformas.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                if (dgvProformas.Columns["Total"] != null)
                    dgvProformas.Columns["Total"].DefaultCellStyle.Format = "N2";

                if (dgvProformas.Columns["SaldoPendiente"] != null)
                    dgvProformas.Columns["SaldoPendiente"].DefaultCellStyle.Format = "N2";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar proformas:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNueva_Click(object sender, EventArgs e)
        {
            int nuevoId;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        INSERT INTO Proforma (PacienteId, Fecha, Estado, Total, SaldoPendiente)
                        VALUES (@PacienteId, @Fecha, 'Pendiente', 0, 0);
                        SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PacienteId", _pacienteId);
                        cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);

                        nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear la proforma:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AbrirEditorProforma(nuevoId);
        }
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProformas.CurrentRow == null)
            {
                MessageBox.Show("Selecciona una proforma para eliminar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var idValue = dgvProformas.CurrentRow.Cells["Id"].Value;
            if (idValue == null)
            {
                MessageBox.Show("No se pudo obtener el Id de la proforma.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int proformaId = Convert.ToInt32(idValue);

            var resp = MessageBox.Show(
                "¿Seguro que deseas eliminar la proforma seleccionada?\n" +
                "Esto eliminará sus detalles y pagos asociados.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resp != DialogResult.Yes)
                return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.AddWithValue("@Id", proformaId);
                        cmd.CommandText = "DELETE FROM ProformaPago WHERE ProformaId = @Id;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM ProformaDetalle WHERE ProformaId = @Id;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM Proforma WHERE Id = @Id;";
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarProformas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la proforma:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvProformas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvProformas.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;

            int id = Convert.ToInt32(row.Cells["Id"].Value);
            AbrirEditorProforma(id);
        }

        private void AbrirEditorProforma(int proformaId)
        {
            using (var frm = new FrmProformaEditar(proformaId, _pacienteId, _nombrePaciente))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    CargarProformas();
                }
            }
        }
    }
}
