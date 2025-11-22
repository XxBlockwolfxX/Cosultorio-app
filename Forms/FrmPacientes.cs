using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmPacientes : Form
    {
        TextBox txtNombre, txtEdad, txtTelefono, txtDireccion, txtCorreo;
        ComboBox cmbSexo, cmbEstadoCivil;
        DataGridView dgvPacientes;
        Button btnAgregar;
        Panel pnlFormulario;

        public FrmPacientes()
        {
            InicializarComponentesPersonalizados();
            CargarPacientes();
        }

        private void InicializarComponentesPersonalizados()
        {
            // === FORM ===
            this.Text = "Gestión de Pacientes";
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 950;
            this.Height = 650;

            // === TÍTULO ===
            Label lblTitulo = new Label()
            {
                Text = "Pacientes Registrados",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 75, 125),
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblTitulo);

            // === PANEL DE FORMULARIO ===
            pnlFormulario = new Panel()
            {
                Left = 30,
                Top = 70,
                Width = 870,
                Height = 160,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlFormulario);

            int xLabel = 20, xField = 120, yBase = 20, sepY = 35;

            // ======= NOMBRE =======
            pnlFormulario.Controls.Add(CrearLabel("Nombre:", xLabel, yBase));
            txtNombre = CrearTextBox(xField, yBase);

            // ======= SEXO =======
            pnlFormulario.Controls.Add(CrearLabel("Sexo:", 450, yBase));
            cmbSexo = new ComboBox()
            {
                Left = 520,
                Top = yBase - 3,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSexo.Items.AddRange(new string[] { "Masculino", "Femenino", "Otro" });
            pnlFormulario.Controls.Add(cmbSexo);

            // ======= EDAD =======
            pnlFormulario.Controls.Add(CrearLabel("Edad:", xLabel, yBase + sepY));
            txtEdad = CrearTextBox(xField, yBase + sepY, 80);

            // ======= ESTADO CIVIL =======
            pnlFormulario.Controls.Add(CrearLabel("Estado Civil:", 450, yBase + sepY));
            cmbEstadoCivil = new ComboBox()
            {
                Left = 550,
                Top = yBase + sepY - 3,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoCivil.Items.AddRange(new string[] { "Soltero", "Casado", "Divorciado", "Unión libre" });
            pnlFormulario.Controls.Add(cmbEstadoCivil);

            // ======= TELÉFONO =======
            pnlFormulario.Controls.Add(CrearLabel("Teléfono:", xLabel, yBase + sepY * 2));
            txtTelefono = CrearTextBox(xField, yBase + sepY * 2);

            // ======= CORREO =======
            pnlFormulario.Controls.Add(CrearLabel("Correo:", 450, yBase + sepY * 2));
            txtCorreo = CrearTextBox(520, yBase + sepY * 2, 250);

            // ======= DIRECCIÓN =======
            pnlFormulario.Controls.Add(CrearLabel("Dirección:", xLabel, yBase + sepY * 3));
            txtDireccion = CrearTextBox(xField, yBase + sepY * 3, 650);

            // === BOTÓN AGREGAR ===
            btnAgregar = new Button()
            {
                Text = "Agregar Paciente",
                BackColor = Color.FromArgb(45, 150, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Width = 180,
                Height = 35,
                Left = 350,
                Top = pnlFormulario.Bottom + 15
            };
            btnAgregar.Click += BtnAgregar_Click;
            this.Controls.Add(btnAgregar);

            // === DATAGRID ===
            dgvPacientes = new DataGridView()
            {
                Left = 30,
                Top = btnAgregar.Bottom + 20,
                Width = 870,
                Height = 300,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(dgvPacientes);
        }

        private void dgvPacientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int id = Convert.ToInt32(dgvPacientes.Rows[e.RowIndex].Cells["Id"].Value);
            new FrmFichaClinica(id).ShowDialog();
        }


        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label()
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                ForeColor = Color.FromArgb(40, 75, 125)
            };
        }

        private TextBox CrearTextBox(int x, int y, int width = 200)
        {
            var txt = new TextBox()
            {
                Left = x,
                Top = y - 3,
                Width = width,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFormulario.Controls.Add(txt);
            return txt;
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    var cmd = new SqlCommand(@"
INSERT INTO Paciente (Nombre, Edad, Sexo, EstadoCivil, Telefono, Correo, Direccion)
VALUES (@Nombre, @Edad, @Sexo, @EstadoCivil, @Telefono, @Correo, @Direccion)", conn);

                    cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                    cmd.Parameters.AddWithValue("@Edad", txtEdad.Text);
                    cmd.Parameters.AddWithValue("@Sexo", cmbSexo.SelectedItem?.ToString() ?? "");
                    cmd.Parameters.AddWithValue("@EstadoCivil", cmbEstadoCivil.SelectedItem?.ToString() ?? "");
                    cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text);
                    cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text);
                    cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Paciente agregado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarPacientes();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar paciente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtEdad.Clear();
            txtTelefono.Clear();
            txtDireccion.Clear();
            txtCorreo.Clear();
            cmbSexo.SelectedIndex = -1;
            cmbEstadoCivil.SelectedIndex = -1;
        }

        private void CargarPacientes()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM Paciente", conn);
                var dt = new DataTable();
                da.Fill(dt);
                dgvPacientes.DataSource = dt;
            }
        }
    }
}
